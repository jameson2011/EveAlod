namespace EveAlod.Valuation

open System
open MongoDB.Driver
open EveAlod.Common.Bson
open EveAlod.Common.MongoDb

type MongoDataWriter(serverName: string, dbName: string, colName: string)=

    let col =   
        lazy (
                connectionString serverName
                        |> EveAlod.Common.MongoDb.initCollection dbName colName
            )
    
    let insertOne = col.Value.InsertOne

    // TODO: need to set.... 

    member __.Write(stats) =
        // TODO:
        ignore 0