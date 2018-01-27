namespace EveAlod.Services

    open EveAlod.Common
    open EveAlod.Data

    module Commentary=

        // TODO: clean this up. No need for two sets...
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
        
        let private conjunctiveTagTexts =
            [|
                [| KillTag.NpcKill; KillTag.Spendy |], [| "No permit = no ship"; "AFK carebear dead carebear" |];
                [| KillTag.NpcKill; KillTag.Expensive |], [| "Bot down!"; "Don't go AFK Mr carebear" |];
                [| KillTag.SoloKill; KillTag.Expensive|], [| "NICE"; "A thing of beauty is a joy forever" |];
                [| KillTag.SoloKill; KillTag.Spendy|], [| "Who said solo is dead?"; "Someone worked hard for his ship." |];
                [| KillTag.SoloKill; KillTag.CorpKill |], [| "GREAT VICTORY"; "GLORIOUS VICTORY"; "Glorious solo victory"; |];
                [| KillTag.EcmFitted; KillTag.CorpLoss |], [| "Someone needs to be fired"; "Someone needs to be call the boss. Deviant found"; |];
                [| KillTag.EcmFitted; KillTag.CorpKill |], [| "Deserves a plex for this"; |];
                [| KillTag.CorpLoss |], [| "CORPIE DOWN"; "RIP" |];
                [| KillTag.CorpKill |], [| "GREAT VICTORY"; "GLORIOUS VICTORY" |];
                [| KillTag.Pod |], [| "Someone should have bought pod insurance"; "Someone needs his implants back"; |];
                [| KillTag.PlexInHold |], [| "Plex in hold!"; "BWAHAHAHAHAHA!"; "Plex vaults - they exist"; "RMT DOWN" |];
                [| KillTag.SkillInjectorInHold |], [| "Skill injector in hold!"; "FFS"; "No comment needed" |];
                [| KillTag.AwoxKill |], [| "Ooooh... Awox"; "Should have checked his API"; "Didn't like that corp anyway" |];
                [| KillTag.EcmFitted |], [| "ECM is illegal"; "Doing God's work" |];
                [| KillTag.Expensive |], [| "DERP"; "Oh dear, how sad, never mind"; "Someone's gonna be crying" |];
                [| KillTag.Spendy |], [| "Oops"; "DEPLOY CREDIT CARD" |];
            |]

        // TODO: not necessary...
        let tags = tagTexts |> Seq.map (fun x -> x.Key)

        let pickText (rnd: System.Random) (texts: string[]) =
            let idx = rnd.Next(0, texts.Length)
            texts.[idx]

        let pickTagText (rnd: System.Random) tag =            
            match tagTexts |> Map.tryFind tag with
            | Some txts -> txts |> pickText rnd
            | _ -> ""
        
        let getTagText = pickTagText (System.Random())
        let getText = pickText (System.Random())

        let getConjunctiveTagsText (text: string[]-> string) (tags: KillTag list)=            
            let tags = tags |> Array.ofSeq
            let matches = conjunctiveTagTexts 
                            |> Seq.filter (fun (ts,_) -> ts |> Seq.conjunctMatch tags)
                            |> Seq.tryHead
            match matches with 
            | Some (_,txts) -> text txts
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


