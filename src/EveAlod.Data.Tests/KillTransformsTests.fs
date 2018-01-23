namespace EveAlod.Data.Tests

    open Xunit
    open FSharp.Data
    open FsCheck
    open FsCheck.Xunit
    open EveAlod.Common.Strings
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

        [<Property(Verbose = true)>]
        let ``toStandardTags builds KillTag list``(npc: bool) (solo: bool) (awox: bool)=
            let props = [ npc; solo; awox ]
                        |> Seq.map str
                        |> Seq.zip [ "npc"; "solo"; "awox" ]
                        |> Seq.map (fun (p,v) -> (p, JsonValue.String(v)))
                        |> Array.ofSeq

            let json = JsonValue.Record(props)

            let tags = KillTransforms.toStandardTags (Some json)
            
            (tags |> Seq.contains KillTag.Awox) = awox &&
            (tags |> Seq.contains KillTag.Solo) = solo &&
            (tags |> Seq.contains KillTag.Npc) = npc
            
        [<Fact>]
        let ``toStandardTags None returns Empty ``()=
            let tags = KillTransforms.toStandardTags None
            tags.IsEmpty

        [<Property(Verbose = true, Arbitrary = [| typeof<NonEmptyStrings> |])>]
        let ``toStandardTags Invalid key returns Empty ``(name)=
            let props = [| (str true, JsonValue.String(name)) |]                        

            let json = JsonValue.Record(props)

            let tags = KillTransforms.toStandardTags (Some json)
            
            tags.IsEmpty
            