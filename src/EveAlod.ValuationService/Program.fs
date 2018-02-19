open System
open EveAlod.Common.CommandLine

let private createApp()=
    let app = app()
    app.Name <- "EveAlod.ValuationService"
    app.Description <- "Provide valuations of kills"
    app        

let private addRun cmd (app: App) =
    let f = setDesc "Run the service" 
                    >> setAction cmd
    app.Command("run", (composeAppPipe f)) |> ignore
    app

let private runService (app)= 

    System.Console.Out.WriteLine("ENTER to quit")
    System.Console.ReadLine() |> ignore

    true

let private createAppTemplate()=
    let app = createApp()
                |> addRun runService                    
                |> setHelp
    
    app
        

[<EntryPoint>]
let main argv = 
    let app = createAppTemplate()
    
    try
            app.Execute(argv)
    with    
    | ex -> Console.Error.WriteLine ex.Message   
            2