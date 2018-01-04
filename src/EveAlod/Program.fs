// Learn more
open System

[<EntryPoint>]
let main argv =
        
    let factory = new EveAlod.Services.ServiceFactory()
    let source = factory.KillSource

    source.Start()
        
    System.Console.Out.WriteLine("ENTER to quit")
    System.Console.ReadLine() |> ignore

    source.Stop()

    0 // return an integer exit code
    
