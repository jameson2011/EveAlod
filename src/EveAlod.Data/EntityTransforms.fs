namespace EveAlod.Data

    module EntityTransforms=

        open EveAlod.Common.Json
        open FSharp.Data
        open System

        type jsonToSolarSystem = JsonProvider<"./SampleSolarSystem.json">
        type jsonGroupIdProvider = JsonProvider<"./SampleIds.json">
        type jsonGroupProvider = JsonProvider<"./SampleEntityGroup.json">
        type jsonEntityProvider = JsonProvider<"./SampleEntity.json">
        type jsonCharacterProvider = JsonProvider<"./SampleCharacter.json">

        

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
            let root = (jsonCharacterProvider.Parse(json))
            Some {Character.Char = {Entity.Id = id; Name = root.Name; };
                    Corp = Some {Entity.Id = root.CorporationId.ToString(); Name = "" };
                    Alliance = None;
                    //Alliance = Some { Entity.Id = root.AllianceId.ToString(); Name = ""};
                }

        let parseEntity json =
            let root = (jsonEntityProvider.Parse(json))
            Some {Entity.Id = root.TypeId.ToString(); 
                        Name = root.Name;
                  }

        let parseEntityGroup json =
            let root = (jsonGroupProvider.Parse(json))
            Some {EntityGroup.Id = root.GroupId.ToString(); 
                        Name = root.Name;
                        EntityIds = root.Types |> Seq.map (fun i -> i.ToString()) |> Array.ofSeq
                }

        let parseSolarSystem json =
            let o = jsonToSolarSystem.Parse(json)            
            Some { SolarSystem.Id = o.SystemId.ToString();
                        Name = o.Name;
                        Security = getSecurityStatus o.Name o.SecurityStatus}
