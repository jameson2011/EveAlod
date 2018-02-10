open System
open EveAlod.Data
open EveAlod.Common.CommandLine
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
            
        app
        

    [<EntryPoint>]
    let main argv =
        let app = createAppTemplate()
    
        try
                app.Execute(argv)
        with    
        | ex -> Console.Error.WriteLine ex.Message   
                2