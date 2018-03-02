namespace EveAlod.Valuation

open EveAlod.Common

module Mongo=    
    
    let connection (config: ValuationConfiguration) = 
            { MongoConnection.Empty with 
                Server = config.MongoServer;
                DbName = config.MongoDb;
                UserName = config.MongoUser;
                Password = config.MongoPassword;                            
                CollectionName = "shiptypestats" }

        

