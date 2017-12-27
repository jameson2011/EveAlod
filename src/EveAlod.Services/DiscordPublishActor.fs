namespace EveAlod.Services

    open System
    open EveAlod.Entities

    type DiscordPublishActor(channel: DiscordChannel, wait: TimeSpan)= 
        
        let rnd = new System.Random()
        let getTagText = (Tagging.getTagText rnd) |> Tagging.getTagsText 

        let getSleep(lastSend: DateTime) =
            let sinceLast = DateTime.UtcNow - lastSend
            match int ((wait - sinceLast).TotalMilliseconds) with
            | x when x < 0 -> 0
            | x -> x
            
            

        let pipe = MailboxProcessor<ActorMessage>.Start(fun inbox -> 
            let rec getNext(lastSend: DateTime) = async {
                let! msg = inbox.Receive()

                match msg with
                | SendToDiscord km ->                        
                    let toWait = getSleep lastSend
                    let! r = Async.Sleep(toWait)

                    let txt = ((getTagText km.Tags) + " " + km.ZkbUri).Trim()
                    
                    let! response = EveAlod.Data.Web.sendDiscord channel.Id channel.Token txt
                    0 |> ignore                      
                | _ ->      0 |> ignore
                
                return! getNext(DateTime.UtcNow)
                }
        
            getNext(DateTime.MinValue)
        )
                
        member this.Start() = pipe.Post Start
        
        member this.Stop() = pipe.Post Stop

        member this.Post(msg: ActorMessage) = pipe.Post msg