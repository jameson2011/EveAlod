namespace EveAlod.Services
    
    open System.IO
    open FSharp.Data
    open EveAlod.Common.IO
    open EveAlod.Common.Strings
    open EveAlod.Data

    type JsonConfigProvider = JsonProvider<"./SampleConfig.json">

    type ConfigProvider()=
        
        let configFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
        let configFilePath = Path.Combine(configFolder, "settings.json")
       
        let loadConfig(filePath: string) =
            let c = JsonConfigProvider.Load filePath
            { Configuration.MinimumScore = (float c.MinimumScore);
                CorpId = c.CorpId;
                ChannelId = c.ChannelId;
                ChannelToken = c.ChannelToken;
                KillSourceUri = c.KillSourceUri;
            }

        let setKillSourceUri(config: Configuration)=
            let uri = match config.KillSourceUri with     
                        | NullOrWhitespace _ -> "https://redisq.zkillboard.com/listen.php?ttw=10"
                        | u -> u
            { config with KillSourceUri = uri }

        member __.Configuration() = loadConfig configFilePath |> setKillSourceUri

