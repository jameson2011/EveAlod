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
            
        // TODO: better way?
        let private toTags (json: JsonValue option) : KillTag list=
            let result = []
            let result = match (json |> getProp "npc" |> getBool) with
                            | true -> (KillTag.Npc :: result)
                            | _ -> result
            let result = match (json |> getProp "solo" |> getBool) with
                            | true -> (KillTag.Solo :: result)
                            | _ -> result
            let result = match (json |> getProp "awox" |> getBool) with
                            | true -> (KillTag.Awox :: result)
                            | _ -> result
            result

        let private toCargoItem (json: JsonValue) : CargoItem = 
            { CargoItem.Item = { 
                                Entity.Id = Some json |> getProp "item_type_id" |> getString
                                Name = ""
                                }; 
                        Quantity =  Some json |> getProp "quantity_dropped" |> getInt
                        }
            
        let private toCargoItems (json: JsonValue[] option) : CargoItem list = 
            match json with
            | Some xs -> xs |> Seq.map toCargoItem |> List.ofSeq
            | None -> []

        let private toAttacker (json: JsonValue option) : Attacker = 
            {
                Attacker.Char = json |> toCharacter;
                Damage = json |> getProp "damage_done" |> getInt;
                Ship = json |> getProp "ship_type_id" |> getString |> toEntity 
            }

        let private toAttackers (json: JsonValue[] option) : Attacker list = 
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
                let location = zkb |> getProp "locationID" |> getString |> toEntity
                let items = victimJson |> getProp "items" |> Option.map (fun j -> j.AsArray())

                Some {
                    Kill.Id = id; 
                    Occurred = occurred; 
                    ZkbUri = (sprintf "https://zkillboard.com/kill/%s/" id);
                    Location = location;

                    Victim = toCharacter victimJson;
                    VictimShip = (victimJson |> getProp "ship_type_id" |> getString) |> toEntity;
                    TotalValue = zkb |> getProp "totalValue" |> getFloat;
                    Cargo = items |> toCargoItems;
                
                    Attackers = attackersJson |> toAttackers;
                
                    Tags = (toTags zkb);

                    AlodScore = 0.;
                }
            | _ -> None
            
            

