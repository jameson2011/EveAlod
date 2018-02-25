open System

open Suave
open EveAlod.Valuation
open EveAlod.ValuationService
open EveAlod.ValuationService.CommandLine

let private backfillConfig(app) =    
    { EveAlod.Valuation.Backfill.BackfillConfiguration.Empty with 
            From = getFromDateValue app; 
            To = getToDateValue app; 
            DestinationUri = match Uri.TryCreate(getDestinationUriValue app, UriKind.Absolute) with
                                | (true, uri) -> uri
                                | _ -> failwith "Invalid URI";
            Sampling = getSamplingValue app
            }



let private runBackfill(app) =
    let config = backfillConfig app
    if config.From >= config.To then    
        failwith "From must precede To."
    
    let sf = Backfill.ServiceFactory(config)

    let crawler = sf.Crawler

    crawler.Start()

    System.Console.Out.WriteLine("ENTER to quit")
    System.Console.ReadLine() |> ignore
    
    true

let private valuationConfig(app)=
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
    
    let config = valuationConfig app
    let cts = new System.Threading.CancellationTokenSource()
    
    let serviceFactory = ValuationServiceFactory(config)
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