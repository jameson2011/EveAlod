namespace EveAlod.Services

    open System.IO
    open EveAlod.Data
    
    type KillDumpActor(log: Post, folder)=
        
        let logException = Actors.postException typeof<KillSourceActor>.Name log

        do
            if not (Directory.Exists folder) then
                Directory.CreateDirectory folder |> ignore

        
        let writeJson (id: string, json: string)=
            let name = sprintf "%s.json" id
            let filePath = Path.Combine(folder, name)
            File.AppendAllText(filePath, json)
            
        let write = Serialization.toJson >> writeJson

        let pipe = MailboxProcessor<ActorMessage>.Start(fun inbox -> 
            let rec getNext() = async {
                
                let! msg = inbox.Receive()

                try
                    match msg with
                    | New km -> 
                        write km                    
                    | _ -> ignore 0
                with e -> logException e

                return! getNext()
            }  
            getNext()
        )

        do pipe.Error.Add(logException)

        member this.Post(msg: ActorMessage) = pipe.Post msg
