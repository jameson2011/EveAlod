namespace EveAlod.Valuation

open System
open MongoDB.Driver
open EveAlod.Common
open EveAlod.Common.MongoDb

type MongoDataWriter(connection: MongoConnection)=

    let col =   
        lazy (
                EveAlod.Common.MongoDb.initCollection connection
            )
    
    let insertOne = col.Value.InsertOne

    // TODO: WIP
    member __.Write(value) =        
        ignore 0