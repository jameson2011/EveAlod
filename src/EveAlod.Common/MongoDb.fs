namespace EveAlod.Common

open System
open MongoDB.Bson
open MongoDB.Driver

module MongoDb=
    
    let connectionString server = 
        sprintf "mongodb://%s:27017" server            
        
    let pingDb (db: IMongoDatabase) = 
        new MongoDB.Driver.BsonDocumentCommand<Object>(BsonDocument.Parse("{ping:1}"))
                    |> db.RunCommand 
                    |> ignore
        db

    let initDb dbName (connection: string) =            
        let client= MongoClient(connection)                                            
        let db = client.GetDatabase(dbName)                        
        db |> pingDb
        

    let setIndex (path: string) (collection: IMongoCollection<'a>) =                        
        let json = sprintf "{'%s': 1 }" path
        let def = IndexKeysDefinition<'a>.op_Implicit(json)
            
        collection.Indexes.CreateOne(def) |> ignore

        collection
        
    let getCollection colName (db: IMongoDatabase) =
        db.GetCollection(colName)                
                            
    let initCollection server dbName collectionName =
        server
        |> connectionString
        |> initDb dbName
        |> getCollection collectionName
        

module Bson =
    open MongoDB.Bson.IO
    open MongoDB.Bson.Serialization

    let ofJson (json: string) =
        BsonSerializer.Deserialize<BsonDocument>(json)
        
    let toJson (bson: BsonDocument) =            
        let jsonWriterSettings = JsonWriterSettings()
        jsonWriterSettings.OutputMode <- JsonOutputMode.Strict
        jsonWriterSettings.Indent <- true
        jsonWriterSettings.IndentChars <- " "
        bson.ToJson(jsonWriterSettings)
                        
    let getObjectId (bson: BsonDocument)=            
        bson.Elements 
        |> Seq.filter (fun e -> e.Name = "_id")
        |> Seq.head
        |> (fun id -> id.Value.AsObjectId)

