namespace EveAlod.Services

    open System
    open EveAlod.Data

    type ServiceFactory()=
    
        let staticDataProvider = StaticEntityProvider() :> IStaticEntityProvider
        let logger = LogPublishActor()
        
        do ActorMessage.Info "Starting EveAlod..." |> logger.Post
        
        let configProvider = ConfigProvider(logger.Post, staticDataProvider)

        let config = configProvider.Configuration() 
                                    |> Config.validate
                                    |> Config.report logger.Post
        

        let mainChannel = { DiscordChannel.Id = config.ChannelId; Token = config.ChannelToken}

        let staticDataActor = StaticDataActor(logger.Post, staticDataProvider)
        
        let discordPublisher = DiscordPublishActor(logger.Post, mainChannel, EveAlod.Common.Discord.sendJsonContentDiscord)
        
        let killPublisher = KillPublisherActor(logger.Post, 
                                                    DiscordKillMessageBuilder(staticDataActor, config.CorpId), 
                                                    [ discordPublisher.Post; ] |> Actors.forwardMany (SendToDiscord))
        
        let killFilter = KillFilterActor(logger.Post, 
                                                config.MinimumScore, 
                                                [ killPublisher.Post; ] |> Actors.forwardMany (Killmail))

        let killScorer = KillScorerActor(logger.Post, 
                                                [ logger.Post; killFilter.Post ] |> Actors.forwardMany (Killmail)) 
        
        let killTagger = KillTaggerActor(logger.Post, 
                                                KillTagger(config), 
                                                [ killScorer.Post ] |> Actors.forwardMany (Killmail))
        
        let killValuationActor = KillValuationActor(config, logger.Post,
                                                    [ killTagger.Post ] |> Actors.forwardMany (Killmail))

        let killBlacklist = KillBlacklistActor(config, logger.Post, 
                                                [ killValuationActor.Post ] |> Actors.forwardMany (Killmail) )


        let killTransform = KillTransformActor(logger.Post, 
                                                [ killBlacklist.Post ] |> Actors.forwardMany (Killmail))
               
        let killSource = KillSourceActor(logger.Post,
                                                [ killTransform.Post ] |> Actors.forwardMany (ActorMessage.KillmailJson) ,
                                                EveAlod.Common.Web.getData,
                                                config.KillSourceUri)
                                            
        member this.KillSource = killSource

        member this.Log = logger.Post
