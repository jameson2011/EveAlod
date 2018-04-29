namespace EveAlod.Services

    open EveAlod.Common
    open EveAlod.Data

    module Commentary=        
        
        let private conjunctiveTagTexts =
            [|                
                [| KillTag.NpcKill; KillTag.Spendy |], [| "No permit = no ship"; "AFK carebear dead carebear"; "Someone worked hard for this"; |];
                [| KillTag.NpcKill; KillTag.Expensive |], [| "Bot down!"; "Don't go AFK Mr carebear" |];
                [| KillTag.Pod; KillTag.CorpKill; KillTag.Expensive |], [| "Remember: we extract all implants at 50% cost"; |];
                [| KillTag.Pod; KillTag.CorpKill; KillTag.Spendy |], [| "Nom nom nom"; |];
                [| KillTag.SoloKill; KillTag.CorpKill; KillTag.Expensive |], [| "NICE"; |];
                [| KillTag.SoloKill; KillTag.CorpKill; KillTag.Spendy |], [| "GLORIOUS"; |];
                [| KillTag.SoloKill; KillTag.CorpKill |], [| "GREAT VICTORY"; "GLORIOUS VICTORY"; "Glorious solo victory"; |];
                [| KillTag.SoloKill; KillTag.Expensive|], [| "NICE"; "A thing of beauty is a joy forever" |];
                [| KillTag.SoloKill; KillTag.Spendy|], [| "Who said solo is dead?";  |];
                [| KillTag.EcmFitted; KillTag.CorpLoss |], [| "Someone needs to be fired"; "Call the boss: PADDLING INCOMING"; |];
                [| KillTag.EcmFitted; KillTag.CorpKill |], [| "Deserves Plex for this"; |];
                [| KillTag.CorpLoss; KillTag.CorpKill |], [| "Oh dear"; |];
                [| KillTag.CorpLoss |], [| "CORPIE DOWN"; "RIP" |];
                [| KillTag.CorpKill |], [| "GREAT VICTORY"; "GLORIOUS VICTORY" |];
                [| KillTag.Pod; KillTag.Expensive |], [| "Pods aren't this expensive, right?"; "Someone should have bought pod insurance"; "Someone needs his implants back"; |];
                [| KillTag.Pod; KillTag.Spendy |], [| "Blame the lag"; "Someone should have bought pod insurance"; "Someone needs his implants back"; |];
                [| KillTag.Pod |], [| "Someone should have bought pod insurance"; "Someone needs his implants back"; |];
                [| KillTag.PlexInHold |], [| "Plex in hold!"; "BWAHAHAHAHAHA!"; "Plex vaults - they exist"; "RMT DOWN" |];
                [| KillTag.SkillInjectorInHold |], [| "Skill injector in hold!"; "FFS"; "No comment needed" |];
                [| KillTag.AwoxKill |], [| "Ooooh... Awox"; "Should have checked his API"; "Didn't like that corp anyway" |];
                [| KillTag.EcmFitted |], [| "ECM is illegal"; "Doing God's work" |];
                [| KillTag.Expensive |], [| "DERP"; "Oh dear, how sad, never mind"; "Someone's gonna be crying" |];
                [| KillTag.Spendy |], [| "Oops"; "DEPLOY CREDIT CARD" |];
                [| KillTag.MissingLows |], [| "Low slots exist" |];
                [| KillTag.MissingMids |], [| "Mid slots exist" |];
                [| KillTag.MissingRigs |], [| "Rig slots exist" |];
            |]

        
        let pickText (rnd: System.Random) (texts: string[]) =
            let idx = rnd.Next(0, texts.Length)
            texts.[idx]
                   
        let getText = pickText (System.Random())

        let getTagsText (text: string[]-> string) (tags: KillTag list)=            
            let tags = tags |> Array.ofSeq
            let matches = conjunctiveTagTexts 
                            |> Seq.filter (fun (ts,_) -> ts |> Seq.conjunctMatch tags)
                            |> Seq.tryHead
            match matches with 
            | Some (_,txts) -> text txts
            | _ -> ""
        

