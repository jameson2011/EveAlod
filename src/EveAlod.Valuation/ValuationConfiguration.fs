namespace EveAlod.Valuation

module ValuationConfigurationDefault =
    let killSourceUri = "https://redisq.zkillboard.com/listen.php?ttw=10"
    let webPort = 81us
    let mongoServer = "127.0.0.1"
    let mongoDb = "evealodvaluation"
    let mongoCollection = "stats"
    let rollingStatsAge = 365

type ValuationConfiguration= 
    {
        KillSourceUri: string
        WebPort: uint16
        MongoServer: string
        MongoDb: string
        MongoCollection: string
        MongoUser: string
        MongoPassword: string
        MaxRollingStatsAge: int
    } with
    static member Empty = { KillSourceUri = ValuationConfigurationDefault.killSourceUri;
                            WebPort = ValuationConfigurationDefault.webPort;
                            MongoServer = ValuationConfigurationDefault.mongoServer;
                            MongoDb = ValuationConfigurationDefault.mongoDb;
                            MongoCollection = ValuationConfigurationDefault.mongoCollection;
                            MongoUser = "";
                            MongoPassword = "";
                            MaxRollingStatsAge = ValuationConfigurationDefault.rollingStatsAge
                            }

