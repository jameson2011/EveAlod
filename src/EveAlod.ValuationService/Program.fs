open System

open Suave
open EveAlod.Valuation
open EveAlod.ValuationService
open EveAlod.ValuationService.CommandLine

let private configFromStartApp(app)=
    { EveAlod.Valuation.Configuration.Empty with
        KillSourceUri = getKillSourceValue app;
        MongoServer = getMongoServerValue app;
        MongoDb = getMongoDbValue app;
        MongoCollection = getMongoCollectionValue app;
        MongoUser = getMongoUserValue app;
        MongoPassword = getMongoPasswordValue app;
        WebPort = getWebPortValue app
        } 

let private runService (app)= 
    
    let config = configFromStartApp app
    let cts = new System.Threading.CancellationTokenSource()
        
    // TODO: 
    //"Starting web app..." |> ActorMessage.Info |> logger.Post        
    let listening,server = startWebServerAsync (WebApp.webConfig config) WebApp.webRoutes

    Async.Start(server, cts.Token)
        
    
    System.Console.Out.WriteLine("ENTER to quit")
    System.Console.ReadLine() |> ignore

    true

let private createAppTemplate()=
    CommandLine.createApp()
        |> CommandLine.addRun runService    
        

[<EntryPoint>]
let main argv = 
    let app = createAppTemplate()
    
    try
            app.Execute(argv)
    with    
    | ex -> Console.Error.WriteLine ex.Message   
            2