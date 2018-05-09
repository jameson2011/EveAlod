namespace EveAlod.Services

open EveAlod.Common
open EveAlod.Data

    
type KillTagger(config: Configuration)= 
        
    let normalisePairs = 
        [
            [| KillTag.MixedTank; KillTag.Industrial |], [ KillTag.Industrial ];
        ] |> List.map (fun (xs,ys) -> xs |> Set.ofSeq, ys)

    let normalise (tags: KillTag list) =
        
        let rec norm pairs tags =
            match pairs with
            | [] ->         tags
            | (s,r)::t ->   let m = s |> Set.intersect tags
                            let normtags =      if m = s then
                                                    s   |> Set.difference tags
                                                        |> Set.append r
                                                else
                                                    tags
                            norm t normtags
            
        norm normalisePairs (Set.ofList tags) |> List.ofSeq

    let appendSupplementary (tags: KillTag list) =
        [ Tagging.normalPrice tags; ]
            |> Seq.mapSomes
            |> List.ofSeq
            |> List.append tags

    member __.Tag(kill: Kill)=            
        let tags = [                            
                        Tagging.isCorpKill config.MinCorpDamage config.CorpId;
                        Tagging.isCorpLoss config.CorpId;
                        Tagging.hasItemInHold KillTag.PlexInHold (ShipTransforms.isPlex <&&> ShipTransforms.hasQuantity 10);
                        Tagging.hasItemInHold KillTag.SkillInjectorInHold (ShipTransforms.isSkillInjector <&&> ShipTransforms.hasQuantity 5);
                        Tagging.hasItemFitted KillTag.EcmFitted ShipTransforms.isEcm;
                        Tagging.hasItemFitted KillTag.Stabbed ShipTransforms.isWarpCoreStab;
                        Tagging.isPod ShipTransforms.isPod;
                        Tagging.isPlayer;
                        Tagging.isExpensive;
                        Tagging.isSpendyWithinLimits config.ValuationLimit;
                        Tagging.isAboveNormalWithinLowerLimits config.ValuationLowerLimit;
                        Tagging.isShipTypeWideMargin config.ValuationSpread;
                        Tagging.isShipTypeNarrowMargin config.ValuationSpread;
                        Tagging.isCheap; 
                        Tagging.isZeroValue;
                        Tagging.locationTag;
                        Tagging.isGateCamp;
                        Tagging.missingLows;
                        Tagging.missingMids;
                        Tagging.noRigs;
                        Tagging.hasMixedTank;
                        Tagging.isIndustrial;
                    ]
                    |> Seq.map (fun f -> f kill)
                    |> Seq.mapSomes
                    |> List.ofSeq
                    |> appendSupplementary
                    |> List.append kill.Tags
                    |> normalise

        {kill with Tags = tags}
