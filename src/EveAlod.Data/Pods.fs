namespace EveAlod.Data

open EveAlod.Common
open IronSde

module Pods=
                
    let private implantSets =
        [| AttributeTypes.implantSetAngel, ImplantSet.Halo;
            AttributeTypes.implantSetBloodraider, ImplantSet.Talisman;
            AttributeTypes.implantSetCaldariNavy, ImplantSet.Talon;
            AttributeTypes.implantSetChristmas, ImplantSet.Christmas;
            AttributeTypes.implantSetFederationNavy, ImplantSet.Spur;
            AttributeTypes.implantSetGuristas, ImplantSet.Crystal;
            AttributeTypes.implantSetImperialNavy, ImplantSet.Grail;
            AttributeTypes.implantSetMordus, ImplantSet.Centurion;
            AttributeTypes.implantSetORE, ImplantSet.Harvest;
            AttributeTypes.implantSetRepublicFleet, ImplantSet.Jackal;
            AttributeTypes.implantSetSansha, ImplantSet.Slave;
            AttributeTypes.implantSetSerpentis, ImplantSet.Snake;
            AttributeTypes.implantSetSerpentis2, ImplantSet.Asklepian;
            AttributeTypes.implantSetSisters, ImplantSet.Virtue;
            AttributeTypes.implantSetSyndicate, ImplantSet.Edge;
            AttributeTypes.implantSetThukker, ImplantSet.Nomad;
            AttributeTypes.implantSetWarpSpeed, ImplantSet.Ascendancy;
        |] |> Map.ofSeq


    let private getImplantSet implant =
        implant.attributes |> Seq.tryFind (fun a -> implantSets.ContainsKey a.key)
                           |> Option.map (fun a -> a.key)

    let implantSet (implant) = 
        implant |> getImplantSet
                |> Option.bind (fun s -> implantSets |> Map.tryFind s)
