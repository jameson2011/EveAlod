namespace EveAlod.Services

    open EveAlod.Entities

    type DiscordPublishActor(channel: DiscordChannel)= 
        let pipe = MailboxProcessor<ActorMessage>.Start(fun inbox -> 
            let rec getNext() = async {
                let! msg = inbox.Receive()

                match msg with                            
                | SendToDiscord km ->    
                            // TODOTK: error? & inject
                            let! response = EveAlod.Data.Web.sendDiscord channel.Id channel.Token km.ZkbUri
                            0 |> ignore                      
                | _ ->      0 |> ignore
                
                return! getNext()            
                }
        
            getNext()
        )
                
        member this.Start() = pipe.Post Start
        
        member this.Stop() = pipe.Post Stop

        member this.Post(msg: ActorMessage) = pipe.Post msg