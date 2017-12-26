namespace EveAlod.Services
    
    open System.IO
    open FSharp.Data
    open EveAlod.Entities

    type configJson = JsonProvider<"./SampleConfig.json">

    type ConfigProvider()=
        
        let configFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
        let configFileName = "settings.json"
        let configFilePath = Path.Combine(configFolder, configFileName)
       
        let loadConfig(filePath: string) =
            let c = configJson.Load filePath
            { Configuration.MinimumScore = (float c.MinimumScore);
                CorpId = c.CorpId;
                ChannelId = c.ChannelId;
                ChannelToken = c.ChannelToken
            }

        member this.Configuration() = loadConfig configFilePath

