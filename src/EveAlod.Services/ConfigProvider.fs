namespace EveAlod.Services
    
    open System
    open System.IO
    open FSharp.Data
    open EveAlod.Common
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
                MinCorpDamage = (float c.MinCorpDamage);
                DiscordWebhookUri = c.DiscordWebhookUri;
                ChannelId = c.ChannelId;
                ChannelToken = c.ChannelToken;
                KillSourceUri = c.KillSourceUri;
                KillValuationUri = c.KillValuationUri;
                ValuationLimit = (float c.ValuationLimit);
                ValuationLowerLimit = (float c.ValuationLowerLimit);
                ValuationSpread = (float c.ValuationSpread);
                IgnoredKillAge = match c.KillAgeHours with
                                    | x when x <= 0 -> TimeSpan.FromDays(365. * 100.)
                                    | x -> TimeSpan.FromHours(float c.KillAgeHours);
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
                        | NullOrWhitespace _ -> Zkb.redisqUri
                        | u -> u
            { config with KillSourceUri = uri }       
            
        let setKillValuationUri(config: Configuration) =
            let uri = match config.KillValuationUri with
                        | NullOrWhitespace _ -> "http://127.0.0.1:81/"
                        | u ->  if not (u.EndsWith("/")) then (u + "/")
                                else u
            { config with KillValuationUri = uri } 
       
        member __.Configuration() = 
            configFilePath
                |> loadConfig  
                |> applyDiscord
                |> applyCorp
                |> setKillSourceUri  
                |> setKillValuationUri
                
