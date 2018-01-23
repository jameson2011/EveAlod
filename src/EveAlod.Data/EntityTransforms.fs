namespace EveAlod.Data

    module EntityTransforms=

        open EveAlod.Common.Json
        open FSharp.Data

        type jsonToSolarSystem = JsonProvider<"./SampleSolarSystem.json">
        type jsonGroupIdProvider = JsonProvider<"./SampleIds.json">
        type jsonGroupProvider = JsonProvider<"./SampleEntityGroup.json">
        type jsonEntityProvider = JsonProvider<"./SampleEntity.json">
        type jsonCharacterProvider = JsonProvider<"./SampleCharacter.json">

        let parseCharacter id json = 
            let root = (jsonCharacterProvider.Parse(json))
            Some {Character.Char = {Entity.Id = id; Name = root.Name; };
                    Corp = Some {Entity.Id = root.CorporationId.ToString(); Name = "" };
                    Alliance = None
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
            let area = match o.SecurityStatus with
                        | s when s >= 5.0m -> SpaceArea.Highsec
                        | s when s > 0.0m -> SpaceArea.Lowsec
                        | s when s < -0.98m -> SpaceArea.Wormhole
                        | _ -> SpaceArea.Nullsec
            Some { SolarSystem.Id = o.SystemId.ToString();
                        Name = o.Name;
                        Space = area}
