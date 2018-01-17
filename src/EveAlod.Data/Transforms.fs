namespace EveAlod.Data
    
    module Transforms=

        open EveAlod.Common.Json
        open FSharp.Data

        type jsonToKill = JsonProvider<"./SampleRedisqKillmail.json">

        let toEntity =
            function
            | "" -> None
            | id -> Some {Entity.Id = id; Name = "" };
       
        let private toCharacter (json: JsonValue option) =
            let char = toEntity (json |> getProp "character_id" |> getString)
            let corp = toEntity (json |> getProp "corporation_id" |> getString)
            let alliance = toEntity (json |> getProp "alliance_id" |> getString)

            match char with
            | Some c -> Some { Character.Char = c; Corp = corp; Alliance = alliance }
            | _ -> None
            
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

        let toItemLocation id =
            match System.Int32.TryParse(id) with
            | true, x -> 
                match x with
                | 0 -> ItemLocation.NoLocation
                | 4 -> ItemLocation.Hangar
                | 5 -> ItemLocation.CargoHold
                | 27 | 28 | 29 | 30 | 31 | 32 | 33 | 34 -> ItemLocation.HighSlot
                | 19 | 20 | 21 | 22 | 23 | 24 | 25 | 26 -> ItemLocation.MidSlot
                | 11 | 12 | 13 | 14 | 15 | 16 | 17 | 18 -> ItemLocation.LowSlot
                | 35 -> ItemLocation.FixedSlot
                | 56 -> ItemLocation.Capsule
                | 57 -> ItemLocation.Pilot
                | 92 | 93 | 94 | 95 | 96 | 97 | 98 | 99 -> ItemLocation.RigSlot
                | 125 | 126 | 127 | 128 | 129 | 130 | 131 | 132 -> ItemLocation.Subsystem
                | 89 -> ItemLocation.Implant
                | 87 -> ItemLocation.DroneBay
                | 90 -> ItemLocation.ShipHangar
                | 138 | 139 | 140 | 141 | 142 -> ItemLocation.ShipHold
                | 122 -> ItemLocation.SecondaryStorage
                | 155 -> ItemLocation.FleetHangar
                | 158 -> ItemLocation.FighterBay
                | 159 | 160 | 161 | 162 | 163 -> ItemLocation.FighterTube
                | 177 -> ItemLocation.SubsystemBay
                | _ -> ItemLocation.Unknown
            | _ -> 
                ItemLocation.Unknown
        
        let private toCargoItem (json: JsonValue) : CargoItem = 
            { CargoItem.Item = { 
                                Entity.Id = Some json |> getProp "item_type_id" |> getString
                                Name = ""
                                }; 
                        Quantity =  Some json |> getProp "quantity_dropped" |> getInt;
                        Location = Some json |> getProp "flag" |> getString |> toItemLocation
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
            
            

