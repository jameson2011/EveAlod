namespace EveAlod.Valuation

open EveAlod.Common.Json
open EveAlod.Data
open EveAlod.Common.Strings

type private ShipTypeMap = Map<string, ShipTypeStatsActor>

type ShipStatsActor(log: PostMessage)=
    
    let shipTypeId json = 
        json |> KillTransforms.asKillPackage |> prop "killmail" |> prop "victim" |> propStr "ship_type_id"

    let onImportKillJson (map: ShipTypeMap) json =
        match shipTypeId json with
        | NullOrWhitespace _ -> map
        | id ->                 let actor = if map.ContainsKey(id) then
                                                map.[id]
                                            else
                                                ShipTypeStatsActor(log, id)                                
                                json |> ImportKillJson |> actor.Post                                
                                map.Add(id, actor)

                                
        
    let pipe = MessageInbox.Start(fun inbox -> 
        let rec loop(map: ShipTypeMap) = async {
                
                let! msg = inbox.Receive()
                let newMap = match msg with
                                | ImportKillJson json ->    onImportKillJson map json                                    
                                | _ ->                      map
                return! loop(newMap)
            }
        loop(Map.empty)
        )

    do pipe.Error.Add(Actors.postException typeof<ShipTypeStatsActor>.Name log)

    member __.Post(msg: ValuationActorMessage) = pipe.Post msg