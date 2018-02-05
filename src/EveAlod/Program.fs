open System
open EveAlod.Data
open EveAlod

[<EntryPoint>]
let main argv =
            
    try               
        let factory = EveAlod.Services.ServiceFactory()
        let log = factory.Log

        let source = factory.KillSource
        
        source.Start()
        
        ActorMessage.Info "Started EveAlod." |> log

        System.Console.Out.WriteLine("ENTER to quit")
        System.Console.ReadLine() |> ignore

        ActorMessage.Info "Stopping EveAlod..." |> log
        source.Stop()
        ActorMessage.Info "Stopped EveAlod." |> log

        0
    
    with e -> 
        Console.Error.WriteLine e.Message
        2