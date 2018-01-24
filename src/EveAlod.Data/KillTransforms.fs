namespace EveAlod.Data
    
    module KillTransforms=

        open EveAlod.Common
        open EveAlod.Common.Json
        open FSharp.Data

        type jsonToKill = JsonProvider<"./SampleRedisqKillmail.json">

        let defaultKill() =
            { Kill.Id = "";
                Occurred = System.DateTime.UtcNow;
                Tags = [];
                ZkbUri = "";
                Location = None;
                Victim = None;
                VictimShip = None;
                AlodScore = 0.;
                Attackers = [];
                Fittings = [];
                Cargo = [];
                TotalValue = 0.;
            }
        
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

        let asKillPackage msg = 
            let kmJson = jsonToKill.Parse(msg)
            Some (kmJson.JsonValue.GetProperty("package"))
            
        let asKill = getPropOption "killmail"

        let applyMeta (package: JsonValue option) (km: Kill)=
            let kmJson = package |> asKill
            let zkb = package |> getPropOption "zkb"
            let id = kmJson |> getPropStr "killmail_id"
            let location = zkb |> getPropStr "locationID" |> EntityTransforms.toEntity

            { km with Id = id;
                        Occurred = kmJson |> getPropDateTime "killmail_time";
                        ZkbUri = (sprintf "https://zkillboard.com/kill/%s/" id);
                        Location = location;
                        TotalValue = zkb |> getPropFloat "totalValue";
                        Tags = (toStandardTags zkb);
                }
            

        let applyVictim (package: JsonValue option) (km: Kill)=
            let victimJson = package |> asKill
                                    |> getPropOption "victim"

            let victim = victimJson |> toCharacter 
            let victimShip = victimJson |> getPropStr "ship_type_id" |> EntityTransforms.toEntity;
            let items = victimJson |> getPropOption "items" |> Option.map (fun j -> j.AsArray()) |> toCargoItems
            let fittings,cargo = items |> Seq.splitBy (fun i -> EntityTransforms.isFitted i.Location)

            { km with Victim = victim;
                        VictimShip = victimShip;
                        Fittings = fittings;
                        Cargo = cargo;
            }
            
        let applyAttackers (package: JsonValue option) (km: Kill)=
            let attackersJson = package 
                                    |> asKill 
                                    |> getPropOption "attackers" 
                                    |> Option.map (fun j -> j.AsArray())
            {  km with Attackers = attackersJson |> toAttackers; }                           
        
        let toKill msg = 
            let json = msg |> asKillPackage
            match json with
            | None -> None
            | Some _ -> let r = defaultKill() 
                                |> applyMeta json 
                                |> applyVictim json 
                                |> applyAttackers json
                        r |> Option.Some
                
        let toKill2 (msg: string)=
            let kmJson = jsonToKill.Parse(msg)
            let package = Some (kmJson.JsonValue.GetProperty("package"))
            let km = package |> getPropOption "killmail"
            match km with
            | Some _ -> 
            
                let id = km |> getPropStr "killmail_id" 
                let occurred = km |> getPropDateTime "killmail_time"
                let victimJson  = km |> getPropOption "victim"
                let attackersJson = km |> getPropOption "attackers" |> Option.map (fun j -> j.AsArray())
                let zkb = package |> getPropOption "zkb"
                let location = zkb |> getPropStr "locationID" |> EntityTransforms.toEntity
                let items = victimJson |> getPropOption "items" |> Option.map (fun j -> j.AsArray()) |> toCargoItems
                let fittings,cargo = items |> Seq.splitBy (fun i -> EntityTransforms.isFitted i.Location)
                Some {
                    Kill.Id = id; 
                    Occurred = occurred; 
                    ZkbUri = (sprintf "https://zkillboard.com/kill/%s/" id);
                    Location = location;

                    Victim = toCharacter victimJson;
                    VictimShip = victimJson |> getPropStr "ship_type_id" |> EntityTransforms.toEntity;
                    TotalValue = zkb |> getPropFloat "totalValue";
                    Fittings = fittings;
                    Cargo = cargo; 
                    Attackers = attackersJson |> toAttackers;                
                    Tags = (toStandardTags zkb);
                    AlodScore = 0.;
                }
            | _ -> None
            
            

