namespace EveAlod.Services
    
    open System.IO
    open FSharp.Data
    open EveAlod.Common.IO
    open EveAlod.Common.Strings
    open EveAlod.Data

    type JsonConfigProvider = JsonProvider<"./SampleConfig.json">

    type ConfigProvider(log: PostMessage, dataProvider: IStaticEntityProvider)=
        
        let configFilePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
                                |> combine "settings.json"

        let info = ActorMessage.Info >> log
       
        let loadConfig(filePath: string) =
            let c = JsonConfigProvider.Load filePath
            
            { Configuration.MinimumScore = (float c.MinimumScore);
                CorpTicker = c.CorpTicker;
                CorpId = c.CorpId;
                DiscordWebhookUri = c.DiscordWebhookUri;
                ChannelId = c.ChannelId;
                ChannelToken = c.ChannelToken;
                KillSourceUri = c.KillSourceUri;
            }

        let getCorpId ticker = 
            let c = dataProvider.CorporationByTicker ticker |> Async.RunSynchronously
            match c with 
            | None ->  failwith ("Corp ticker not found!")
            | Some corp -> corp.Id

        let getDiscordChannel webhook = 
            match (EveAlod.Common.Discord.getDiscordChannel webhook |> Async.RunSynchronously) with
            | Choice1Of2 (id, token) -> { DiscordChannel.Id = id; Token = token } 
            | Choice2Of2 msg -> failwith ("Error connecting to Discord: " + msg)

        let applyDiscord config = 
            match config.DiscordWebhookUri with 
            | NullOrWhitespace _ -> config
            | u ->  sprintf "Resolving Discord webhook %s..." u |> info
                    let channel = getDiscordChannel u
                    info "Discord webhook resolved."
                    { config with ChannelId = channel.Id; ChannelToken = channel.Token }

        let applyCorp config = 
            match config.CorpTicker with
            | NullOrWhitespace _ -> config
            | ticker ->     sprintf "Resolving corp ticker %s..." ticker |> info
                            let corpId = getCorpId ticker       
                            info "Corp ticker resolved."
                            { config with CorpId = corpId }
            
        let setKillSourceUri(config: Configuration)=
            let uri = match config.KillSourceUri with     
                        | NullOrWhitespace _ -> "https://redisq.zkillboard.com/listen.php?ttw=10"
                        | u -> u
            { config with KillSourceUri = uri }       
            
        member __.Configuration() = 
            loadConfig configFilePath 
                |> applyDiscord
                |> applyCorp
                |> setKillSourceUri                
                
