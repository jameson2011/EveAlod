namespace EveAlod.Data.Tests

    open Xunit
    open FSharp.Data
    open FsCheck
    open FsCheck.Xunit
    open EveAlod.Common.Tests
    open EveAlod.Data

    module KillTransformsTests=
        
        [<Property(Verbose = true, Arbitrary = [| typeof<NonEmptyStrings> |])>]
        let ``toCharacter null``(data)=
            let props = [ data;  ]
                        |> Seq.zip [ "stuff";  ]
                        |> Seq.map (fun (p,v) -> (p, JsonValue.String(v)))
                        |> Array.ofSeq

            let json = JsonValue.Record(props)
            
            let c = KillTransforms.toCharacter (Some json)
            
            c.IsNone
            
        [<Property(Verbose = true, Arbitrary = [| typeof<NonEmptyStrings> |])>]
        let ``toCharacter id ``(id: string)  =

            let props = [ id;  ]
                        |> Seq.zip [ "character_id";  ]
                        |> Seq.map (fun (p,v) -> (p, JsonValue.String(v)))
                        |> Array.ofSeq

            let json = JsonValue.Record(props)
            
            let c = KillTransforms.toCharacter (Some json)

            c.Value.Char.Id = id &&
            c.Value.Corp.IsNone &&
            c.Value.Alliance.IsNone

        [<Property(Verbose = true, Arbitrary = [| typeof<NonEmptyStrings> |])>]
        let ``toCharacter id corp_id ``(id: string) (corpId: string) =

            let props = [ id; corpId; ]
                        |> Seq.zip [ "character_id"; "corporation_id"; ]
                        |> Seq.map (fun (p,v) -> (p, JsonValue.String(v)))
                        |> Array.ofSeq

            let json = JsonValue.Record(props)
            
            let c = KillTransforms.toCharacter (Some json)

            c.Value.Char.Id = id &&
            c.Value.Corp.Value.Id = corpId &&
            c.Value.Alliance.IsNone
        
        [<Property(Verbose = true, Arbitrary = [| typeof<NonEmptyStrings> |])>]
        let ``toCharacter id corp_id alliance_id ``(id: string) (corpId: string) (allianceId: string)=

            let props = [ id; corpId; allianceId ]
                        |> Seq.zip [ "character_id"; "corporation_id"; "alliance_id" ]
                        |> Seq.map (fun (p,v) -> (p, JsonValue.String(v)))
                        |> Array.ofSeq

            let json = JsonValue.Record(props)
            
            let c = KillTransforms.toCharacter (Some json)

            c.Value.Char.Id = id &&
            c.Value.Corp.Value.Id = corpId &&
            c.Value.Alliance.Value.Id = allianceId