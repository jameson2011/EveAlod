namespace EveAlod
    
module CommandLine=        
        
    open EveAlod.Common.CommandLine


    let createApp() =
        let app = app()
        app.Name <- "EveAlod"
        app.Description <- "Find and publish awful losses"
        app    
    
        
    // verbs:
    let addRun cmd (app: App) =
        let f = setDesc "Run the service" 
                        >> setAction cmd
        app.Command("run", (composeAppPipe f)) |> ignore
        app

    let create() =
        createApp()
        |> setHelp
        
