namespace EveAlod.Data

open System
open EveAlod.Common
open IronSde
open IronSde.ItemTypes

module Pods=
         


    let private implantSets =
        [| AttributeTypes.implantSetAngel, ImplantSet.Halo;
            AttributeTypes.implantSetBloodraider, ImplantSet.Talisman;
            AttributeTypes.implantSetCaldariNavy, ImplantSet.Talon;
            AttributeTypes.implantSetLGCaldariNavy, ImplantSet.Talon;
            AttributeTypes.implantSetChristmas, ImplantSet.Christmas;
            AttributeTypes.implantSetFederationNavy, ImplantSet.Spur;
            AttributeTypes.implantSetLGFederationNavy, ImplantSet.Spur;
            AttributeTypes.implantSetGuristas, ImplantSet.Crystal;
            AttributeTypes.implantSetImperialNavy, ImplantSet.Grail;
            AttributeTypes.implantSetLGImperialNavy, ImplantSet.Grail;
            AttributeTypes.implantSetMordus, ImplantSet.Centurion;
            AttributeTypes.implantSetORE, ImplantSet.Harvest;
            AttributeTypes.implantSetRepublicFleet, ImplantSet.Jackal;
            AttributeTypes.implantSetLGRepublicFleet, ImplantSet.Jackal;
            AttributeTypes.implantSetSansha, ImplantSet.Slave;
            AttributeTypes.implantSetSerpentis, ImplantSet.Snake;
            AttributeTypes.implantSetSerpentis2, ImplantSet.Asklepian;
            AttributeTypes.implantSetSisters, ImplantSet.Virtue;
            AttributeTypes.implantSetSyndicate, ImplantSet.Edge;
            AttributeTypes.implantSetThukker, ImplantSet.Nomad;
            AttributeTypes.implantSetWarpSpeed, ImplantSet.Ascendancy;
        |] |> Map.ofSeq

    let private gradePrefixes =
        [| ImplantGrade.HighGrade, "High-grade"; 
           ImplantGrade.MidGrade, "Mid-grade"; 
           ImplantGrade.LowGrade, "Low-grade"; 
            |] |> Map.ofSeq


    let private getImplantSetKey implant =
        implant.attributes |> Seq.tryFind (fun a -> implantSets.ContainsKey a.key)
                           |> Option.map (fun a -> a.key)

    let private getSet (attributeType: AttributeTypes) =
        implantSets |> Map.tryFind attributeType

    let private getGrade implant =
        gradePrefixes |> Seq.filter (fun (kvp) -> implant.name.StartsWith(kvp.Value, StringComparison.Ordinal))
                      |> Seq.map (fun kvp -> kvp.Key)
                      |> Seq.tryHead
                      
    let implants =
        let groups = 
            ItemTypeCategories.Implant
            |> groups
            |> Seq.filter (fun g -> g.key <> ItemTypeGroups.Booster)

        groups |> Seq.collect itemTypes

    let implantSet (implant) = 
        let set = getImplantSetKey implant |> Option.bind getSet

        match set, getGrade implant with
        | Some s, Some g -> Some (s,g)
        | _ -> None
        

        
