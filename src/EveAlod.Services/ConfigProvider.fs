namespace EveAlod.Services
    
    open System.IO
    open FSharp.Data
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
                ChannelToken = c.ChannelToken
            }

        member this.Configuration() = loadConfig configFilePath

