namespace EveAlod.Data

    module EntityTransforms=

        open FSharp.Data
        open System

        type JsonSolarSystemProvider = JsonProvider<"./SampleSolarSystem.json">
        type JsonGroupIdProvider = JsonProvider<"./SampleIds.json">
        type JsonGroupProvider = JsonProvider<"./SampleEntityGroup.json">
        type JsonEntityProvider = JsonProvider<"./SampleEntity.json">
        type JsonCharacterProvider = JsonProvider<"./SampleCharacter.json">


        let toEntity =
            function
            | "" -> None
            | id -> Some {Entity.Id = id; Name = "" };
       
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
        
        let isFitted = 
            function
            | ItemLocation.HighSlot 
            | ItemLocation.MidSlot 
            | ItemLocation.LowSlot
            | ItemLocation.DroneBay
            | ItemLocation.RigSlot
            | ItemLocation.FixedSlot
            | ItemLocation.FighterBay
            | ItemLocation.FighterTube
            | ItemLocation.Implant
            | ItemLocation.Subsystem -> true
            | _ -> false

        // Temporary until SDE integrated    
        let isWormholeName (name: string) = 
            let stringComp = StringComparer.InvariantCultureIgnoreCase
            (name.StartsWith("J", StringComparison.InvariantCultureIgnoreCase) &&          
                (name.IndexOf("-") < 0 ) &&
                Int32.TryParse(name.Substring(1)) |> (fun (isWh,_) -> isWh) ) ||
            stringComp.Equals(name, "Thera")

        let getSecurityStatus name status = 
            match (name, status) with        
                        | _,s when s >= 0.5m -> SpaceSecurity.Highsec
                        | _,s when s > 0.0m -> SpaceSecurity.Lowsec
                        | n,s when (isWormholeName n) -> SpaceSecurity.Wormhole
                        | _ -> SpaceSecurity.Nullsec        

        let parseCharacter id json = 
            let root = (JsonCharacterProvider.Parse(json))
            Some {Character.Char = {Entity.Id = id; Name = root.Name; };
                    Corp = Some {Entity.Id = root.CorporationId.ToString(); Name = "" };
                    Alliance = None;
                    //Alliance = Some { Entity.Id = root.AllianceId.ToString(); Name = ""};
                }

        let parseEntity json =
            let root = (JsonEntityProvider.Parse(json))
            Some {Entity.Id = root.TypeId.ToString(); 
                        Name = root.Name;
                  }

        let parseEntityGroup json =
            let root = (JsonGroupProvider.Parse(json))
            Some {EntityGroup.Id = root.GroupId.ToString(); 
                        Name = root.Name;
                        EntityIds = root.Types |> Seq.map (fun i -> i.ToString()) |> Array.ofSeq
                }

        let parseSolarSystem json =
            let o = JsonSolarSystemProvider.Parse(json)            
            Some { SolarSystem.Id = o.SystemId.ToString();
                        Name = o.Name;
                        Security = getSecurityStatus o.Name o.SecurityStatus}
