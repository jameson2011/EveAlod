namespace EveAlod.Services

    open System.IO
    open EveAlod.Common
    open EveAlod.Data
    
    type KillDumpActor(log: PostMessage, folder)=
        
        let logException = Actors.postException typeof<KillSourceActor>.Name log
        let zkbKillsFolder = folder |> IO.combine "zkb" |> IO.createDirectory 
        let alodKillsFolder = folder |> IO.combine "evealod"|> IO.createDirectory 
                
        // in future replace with a DB
        let writeJson folder (id: string, json: string) =
            let name = sprintf "%s.json" id
            let filePath = Path.Combine(folder, name)
            File.AppendAllText(filePath, json)
            
        let writeKill folder = Serialization.killToJson >> writeJson folder

        let pipe = MessageInbox.Start(fun inbox -> 
            let rec getNext() = async {
                
                let! msg = inbox.Receive()

                try
                    match msg with
                    | KillJson json -> 
                        [   ("0", System.Environment.NewLine);
                            ("0", json);                            
                            ("0", System.Environment.NewLine);
                        ] |> List.iter (writeJson zkbKillsFolder)
                    | Kill km -> 
                        km |> writeKill alodKillsFolder
                    | _ -> ignore 0
                with e -> logException e

                return! getNext()
            }  
            getNext()
        )

        do pipe.Error.Add(logException)

        member this.Post(msg: ActorMessage) = pipe.Post msg
