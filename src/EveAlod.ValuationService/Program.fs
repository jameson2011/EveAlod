open System

open Suave
open EveAlod.Valuation
open EveAlod.ValuationService
open EveAlod.ValuationService.CommandLine

let private runBackfill(app) =
    let fromDate = getFromDateValue app
    let toDate = getToDateValue app
    if fromDate >= toDate then
        failwith "From must precede To"
    let destinationUri = match Uri.TryCreate(getDestinationUriValue app, UriKind.Absolute) with
                            | (true, uri) -> uri
                            | _ -> failwith "Invalid URI"
    // TODO: 
    true


let private configFromStartValuation(app)=
    { EveAlod.Valuation.ValuationConfiguration.Empty with
        KillSourceUri = getKillSourceValue app;
        MongoServer = getMongoServerValue app;
        MongoDb = getMongoDbValue app;
        MongoCollection = getMongoCollectionValue app;
        MongoUser = getMongoUserValue app;
        MongoPassword = getMongoPasswordValue app;
        WebPort = getWebPortValue app;
        MaxRollingStatsAge = getMaxAgeValue app;
        } 


let private runService (app)= 
    
    let config = configFromStartValuation app
    let cts = new System.Threading.CancellationTokenSource()
    
    let serviceFactory = ServiceFactory(config)
    let logger = serviceFactory.Log

    "Starting kill source..." |> EveAlod.Data.ActorMessage.Info |> logger        
    serviceFactory.Source.Start()

    "Starting web app..." |> EveAlod.Data.ActorMessage.Info |> logger        
    let listening,server = startWebServerAsync (WebApp.webConfig config) (WebApp.webRoutes serviceFactory.ShipStats)

    Async.Start(server, cts.Token)
       
    
    System.Console.Out.WriteLine("ENTER to quit")
    System.Console.ReadLine() |> ignore

    serviceFactory.Source.Stop()

    true

let private createAppTemplate()=
    CommandLine.createApp()
        |> CommandLine.addRun runService    
        |> CommandLine.addBackfill runBackfill
        

[<EntryPoint>]
let main argv = 
    let app = createAppTemplate()
    
    try
            app.Execute(argv)
    with    
    | ex -> Console.Error.WriteLine ex.Message   
            2