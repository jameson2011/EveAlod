namespace EveAlod.Valuation

open System
open MongoDB.Bson
open MongoDB.Driver
open EveAlod.Common
open EveAlod.Common.Strings
open EveAlod.Common.MongoDb
open EveAlod.Data


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
        
    let docsSeq (cursor: IAsyncCursor<BsonDocument>) =
        seq {
            while cursor.MoveNext() do
                let c = cursor.Current
                yield c
        }

    let getCachedStats()=
            async {                
                try
                    use! docs = shipTypeStatsCollection.Value.FindAsync(fun d -> true) |> Async.AwaitTask

                    let result = docs 
                                    |> docsSeq |> Seq.collect id
                                    |> Seq.map toStatsEntry
                                    |> List.ofSeq
                    return Choice1Of2 result
                    
                with
                | e ->  logException e
                        return Choice2Of2 e
            }
        
        
    let rehydrate()=
        async {
            try
                let start = DateTime.UtcNow
                logInfo "Reading stats cache..." 
                let! items = getCachedStats()
                match items with
                | Choice1Of2 stats ->                                 
                                logInfo "Starting stats merge..."
                                let count = stats 
                                            // No need to wait! The actor queues have single threaded consumers 
                                            // so any subsequent outside request will fall to the back of the queue
                                            |> Seq.map (ValuationActorMessage.ImportShipTypePeriod >> sendShipStats)
                                            |> Seq.length

                                return Choice1Of2 (DateTime.UtcNow - start, count)
                            
                | Choice2Of2 e -> return (Choice2Of2 e)
            with
            | e ->  logException e
                    return Choice2Of2 e
        }
        
    let pipe = MailboxProcessor<AsyncReplyChannel<bool>>.Start(fun inbox ->
        let rec loop () = 
            async {                
                let! ch = inbox.Receive()

                let! result = rehydrate()
                match result with
                | Choice1Of2 (duration,count) ->                     
                    "Rehydration complete." |> logInfo 
                    sprintf "Rehydrate Duration: %s Total Records: %i " (duration.ToString()) count |> logInfo
                    ch.Reply true
                    return (ignore 0)
                | Choice2Of2 e ->                      
                    "Rehydration finished with error." |> logError
                    ch.Reply false
                    return (ignore 0)                
            }            
        loop( )
        )

    do pipe.Error.Add(logException)
    
    member __.Start() =
        "Starting Rehydrate..." |> logInfo
        pipe.PostAndAsyncReply id
