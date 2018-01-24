namespace EveAlod.Data
    
    module KillTransforms=

        open EveAlod.Common
        open EveAlod.Common.Json
        open FSharp.Data

        type jsonToKill = JsonProvider<"./SampleRedisqKillmail.json">

        
        let toCharacter (json: JsonValue option) =
            let char =  json |> getPropStr "character_id" |> EntityTransforms.toEntity
            let corp = json |> getPropStr "corporation_id" |> EntityTransforms.toEntity
            let alliance = json |> getPropStr "alliance_id" |> EntityTransforms.toEntity

            match char with
            | Some c -> Some { Character.Char = c; Corp = corp; Alliance = alliance }
            | _ -> None
            
        
        let toStandardTags (json: JsonValue option) =
            let tagMap = [ "npc", KillTag.Npc;
                        "solo", KillTag.Solo;
                        "awox", KillTag.Awox]
            let rec addTags (json) (map) result=
                match map with
                | [] -> result
                | (name,tag)::t -> 
                        let r = match json |> getPropOption name |> getBool with
                                | true -> tag :: result
                                | _ -> result
                        addTags json t r
            addTags json tagMap []
            
        
        let toCargoItem (json: JsonValue) = 
            let json = Some json
            let item = json |> getPropStr "item_type_id" |> EntityTransforms.toEntity;
            match item with
            | Some i -> 
                let destroyed = json |> getPropInt "quantity_destroyed" 
                let dropped = json |> getPropInt "quantity_dropped" 
                let quantity = (destroyed + dropped)
                Some { CargoItem.Item =  i;
                            Quantity =  quantity;
                            Location = json |> getPropStr "flag" |> EntityTransforms.toItemLocation
                            }                        
            | _ -> None

        let toCargoItems (json: JsonValue[] option) : CargoItem list = 
            match json with
            | Some xs -> xs |> Seq.map toCargoItem |> Seq.mapSomes |> List.ofSeq
            | None -> []

        let toAttacker (json: JsonValue option) = 
            {
                Attacker.Char = json |> toCharacter;
                Damage = json |> getPropInt "damage_done";
                Ship = json |> getPropStr "ship_type_id" |> EntityTransforms.toEntity 
            }

        let toAttackers (json: JsonValue[] option) = 
            match json with
            | Some xs -> xs |> Seq.map (fun j -> Some j |> toAttacker) |> List.ofSeq
            | None -> []


        let toKill (msg: string)=
            let kmJson = jsonToKill.Parse(msg)
            let package = Some (kmJson.JsonValue.GetProperty("package"))
            let km = package |> getPropOption "killmail"
            match km with
            | Some _ -> 
            
                let id = km |> getPropOption "killmail_id" |> getString
                let occurred = km |> getPropOption "killmail_time" |> getDateTime
                let victimJson  = km |> getPropOption "victim"
                let attackersJson = km |> getPropOption "attackers" |> Option.map (fun j -> j.AsArray())
                let zkb = package |> getPropOption "zkb"
                let location = zkb |> getPropOption "locationID" |> getString |> EntityTransforms.toEntity
                let items = victimJson |> getPropOption "items" |> Option.map (fun j -> j.AsArray())

                Some {
                    Kill.Id = id; 
                    Occurred = occurred; 
                    ZkbUri = (sprintf "https://zkillboard.com/kill/%s/" id);
                    Location = location;

                    Victim = toCharacter victimJson;
                    VictimShip = (victimJson |> getPropOption "ship_type_id" |> getString) |> EntityTransforms.toEntity;
                    TotalValue = zkb |> getPropOption "totalValue" |> getFloat;
                    Cargo = items |> toCargoItems;
                
                    Attackers = attackersJson |> toAttackers;
                
                    Tags = (toStandardTags zkb);

                    AlodScore = 0.;
                }
            | _ -> None
            
            

