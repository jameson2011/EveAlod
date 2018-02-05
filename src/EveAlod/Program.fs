open System
open EveAlod.Data

open EveAlod

module Program=

    let private runService (app)= 
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

        true


    let private createAppTemplate()=
        let app = CommandLine.createApp()
                    |> CommandLine.addRun runService
                    |> CommandLine.setHelp
    
        app.OnExecute(fun () -> app.ShowHelp()
                                0)
        app
        

    [<EntryPoint>]
    let main argv =
        let app = createAppTemplate()
    
        app.OnExecute(fun () -> app.ShowHelp()
                                0)
        try
                app.Execute(argv)
        with    
        | ex -> Console.Error.WriteLine ex.Message   
                2