namespace EveAlod.ValuationService

open System
open EveAlod.Common.CommandLine
open EveAlod.Valuation

module CommandLine=

    let private dbServerArg = "svr"
    let private dbNameArg = "db"
    let private dbCollectionArg = "col"
    let private dbUserArg = "un"
    let private dbPasswordArg = "pw"
    let private webPortArg = "port"
    let private killsourceArg = "killsource"

    let addMongoServerArg =             addSingleOption dbServerArg "server" (sprintf "The MongoDB server name. Default: %s" ValuationConfigurationDefault.mongoServer)
    let getMongoServerValue app =       getStringOption dbServerArg app  |> Option.defaultValue ValuationConfigurationDefault.mongoServer
    let addMongoDbArg =                 addSingleOption dbNameArg dbNameArg (sprintf "The MongoDB DB name. Defaut: %s" ValuationConfigurationDefault.mongoDb)
    let getMongoDbValue app =           getStringOption dbNameArg app |> Option.defaultValue ValuationConfigurationDefault.mongoDb
    let addMongoCollectionArg =         addSingleOption dbCollectionArg "collection" (sprintf "The MongoDB DB collection name. Default: %s" ValuationConfigurationDefault.mongoCollection )
    let getMongoCollectionValue app =   getStringOption dbCollectionArg app |> Option.defaultValue ValuationConfigurationDefault.mongoCollection
    let addMongoUserArg =               addSingleOption dbUserArg "user" "User name for MongoDB. Default: no auth is assumed."
    let getMongoUserValue app =         getStringOption dbUserArg app  |> Option.defaultValue ""
    let addMongoPasswordArg =           addSingleOption dbPasswordArg "password" "MongoDB user password"
    let getMongoPasswordValue app =     getStringOption dbPasswordArg app |> Option.defaultValue ""
    
    let addKillSourceArg =              addSingleOption killsourceArg killsourceArg "The URI providing zKB kills. By default this is zKB RedisQ"
    let getKillSourceValue app =        getStringOption killsourceArg app |> Option.defaultValue ValuationConfigurationDefault.killSourceUri

    let addWebPortArg =                 addSingleOption webPortArg webPortArg (sprintf "The proxy's web server port. Default: %i" ValuationConfigurationDefault.webPort )
    let getWebPortValue app =     
        match getStringOption webPortArg app with
        | None  -> ValuationConfigurationDefault.webPort
        | Some x -> 
            match UInt16.TryParse(x) with
            | (true,x) -> x
            | _ -> failwith "Invalid port."

    let createApp()=
        let app = app()
        app.Name <- "EveAlod.ValuationService"
        app.Description <- "Provide valuations of kills"

        setHelp app        

    
    let addRun cmd (app: App) =
        let f = setDesc "Run the service" 
                        >> addKillSourceArg
                        >> addWebPortArg
                        >> addMongoServerArg
                        >> addMongoDbArg
                        >> addMongoCollectionArg
                        >> addMongoUserArg
                        >> addMongoPasswordArg
                        >> setAction cmd
        app.Command("run", (composeAppPipe f)) |> ignore
        app

