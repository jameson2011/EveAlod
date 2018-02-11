namespace EveAlod.Common

open System
open MongoDB.Bson
open MongoDB.Driver

module MongoDb=
    
    let connectionString server = 
        sprintf "mongodb://%s:27017" server            
        
    let initDb dbName (connection: string) =            
        let client= MongoClient(connection)                                            
        let db = client.GetDatabase(dbName)
                
        new MongoDB.Driver.BsonDocumentCommand<Object>(BsonDocument.Parse("{ping:1}"))
                    |> db.RunCommand |> ignore
        db

    let setIndex (path: string) (collection: IMongoCollection<'a>) =                        
        let json = sprintf "{'%s': 1 }" path
        let def = IndexKeysDefinition<'a>.op_Implicit(json)
            
        let r = collection.Indexes.CreateOne(def)

        collection
        
    let getCollection colName (db: IMongoDatabase) =
        db.GetCollection(colName)                
                            
    let defaultCollection server dbName collectionName =
        server
        |> connectionString
        |> initDb dbName
        |> getCollection collectionName
        |> setIndex "_v.package.killID"

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

