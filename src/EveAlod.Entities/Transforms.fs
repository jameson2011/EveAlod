namespace EveAlod.Entities
    
    open FSharp.Data

    module Transforms=

        type jsonToKill = JsonProvider<"./SampleRedisqKillmail.json">

        let private toEntity (id: string) =
            match id with
            | "" -> None
            | id -> Some {Entity.Id = id; Name = "" };

        let private toCharacter (json: JsonValue option) =
            let char = toEntity (json |> getProp "character_id" |> getString)
            let corp = toEntity (json |> getProp "corporation_id" |> getString)
            let alliance = toEntity (json |> getProp "alliance_id" |> getString)

            match char with
            | Some c -> Some { Character.Char = c; Corp = corp; Alliance = alliance }
            | _ -> None
            
        // TODOTK: better way???
        let private toTags (json: JsonValue option) : KillTag list=
            let result = []
            let result = match (json |> getProp "npc" |> getBool) with
                            | true -> (KillTag.Npc "NPC" :: result)
                            | _ -> result
            let result = match (json |> getProp "solo" |> getBool) with
                            | true -> (KillTag.Solo "Solo" :: result)
                            | _ -> result
            let result = match (json |> getProp "awox" |> getBool) with
                            | true -> (KillTag.Awox "Awox" :: result)
                            | _ -> result
            result

        let toKill (msg: string)=
            let kmJson = jsonToKill.Parse(msg)
            let package = Some (kmJson.JsonValue.GetProperty("package"))
            let km = package |> getProp "killmail"
            match km with
            | Some _ -> 
            
                let id = km |> getProp "killmail_id" |> getString
                let occurred = km |> getProp "killmail_time" |> getDateTime
                let victimJson  = km |> getProp "victim"
                let zkb = package |> getProp "zkb"
                let location = zkb |> getProp "locationID" |> getString |> toEntity
                
                Some {
                    Kill.Id = id; 
                    Occurred = occurred; 
                    ZkbUri = (sprintf "https://zkillboard.com/kill/%s/" id);
                    Location = location;

                    Victim = toCharacter victimJson;
                    VictimShip = (victimJson |> getProp "ship_type_id" |> getString) |> toEntity;
                    TotalValue = zkb |> getProp "totalValue" |> getFloat;
                
                    Attackers = []; // TODOTK:
                
                    Tags = (toTags zkb)
                }
            | _ -> None
            

        let toKill3 (msg: string)=
            // TODOTK:
            Some {
                Kill.Id = ""; 
                Occurred = System.DateTime.UtcNow; 
                ZkbUri = "http://zkillboard.com";
                Location = None;

                Victim = None;
                VictimShip = None;
                TotalValue = 0.;
                
                Attackers = [];
                
                Tags = []
            }

