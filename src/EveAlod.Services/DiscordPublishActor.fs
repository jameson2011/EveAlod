namespace EveAlod.Services

    open EveAlod.Entities

    type DiscordPublishActor(channel: DiscordChannel)= 
        
            
        let pipe = MailboxProcessor<ActorMessage>.Start(fun inbox -> 
            let rec getNext() = async {
                let! msg = inbox.Receive()

                match msg with
                | SendToDiscord km ->    
                    let txt = ((Tagging.getTagsText km.Tags) + " " + km.ZkbUri).Trim()

                    // TODO: add timer! 1 min or so?
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