namespace EveAlod.Services

    open EveAlod.Common
    
    open EveAlod.Data

    module Commentary=

        let private tagTexts = 
            [ 
                KillTag.CorpLoss, [| "CORPIE DOWN"; "RIP" |];
                KillTag.CorpKill, [| "GREAT VICTORY"; "GLORIOUS VICTORY" |];
                KillTag.Pod, [| "Someone should have bought pod insurance"; "Someone needs his implants back"; |];
                KillTag.PlexInHold, [| "Plex in hold!"; "BWAHAHAHAHAHA!"; "Plex vaults - they exist"; "RMT DOWN" |];
                KillTag.SkillInjectorInHold, [| "Skill injector in hold!"; "FFS"; "No comment needed" |];
                KillTag.AwoxKill, [| "Ooooh... Awox"; "Should have checked his API"; "Didn't like that corp anyway" |];
                KillTag.EcmFitted, [| "ECM is illegal"; "Doing God's work" |];
                KillTag.Expensive, [| "DERP"; "Oh dear, how sad, never mind"; "Someone's gonna be crying" |];
                KillTag.Spendy, [| "Oops"; "DEPLOY CREDIT CARD" |];
            ]
            |> Map.ofSeq


        let getTagText (rnd: System.Random) tag =            
            match tagTexts |> Map.tryFind tag with
            | Some txts -> 
                let idx = rnd.Next(0, txts.Length)
                txts.[idx]
            | _ -> ""
        
        
        let getTagsText (text: KillTag -> string) (tags: KillTag list) =
            let rec getTexts tags = 
                match tags with
                | [] -> ""
                | head::tail -> 
                    match (text head) with
                    | "" -> getTexts tail 
                    | txt -> txt
            getTexts tags


