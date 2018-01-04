namespace EveAlod.Services

    open System
    open EveAlod.Data

    type DiscordPublishActor(channel: DiscordChannel, defaultWait: TimeSpan)= 
        
        let rnd = new System.Random()
        let getTagText = (Tagging.getTagText rnd) |> Tagging.getTagsText 
    
        let sendToDiscord (wait: TimeSpan) (km: Kill) : Async<TimeSpan> =
            async {
                let! r = Async.Sleep(int wait.TotalMilliseconds)
               
                let txt = ((getTagText km.Tags) + " " + km.ZkbUri).Trim()
                    
                let! wait, response = EveAlod.Common.Web.sendDiscord channel.Id channel.Token txt
                let result = match wait with
                                | x when x < defaultWait -> 
                                    defaultWait
                                | x -> x
                
                // Do not try to resend. The queue will accumulate, and keeping Discord happy is more important
                
                return result
            }

        let pipe = MailboxProcessor<ActorMessage>.Start(fun inbox -> 
            let rec getNext(wait: TimeSpan) = async {
                let! msg = inbox.Receive()

                let! nextWait = async {
                                        match msg with
                                                | SendToDiscord km ->                        
                                                    let! wait = sendToDiscord wait km
                                                    return wait
                                                | _ ->      
                                                    return TimeSpan.Zero
                                        }
                return! getNext(nextWait)
                }
        
            getNext(TimeSpan.Zero)
        )
                
        member this.Start() = pipe.Post Start
        
        member this.Stop() = pipe.Post Stop

        member this.Post(msg: ActorMessage) = pipe.Post msg