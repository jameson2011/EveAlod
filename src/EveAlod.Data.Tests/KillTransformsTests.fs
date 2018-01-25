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
            let json = [ data;  ]
                        |> Seq.zip [ "stuff";  ]
                        |> Seq.map (fun (p,v) -> (p, JsonValue.String(v)))
                        |> Array.ofSeq
                        |> JsonValue.Record
                        
            let c = KillTransforms.toCharacter (Some json)
            
            c.IsNone
            
        [<Property(Verbose = true, Arbitrary = [| typeof<NonEmptyStrings> |])>]
        let ``toCharacter id ``(id: string)  =

            let json = [ id;  ]
                        |> Seq.zip [ "character_id";  ]
                        |> Seq.map (fun (p,v) -> (p, JsonValue.String(v)))
                        |> Array.ofSeq
                        |> JsonValue.Record
            
            let c = KillTransforms.toCharacter (Some json)

            c.Value.Char.Id = id &&
            c.Value.Corp.IsNone &&
            c.Value.Alliance.IsNone

        [<Property(Verbose = true, Arbitrary = [| typeof<NonEmptyStrings> |])>]
        let ``toCharacter id corp_id ``(id: string) (corpId: string) =

            let json = [ id; corpId; ]
                        |> Seq.zip [ "character_id"; "corporation_id"; ]
                        |> Seq.map (fun (p,v) -> (p, JsonValue.String(v)))
                        |> Array.ofSeq
                        |> JsonValue.Record
            
            let c = KillTransforms.toCharacter (Some json)

            c.Value.Char.Id = id &&
            c.Value.Corp.Value.Id = corpId &&
            c.Value.Alliance.IsNone
        
        [<Property(Verbose = true, Arbitrary = [| typeof<NonEmptyStrings> |])>]
        let ``toCharacter id corp_id alliance_id ``(id: string) (corpId: string) (allianceId: string)=

            let json = [ id; corpId; allianceId ]
                        |> Seq.zip [ "character_id"; "corporation_id"; "alliance_id" ]
                        |> Seq.map (fun (p,v) -> (p, JsonValue.String(v)))
                        |> Array.ofSeq
                        |> JsonValue.Record
           
            
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
            
            (tags |> Seq.contains KillTag.AwoxKill) = awox &&
            (tags |> Seq.contains KillTag.SoloKill) = solo &&
            (tags |> Seq.contains KillTag.NpcKill) = npc
            
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
            
        [<Property(Verbose = true, Arbitrary = [| typeof<NonEmptyStrings>; typeof<PositiveInts> |])>]
        let ``toCargoItem``(id: string) (quantity: int) (flag: int)=

            let json = [ id; str quantity; str flag  ]
                        |> Seq.zip [ "item_type_id"; "quantity_dropped"; "flag"; ]
                        |> Seq.map (fun (p,v) -> (p, JsonValue.String(v)))
                        |> Array.ofSeq
                        |> JsonValue.Record
            let r = (KillTransforms.toCargoItem json).Value
            r.Item.Id = id &&
            r.Quantity = quantity &&
            r.Location = (EntityTransforms.toItemLocation (str flag))
                        
        [<Property(Verbose = true, Arbitrary = [| typeof<NonEmptyStrings>; typeof<PositiveInts> |])>]
        let ``toCargoItem invalid quantity prop``(id: string) (quantity: int) (flag: int) name=

            let json = [ id; str quantity; str flag  ]
                        |> Seq.zip [ "item_type_id"; name; "flag"; ]
                        |> Seq.map (fun (p,v) -> (p, JsonValue.String(v)))
                        |> Array.ofSeq
                        |> JsonValue.Record
            let r = (KillTransforms.toCargoItem json).Value
            r.Item.Id = id &&
            r.Quantity = 0 &&
            r.Location = (EntityTransforms.toItemLocation (str flag))
                        
        [<Property(Verbose = true, Arbitrary = [| typeof<NonEmptyStrings>; typeof<PositiveInts> |])>]
        let ``toCargoItem invalid id prop``(id: string) (quantity: int) (flag: int) (name: string)=

            let json = [ id; str quantity; str flag  ]
                        |> Seq.zip [ name; "quantity_dropped2"; "flag"; ]
                        |> Seq.map (fun (p,v) -> (p, JsonValue.String(v)))
                        |> Array.ofSeq
                        |> JsonValue.Record
            let r = (KillTransforms.toCargoItem json)
            r = None
                        
        [<Property(Verbose = true, Arbitrary = [| typeof<NonEmptyStrings>; typeof<PositiveInts> |])>]
        let ``toCargoItem invalid flag prop``(id: string) (dropped: int) (destroyed: int) (flag: int) name=

            let json = [ id; str dropped; str destroyed; str flag  ]
                        |> Seq.zip [ "item_type_id"; "quantity_dropped"; "quantity_destroyed"; name; ]
                        |> Seq.map (fun (p,v) -> (p, JsonValue.String(v)))
                        |> Array.ofSeq
                        |> JsonValue.Record
            let r = (KillTransforms.toCargoItem json).Value
            r.Item.Id = id &&
            r.Quantity = (dropped + destroyed) &&
            r.Location = ItemLocation.Unknown

        [<Property(Verbose = true, Arbitrary = [| typeof<PositiveInts> |])>]
        let ``toAttackers invalidAttackers`` (corpId: int)=
            let char = [ ""; str corpId ]
                        |> Seq.zip [ "character_id"; "corporation_id";  ]
                        |> Seq.map (fun (p,v) -> (p, JsonValue.String(v)))
                        |> Array.ofSeq
                        |> JsonValue.Record
            let chars = Some [| char |]

            let r = KillTransforms.toAttackers chars

            match r with
            | [] -> true
            | _ -> false
            

        [<Property(Verbose = true, Arbitrary = [| typeof<PositiveInts> |])>]
        let ``toAttackers validAttackers``(id: int) (corpId: int)=
            let char = [ str id; str corpId ]
                        |> Seq.zip [ "character_id"; "corporation_id";  ]
                        |> Seq.map (fun (p,v) -> (p, JsonValue.String(v)))
                        |> Array.ofSeq
                        |> JsonValue.Record
            
            let chars = Some [| char |]

            let r = KillTransforms.toAttackers chars

            match r with
            | [] -> false
            | _ -> true            
            