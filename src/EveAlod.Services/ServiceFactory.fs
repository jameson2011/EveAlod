namespace EveAlod.Services

    open System
    open EveAlod.Data

    type ServiceFactory()=
    
        let dataProvider = StaticEntityProvider() :> IStaticEntityProvider
        let logger = LogPublishActor()
        
        do ActorMessage.Info "Starting EveAlod..." |> logger.Post
        
        let configProvider = ConfigProvider(logger.Post, dataProvider)

        let config = configProvider.Configuration() 
                                    |> Config.validate
                                    |> Config.report logger.Post
        

        let mainChannel = { DiscordChannel.Id = config.ChannelId; Token = config.ChannelToken}

        let dataActor = StaticDataActor(logger.Post, dataProvider)

        let discordPublisher = DiscordPublishActor(logger.Post, mainChannel, EveAlod.Common.Web.sendDiscord)
        
        let killPublisher = KillPublisherActor(logger.Post, 
                                                    KillMessageBuilder(dataActor, config.CorpId), 
                                                    [ discordPublisher.Post; ] |> Actors.forwardMany (SendToDiscord))
        
        let killFilter = KillFilterActor(logger.Post, 
                                                config.MinimumScore, 
                                                [ killPublisher.Post; ] |> Actors.forwardMany (Kill))

        let killScorer = KillScorerActor(logger.Post, 
                                                [ logger.Post; killFilter.Post ] |> Actors.forwardMany (Kill)) 
        
        let killTagger = KillTaggerActor(logger.Post, 
                                                KillTagger(dataActor, config.CorpId), 
                                                [ killScorer.Post ] |> Actors.forwardMany (Kill))
               
        let killTransform = KillTransformActor(logger.Post, 
                                                [ killTagger.Post ] |> Actors.forwardMany (Kill))
       
        let killSource = KillSourceActor(logger.Post,
                                                [ killTransform.Post ] |> Actors.forwardMany (ActorMessage.KillJson) ,
                                                EveAlod.Common.Web.getData,
                                                config.KillSourceUri)
                                            
        member this.KillSource = killSource

        member this.Log = logger.Post
