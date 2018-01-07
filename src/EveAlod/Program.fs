// Learn more
open System
open EveAlod.Data

[<EntryPoint>]
let main argv =
        
    try
        let factory = new EveAlod.Services.ServiceFactory()
        let source = factory.KillSource
        let log = factory.Log

        ActorMessage.Info "Starting EveAlod..." |> log

        source.Start()
        
        System.Console.Out.WriteLine("ENTER to quit")
        System.Console.ReadLine() |> ignore

        ActorMessage.Info "Stopping EveAlod..." |> log

        source.Stop()

        ActorMessage.Info "Stopped EveAlod." |> log

        0
    
    with e -> 
        Console.Error.WriteLine e.Message
        2