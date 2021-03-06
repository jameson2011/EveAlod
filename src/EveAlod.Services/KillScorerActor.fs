﻿namespace EveAlod.Services

    open EveAlod.Data
    
    type KillScorerActor(log: PostMessage, forward: PostKill) =

        let pipe = MessageInbox.Start(fun inbox -> 
            let rec getNext() = async {
                let! msg = inbox.Receive()

                match msg with                                    
                | Killmail km ->    
                            try
                                let score = Scoring.score km
                                let km = { km with AlodScore = score}

                                forward km
                            with e -> Actors.postException typeof<KillScorerActor>.Name log e
                | _ ->      0 |> ignore
                
                return! getNext()            
                }
        
            getNext()
        )

        do pipe.Error.Add(Actors.postException typeof<KillScorerActor>.Name log)
                
        member this.Start() = pipe.Post Start
        
        member this.Stop() = pipe.Post Stop
        
        member this.Post(msg: ActorMessage) = pipe.Post msg

