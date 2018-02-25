namespace EveAlod.Valuation

open EveAlod.Common.Json
open EveAlod.Data
open EveAlod.Common.Strings

type private ShipTypeMap = Map<string, ShipTypeStatsActor>

type ShipStatsActor(config: ValuationConfiguration, log: PostMessage)=
    let logException e = ActorMessage.Exception (typeof<ShipStatsActor>.Name, e) |> log
    let logInfo = ActorMessage.Info >> log

    let shipTypeId json = 
        json |> KillTransforms.asKillPackage |> prop "killmail" |> prop "victim" |> propStr "ship_type_id"

    let getAddActor (map: ShipTypeMap) id =
        let actor = if map.ContainsKey(id) then map.[id]
                    else ShipTypeStatsActor(config, log, id)
        map.Add(id, actor), actor

    let onImportKillJson (map: ShipTypeMap) json =
        match shipTypeId json with
        | NullOrWhitespace _ -> map
        | id ->                 let map,actor = getAddActor map id
                                json |> ImportKillJson |> actor.Post
                                map

    let onGetShipSummaryStats (map:ShipTypeMap ) (ch: AsyncReplyChannel<ShipSummaryStatistics>) =        
        let getActorStats (actor: ShipTypeStatsActor) = 
            async {
                return! actor.GetStatsSummary()
                }

        let allStats = map |> Map.toSeq                        
                        |> Seq.map (fun (_,actor) -> getActorStats actor)
                        |> Async.Parallel
                        |> Async.RunSynchronously

        { ShipSummaryStatistics.Empty with 
                                ShipTypeCount = map.Count;
                                ShipTypeIds = (map |> Seq.map (fun k -> k.Key) |> Set.ofSeq);
                                TotalKills = allStats |> Seq.sumBy (fun s -> s.TotalKills) }
                                |>  ch.Reply 
        map
        
    let onGetShipTypeStats map id ch =
        let map,actor = getAddActor map id
        GetShipTypeStats (id,ch) |> actor.Post
        map
        
    let pipe = MessageInbox.Start(fun inbox -> 
        let rec loop(map: ShipTypeMap) = async {
                
                let! msg = inbox.Receive()
                let newMap = try
                                match msg with
                                    | ImportKillJson json ->        onImportKillJson map json
                                    | GetShipTypeStats (id,ch) ->   onGetShipTypeStats map id ch                                
                                    | GetShipSummaryStats (ch) ->   onGetShipSummaryStats map ch
                                with 
                                | e ->  logException e
                                        map
                return! loop(newMap)
            }
        loop(Map.empty)
        )

    do pipe.Error.Add(Actors.postException typeof<ShipTypeStatsActor>.Name log)

    member __.Post(msg: ValuationActorMessage) = pipe.Post msg

    member __.GetShipSummaryStats()=
        pipe.PostAndAsyncReply ValuationActorMessage.GetShipSummaryStats 


    member __.GetShipTypeStats(typeId: string)=        
        pipe.PostAndAsyncReply (fun ch -> ValuationActorMessage.GetShipTypeStats (typeId, ch))

