namespace EveAlod.Valuation

open System
open MongoDB.Driver
open EveAlod.Common
open EveAlod.Common.MongoDb
open EveAlod.Data

type MongoWriteActor(log: PostMessage, config: ValuationConfiguration)=
    
    let logException = (Actors.postException typeof<MongoWriteActor>.Name log)
    let logTrace msg = msg |> fun s -> ActorMessage.Trace (typeof<MongoWriteActor>.Name, s) |> log

    let connection = { MongoConnection.Empty with 
                            Server = config.MongoServer;
                            DbName = config.MongoDb;
                            UserName = config.MongoUser;
                            Password = config.MongoPassword;                            
                            CollectionName = "shiptypestats" }

    let shipTypeStatsCollection =   
        lazy (
                sprintf "Initialising Mongo %s.%s..." connection.Server connection.DbName |> ActorMessage.Info |> log
                let result = EveAlod.Common.MongoDb.initCollection connection
                            |> EveAlod.Common.MongoDb.setIndex "{ Document.shipType: 1, Document.period: 1, { unique: true } }"
                "Mongo initialisation done." |> ActorMessage.Info |> log
                result
            )
    
    let keyJson(shipTypeId: string) (date: DateTime)=
        sprintf "shipType: %s, period: ISODate(\"%s\")" shipTypeId (date.ToString("o"))
        

    let countJson (stats: ValueStatistics)=
        sprintf "count: %i" stats.Count

    let statsJson prefix (stats: ValueStatistics)=        
        [
            sprintf "TotalValue: %f" stats.TotalValue;
            sprintf "AverageValue: %f" stats.AverageValue;
            sprintf "MinValue: %f" stats.MinValue;
            sprintf "MaxValue: %f" stats.MaxValue;
            sprintf "ValueRange: %f" stats.ValueRange;
            sprintf "MedianValue: %f" stats.MedianValue;
        ] 
        |> Seq.map (fun s -> prefix + s)
        |> Strings.join ", "
        
        

    let setJson (shipTypeId: string) (date: DateTime) (fittedStats: ValueStatistics) (totalStats: ValueStatistics)=
        let key = keyJson shipTypeId date
        let countJson = countJson fittedStats
        let fittedStats = statsJson "fitted" fittedStats
        let totalStats = statsJson "total" totalStats
        sprintf "{ $set: { %s, %s, %s, %s } }" key countJson fittedStats totalStats
    
    let queryJson(shipTypeId: string) (date: DateTime) = 
        keyJson shipTypeId date |> sprintf "{ %s }"

    let upsertShipTypeStats (shipTypeId: string) (fitted: ValueStatistics) (total: ValueStatistics) (date: DateTime)=
        try
            let opts = UpdateOptions()
            opts.IsUpsert <- true
        
            let query = queryJson shipTypeId date |> Bson.ofJson |> FilterDefinition.op_Implicit
            let set = setJson shipTypeId date fitted total |> Bson.ofJson |> UpdateDefinition.op_Implicit
           
            shipTypeStatsCollection.Value.UpdateMany(query, set, opts) |> ignore                

            sprintf "Written for %s %s" shipTypeId (date.ToShortDateString()) |> logTrace
        with
        | e -> logException e
    
    let pipe = MailboxProcessor<string * DateTime * ValueStatistics * ValueStatistics>.Start(fun inbox -> 
        let rec loop() = async {
            let! shipTypeId, date, fittedStats, totalStats = inbox.Receive()

            DateTime(date.Year, date.Month, date.Day,0,0,0,DateTimeKind.Utc)
                    |> upsertShipTypeStats shipTypeId fittedStats totalStats

            return! loop()
            }
        loop()
        )
    
    do pipe.Error.Add(logException)

    member __.Write(shipTypeId: string) (date: DateTime) (stats: ValueStatistics * ValueStatistics) =
        let fitted, total = stats
        pipe.Post(shipTypeId, date, fitted, total)
        
