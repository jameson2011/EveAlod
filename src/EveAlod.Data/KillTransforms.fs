namespace EveAlod.Data
    
    module KillTransforms=

        open EveAlod.Common.Json
        open FSharp.Data

        type jsonToKill = JsonProvider<"./SampleRedisqKillmail.json">

        
        let toCharacter (json: JsonValue option) =
            let char = EntityTransforms.toEntity (json |> getProp "character_id" |> getString)
            let corp = EntityTransforms.toEntity (json |> getProp "corporation_id" |> getString)
            let alliance = EntityTransforms.toEntity (json |> getProp "alliance_id" |> getString)

            match char with
            | Some c -> Some { Character.Char = c; Corp = corp; Alliance = alliance }
            | _ -> None
            
        let toStandardTags (json: JsonValue option) =
            let result = []
            let result = match (json |> getProp "npc" |> getBool) with
                            | true -> (KillTag.Npc :: result)
                            | _ -> result
            let result = match (json |> getProp "solo" |> getBool) with
                            | true -> (KillTag.Solo :: result)
                            | _ -> result
            match (json |> getProp "awox" |> getBool) with
                            | true -> (KillTag.Awox :: result)
                            | _ -> result
            

        
        
        let toCargoItem (json: JsonValue) = 
            { CargoItem.Item = { 
                                Entity.Id = Some json |> getProp "item_type_id" |> getString
                                Name = ""
                                }; 
                        Quantity =  Some json |> getProp "quantity_dropped" |> getInt;
                        Location = Some json |> getProp "flag" |> getString |> EntityTransforms.toItemLocation
                        }


            
        let toCargoItems (json: JsonValue[] option) : CargoItem list = 
            match json with
            | Some xs -> xs |> Seq.map toCargoItem |> List.ofSeq
            | None -> []

        let toAttacker (json: JsonValue option) = 
            {
                Attacker.Char = json |> toCharacter;
                Damage = json |> getProp "damage_done" |> getInt;
                Ship = json |> getProp "ship_type_id" |> getString |> EntityTransforms.toEntity 
            }

        let toAttackers (json: JsonValue[] option) = 
            match json with
            | Some xs -> xs |> Seq.map (fun j -> Some j |> toAttacker) |> List.ofSeq
            | None -> []


        let toKill (msg: string)=
            let kmJson = jsonToKill.Parse(msg)
            let package = Some (kmJson.JsonValue.GetProperty("package"))
            let km = package |> getProp "killmail"
            match km with
            | Some _ -> 
            
                let id = km |> getProp "killmail_id" |> getString
                let occurred = km |> getProp "killmail_time" |> getDateTime
                let victimJson  = km |> getProp "victim"
                let attackersJson = km |> getProp "attackers" |> Option.map (fun j -> j.AsArray())
                let zkb = package |> getProp "zkb"
                let location = zkb |> getProp "locationID" |> getString |> EntityTransforms.toEntity
                let items = victimJson |> getProp "items" |> Option.map (fun j -> j.AsArray())

                Some {
                    Kill.Id = id; 
                    Occurred = occurred; 
                    ZkbUri = (sprintf "https://zkillboard.com/kill/%s/" id);
                    Location = location;

                    Victim = toCharacter victimJson;
                    VictimShip = (victimJson |> getProp "ship_type_id" |> getString) |> EntityTransforms.toEntity;
                    TotalValue = zkb |> getProp "totalValue" |> getFloat;
                    Cargo = items |> toCargoItems;
                
                    Attackers = attackersJson |> toAttackers;
                
                    Tags = (toStandardTags zkb);

                    AlodScore = 0.;
                }
            | _ -> None
            
            

