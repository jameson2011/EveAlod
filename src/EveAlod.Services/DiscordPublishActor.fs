namespace EveAlod.Services

    open EveAlod.Entities

    type DiscordPublishActor(channel: DiscordChannel)= 
        let getTagText tag =
            match tag with
            | CorpLoss -> "CORPIE DOWN RIP"
            | CorpKill -> "GREAT VICTORY"
            | Expensive -> "DERP"
            | PlexInHold -> "BWAHAHAHAHAHA"
            | SkillInjectorInHold -> "RETARD DOWN"
            | Awox -> "Didn't like that corp anyway"
            | _ -> ""

        let getTagsText (tags: KillTag list) =
            match tags with
            | [] -> ""
            | head::_ -> getTagText head
            
        let pipe = MailboxProcessor<ActorMessage>.Start(fun inbox -> 
            let rec getNext() = async {
                let! msg = inbox.Receive()

                match msg with
                | SendToDiscord km ->    
                            
                            let txt = ((getTagsText km.Tags) + " " + km.ZkbUri).Trim()

                            let! response = EveAlod.Data.Web.sendDiscord channel.Id channel.Token txt
                            0 |> ignore                      
                | _ ->      0 |> ignore
                
                return! getNext()            
                }
        
            getNext()
        )
                
        member this.Start() = pipe.Post Start
        
        member this.Stop() = pipe.Post Stop

        member this.Post(msg: ActorMessage) = pipe.Post msg