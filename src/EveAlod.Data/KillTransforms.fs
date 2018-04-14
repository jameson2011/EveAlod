namespace EveAlod.Data
    
    module KillTransforms=

        open FSharp.Data
        open EveAlod.Common
        open EveAlod.Common.Json
        open EveAlod.Common.Strings
        open EveAlod.Data.EntityTransforms

        type JsonKillProvider = JsonProvider<"./SampleRedisqKillmail.json">   
        
        let toCharacter (json: JsonValue option) =
            let char =  json |> propStr "character_id" |> toEntity
            let corp = json |> propStr "corporation_id" |> toEntity
            let alliance = json |> propStr "alliance_id" |> toEntity

            match char with
            | Some c -> Some { Character.Char = c; Corp = corp; Alliance = alliance }
            | _ -> None
            
        
        let toStandardTags (json: JsonValue option) =
            let tagMap = [ "npc", KillTag.NpcKill;
                        "solo", KillTag.SoloKill;
                        "awox", KillTag.AwoxKill]
            let rec addTags (json) (map) result=
                match map with
                | [] -> result
                | (name,tag)::t -> 
                        let r = match json |> propBool name with
                                | true -> tag :: result
                                | _ -> result
                        addTags json t r
            addTags json tagMap []
            
        
        let toCargoItem (json: JsonValue) = 
            let json = Some json
            let item = json |> propStr "item_type_id" |> toEntity;
            match item with
            | Some i -> 
                let destroyed = json |> propInt "quantity_destroyed" 
                let dropped = json |> propInt "quantity_dropped" 
                let quantity = (destroyed + dropped)
                Some { CargoItem.Item =  i;
                            Quantity =  quantity;
                            Location = json |> propStr "flag" |> toItemLocation
                            }                        
            | _ -> None

        let toCargoItems (json: JsonValue[] option) : CargoItem list = 
            match json with
            | Some xs -> xs |> Seq.map toCargoItem |> Seq.mapSomes |> List.ofSeq
            | None -> []

        let toAttacker (json: JsonValue option) = 
            match json |> toCharacter with
            | Some char -> Some {
                                Attacker.Char = Some char;
                                Damage = json |> propInt "damage_done";
                                Ship = json |> propStr "ship_type_id" |> toItemTypeEntity 
                            }
            | _ -> None

        let toAttackers (json: JsonValue[] option) = 
            match json with
            | Some xs -> xs |> Seq.map (Some >> toAttacker) |> Seq.mapSomes |> List.ofSeq
            | None -> []

        let asKillPackage msg = 
            let kmJson = JsonKillProvider.Parse(msg)
            Some (kmJson.JsonValue.GetProperty("package"))
            
        let asKill = prop "killmail"

        let applyMeta (package: JsonValue option) (km: Kill)=
            let kmJson = package |> asKill
            let zkb = package |> prop "zkb"
            let id = kmJson |> propStr "killmail_id"
            let victimJson = kmJson |> prop "victim"
            
            let positionJson = victimJson |> prop "position"
            let posX = positionJson |> propFloat "x"
            let posY = positionJson |> propFloat "y"
            let posZ = positionJson |> propFloat "z"
            let solarSystemId = kmJson |> propInt "solar_system_id"
            
            let location = Locations.toLocation solarSystemId posX posY posZ

            { km with Id = id;
                        Occurred = kmJson |> propDateTime "killmail_time";
                        ZkbUri = id |> Zkb.killUri;
                        Location = location;
                        TotalValue = zkb |> propFloat "totalValue";
                        Tags = (toStandardTags zkb);
                }
            

        let applyVictim (package: JsonValue option) (km: Kill)=
            let victimJson = package |> asKill |> prop "victim"
            let victim = victimJson |> toCharacter 
            let victimShip = victimJson |> propStr "ship_type_id" |> toItemTypeEntity;
            let items = victimJson |> prop "items" |> Option.map asArray |> toCargoItems
            let fittings,cargo = items |> Seq.splitBy (fun i -> isFitted i.Location)

            { km with Victim = victim;
                        VictimShip = victimShip;
                        Fittings = fittings;
                        Cargo = cargo;
            }
            
        let applyAttackers (package: JsonValue option) (km: Kill)=
            let attackersJson = package 
                                    |> asKill 
                                    |> prop "attackers" 
                                    |> Option.map asArray
            {  km with Attackers = attackersJson |> toAttackers; }                           
        
        let isValid (km) = 
            match km.Id with
            | NullOrWhitespace _ -> None
            | _ -> Some km

        let toKill msg = 
            let json = msg |> asKillPackage
            match json with
            | None -> None
            | Some _ -> Kill.empty 
                            |> applyMeta json 
                            |> applyVictim json 
                            |> applyAttackers json
                            |> isValid
        
        let isKill msg = 
            let id = msg |> asKillPackage |> propStr "killID" 
            id <> ""
            
            
            
            
            

