namespace EveAlod.Services

    open System
    open EveAlod.Data

    type ServiceFactory(configProvider: unit -> Configuration)=
    
        let config = configProvider()
        let mainChannel = { DiscordChannel.Id = config.ChannelId; Token = config.ChannelToken}

        let dataProvider = StaticEntityProvider() :> IStaticEntityProvider
        let logger = LogPublishActor()
        let dataActor = StaticDataActor(logger.Post, dataProvider)
        
        let discordPublisher = DiscordPublishActor(logger.Post, mainChannel, EveAlod.Common.Web.sendDiscord)
        
        let killPublisher = KillPublisherActor(logger.Post, 
                                                    KillMessageBuilder(dataActor, config.CorpId), 
                                                    Actors.forward SendToDiscord discordPublisher.Post
                                                    )
        
        let killFilter = KillFilterActor(logger.Post, 
                                                config.MinimumScore, 
                                                Actors.forward Publish killPublisher.Post)

        let killScorer = KillScorerActor(logger.Post, 
                                                fun km ->   logger.Post (Log km)
                                                            killFilter.Post (Scored km)) 
        
        let killTagger = KillTaggerActor(logger.Post, 
                                                KillTagger(dataActor, config.CorpId), 
                                                Actors.forward Score killScorer.Post)

        let killSource = KillSourceActor(logger.Post,
                                                Actors.forward Tag killTagger.Post,
                                                EveAlod.Common.Web.getData,
                                                "https://redisq.zkillboard.com/listen.php?ttw=10")
                                            
        member this.KillSource = killSource

        member this.Log = logger.Post
