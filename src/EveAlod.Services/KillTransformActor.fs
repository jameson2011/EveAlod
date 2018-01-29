namespace EveAlod.Services

    open System
    open EveAlod.Common
    open EveAlod.Data
        
    

    type KillTransformActor(log: PostMessage,
                            forward: PostKill)=

        let logException = Actors.postException typeof<KillTransformActor>.Name log

        let pipe = MessageInbox.Start(fun inbox -> 
            let rec getNext() = async{

                let! msg = inbox.Receive()

                try
                    match msg with
                    | KillJson json -> 
                        match KillTransforms.toKill json with
                        | Some k -> k |> forward
                        | _ ->  ignore 0
                    | _ -> ignore 0
                with e -> logException e

                return! getNext()
                }
       
            getNext()
        )

        do pipe.Error.Add(logException)

        member __this.Post(msg) =
            pipe.Post msg