namespace EveAlod.Services

    open System
    open EveAlod.Common
    open EveAlod.Data

    type KillPublisherActor(forward: string -> unit)=

        let rnd = new System.Random()
        let getTagText = (Tagging.getTagText rnd) |> Tagging.getTagsText 

        let getMsg km = ((getTagText km.Tags) + " " + km.ZkbUri).Trim()

        let pipe = MailboxProcessor<ActorMessage>.Start(fun inbox -> 
            let rec getNext() = async {
                let! msg = inbox.Receive()
                
                match msg with
                        | Publish km ->                        
                            km |> getMsg |> forward
                        | _ ->      
                            ignore 0
                
                return! getNext()
                }
        
            getNext()
        )


        member this.Post(msg: ActorMessage) = pipe.Post msg

