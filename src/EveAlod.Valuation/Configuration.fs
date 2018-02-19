namespace EveAlod.Valuation

module ConfigurationDefaults =
    let killSourceUri = "https://redisq.zkillboard.com/listen.php?ttw=10"
    let webPort = 81us
    let mongoServer = "127.0.0.1"
    let mongoDb = "evealodvaluation"
    let mongoCollection = "stats"


type Configuration= 
    {
        KillSourceUri: string
        WebPort: uint16
        MongoServer: string
        MongoDb: string
        MongoCollection: string
        MongoUser: string
        MongoPassword: string
    } with
    static member Empty = { KillSourceUri = ConfigurationDefaults.killSourceUri;
                            WebPort = ConfigurationDefaults.webPort;
                            MongoServer = ConfigurationDefaults.mongoServer;
                            MongoDb = ConfigurationDefaults.mongoDb;
                            MongoCollection = ConfigurationDefaults.mongoCollection;
                            MongoUser = "";
                            MongoPassword = ""
                            }

