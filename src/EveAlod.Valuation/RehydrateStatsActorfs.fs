namespace EveAlod.Valuation

open System
open MongoDB.Bson
open MongoDB.Driver
open EveAlod.Common
open EveAlod.Common.Strings
open EveAlod.Common.MongoDb
open EveAlod.Data

type private RehydrateStatsState = 
    {
        Start: DateTime
        TotalRecords: int64        
    } with 
    static member Empty = { Start = DateTime.MinValue; TotalRecords = 0L; }

type RehydrateStatsActor(log: PostMessage, sendShipStats: PostValuationMessage, config: ValuationConfiguration)=

    let logException = (Actors.postException typeof<RehydrateStatsActor>.Name log)
    let logTrace msg = msg |> fun s -> ActorMessage.Trace (typeof<RehydrateStatsActor>.Name, s) |> log
    let logError msg = msg |> fun s -> ActorMessage.Error (typeof<RehydrateStatsActor>.Name, s) |> log
    let logInfo = ActorMessage.Info >> log

    let connection = Mongo.connection config 

    let shipTypeStatsCollection =   
        lazy (
                sprintf "Initialising Rehydrate Stats connection to Mongo %s.%s..." connection.Server connection.DbName |> logInfo
                let result = EveAlod.Common.MongoDb.initCollection connection
                "Rehydrate Stats connect initialisation done." |> logInfo
                result
            )

    let rec parseHeader (elements: BsonElement list) (stats: ShipTypePeriodStatistics) = 
        match elements with
        | [] -> stats
        | h::t -> parseHeader t (match h.Name, h.Value with
                                    | "shipType", v -> { stats with ShipTypeId = v.ToString() } 
                                    | "period", v -> { stats with Period = v.ToUniversalTime() }
                                    | _ -> stats )
    
    let rec parseValues prefix (elements: BsonElement list) (stats: ValueStatistics) = 
        match elements with
        | [] -> stats
        | h::t -> parseValues prefix t (match h.Name with
                                        | "count" -> { stats with Count = h.Value.ToInt64() }                                    
                                        | Split prefix "AverageValue" _ -> { stats with AverageValue = h.Value.ToDouble() }
                                        | Split prefix "MaxValue" _ -> { stats with MaxValue = h.Value.ToDouble() }
                                        | Split prefix "MinValue" _ -> { stats with MinValue = h.Value.ToDouble() }
                                        | Split prefix "MedianValue" _ -> { stats with MedianValue = h.Value.ToDouble() }
                                        | Split prefix "TotalValue" _ -> { stats with TotalValue = h.Value.ToDouble() }
                                        | Split prefix "ValueRange" _ -> { stats with ValueRange = h.Value.ToDouble() }                                    
                                        | _ -> stats )



    let toStatsEntry (doc: BsonDocument)=
        let elements = doc.Elements |> List.ofSeq

        let seed = ShipTypePeriodStatistics.Empty 
        
        { seed with Fitted = parseValues "fitted" elements seed.Fitted;
                    Total = parseValues "total" elements seed.Total } 
            |> parseHeader elements
        
        

    let getFirstCachedItem()=
            async {                
                try
                    use! docs = shipTypeStatsCollection.Value.FindAsync(fun d -> true) |> Async.AwaitTask
                
                    let! doc = docs.FirstOrDefaultAsync() |> Async.AwaitTask
                    return match doc with
                            | null ->   Choice1Of3 None
                            | _ ->      let objectId = Bson.getObjectId doc
                                        Choice2Of3 (objectId, (toStatsEntry doc))                
                with
                | e ->  logException e
                        return Choice3Of3 e
            }
    
    let getNextCachedItem(id: ObjectId) =
            async{
                try
                    let filterJson = sprintf @"{_id: { $gt: ObjectId(""%s"") } }" (id.ToString())
                    let idFieldFilter = new JsonFilterDefinition<BsonDocument>(filterJson)
                                    
                    use! docs = shipTypeStatsCollection.Value.FindAsync(idFieldFilter) |> Async.AwaitTask
                
                    let! doc = docs.FirstOrDefaultAsync() |> Async.AwaitTask
                
                    return match doc with
                            | null ->   Choice1Of3 None
                            | d ->      let objectId = Bson.getObjectId doc
                                        Choice2Of3 (objectId, (toStatsEntry doc))
                with
                | e ->  logException e
                        return Choice3Of3 e
            }
       
    let onGetNext(id: ObjectId option)=
        async {
                
            let! r = match id with
                        | None -> getFirstCachedItem()
                        | Some id -> getNextCachedItem(id)                        
            
            return match r with
                    | Choice2Of3 (id, stats) -> 
                            sprintf "Got %s:%s" stats.ShipTypeId (stats.Period.ToShortDateString()) |> logTrace
                            stats |> ValuationActorMessage.ImportShipTypePeriod |> sendShipStats                        
                            Choice1Of3 id
                    | Choice1Of3 _ -> Choice2Of3 None
                    | Choice3Of3 e -> Choice3Of3 e
        }

    let pipe = MailboxProcessor<ObjectId option * AsyncReplyChannel<bool>>.Start(fun inbox ->
        let rec loop (state: RehydrateStatsState) = 
            async {                
                let! lastId,ch = inbox.Receive()

                let! nextId = onGetNext(lastId)
                
                match nextId with
                | Choice1Of3 id -> 
                        (Some id, ch) |> inbox.Post
                        return! loop {state with TotalRecords = state.TotalRecords + 1L }
                | Choice2Of3 _ -> 
                        let duration = DateTime.UtcNow - state.Start
                        "Rehydration complete." |> logInfo 
                        sprintf "Rehydrate Duration: %s Total Records: %i " (duration.ToString()) state.TotalRecords |> logInfo
                        ch.Reply true
                        return (ignore 0)
                | Choice3Of3 e -> 
                        "Rehydration failing." |> logError
                        ch.Reply false
                        return (ignore 0)
            }            
        loop( { RehydrateStatsState.Empty with Start = DateTime.UtcNow } )
        )

    do pipe.Error.Add(logException)
    
    member __.Start() =
        "Starting Rehydrate..." |> logInfo
        pipe.PostAndAsyncReply (fun ch -> None, ch)
