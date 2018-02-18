namespace EveAlod.Common

open System
open MongoDB.Bson
open MongoDB.Driver
open EveAlod.Common.Strings

type MongoConnection = 
    {
        Server: string
        DbName: string
        CollectionName: string
        UserName: string
        Password: string
    }

module MongoDb=
    let private defaultMongoPort = 27017
    
    let appendPort server = 
        match server |> split ":" with
        | [| name; port |] -> server
        | _ -> sprintf "%s:%i" server defaultMongoPort            

    let resolveServerPorts = 
        split "," >> Seq.map appendPort >> join ","
    
    let connectionString userName password server = 
        let servers = resolveServerPorts server
        match userName with
        | NullOrWhitespace _ -> sprintf "mongodb://%s" servers
        | name -> sprintf "mongodb://%s:%s@%s" name password servers
    
    let setDbConnection dbName connectionString =
        match dbName with
        | NullOrWhitespace _ -> connectionString
        | x -> sprintf "%s/%s" connectionString dbName
                
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
                            
    let initCollection (connection: MongoConnection)=
        connectionString connection.Server connection.UserName connection.Password
        |> setDbConnection connection.DbName
        |> initDb connection.DbName
        |> getCollection connection.CollectionName
        

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

