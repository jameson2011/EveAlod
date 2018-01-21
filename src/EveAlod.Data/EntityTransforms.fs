namespace EveAlod.Data

    module EntityTransforms=

        open EveAlod.Common.Json
        open FSharp.Data

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