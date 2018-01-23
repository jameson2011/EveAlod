namespace EveAlod.Services

    open System
    open EveAlod.Data

    type ServiceFactory(configProvider: unit -> Configuration)=
    
        let config = configProvider()
        let mainChannel = { DiscordChannel.Id = config.ChannelId; Token = config.ChannelToken}

        let staticData = new StaticEntityProvider() :> IStaticEntityProvider

        let logger = new LogPublishActor()
        
        let discordPublisher = new DiscordPublishActor(logger.Post, mainChannel, EveAlod.Common.Web.sendDiscord)
        
        let killPublisher = new KillPublisherActor(logger.Post, 
                                                    new KillMessageBuilder(staticData, config.CorpId), 
                                                    Actors.forward SendToDiscord discordPublisher.Post
                                                    )
        
        let killFilter = new KillFilterActor(logger.Post, 
                                                config.MinimumScore, 
                                                Actors.forward Publish killPublisher.Post)

        let killScorer = new KillScorerActor(logger.Post, 
                                                fun km ->   logger.Post (Log km)
                                                            killFilter.Post (Scored km)) 
        
        let killTagger = new KillTaggerActor(logger.Post, 
                                                new KillTagger(staticData, config.CorpId), 
                                                Actors.forward Score killScorer.Post)

        let killSource = new KillSourceActor(logger.Post,
                                                Actors.forward Tag killTagger.Post,
                                                EveAlod.Common.Web.getData,
                                                "https://redisq.zkillboard.com/listen.php?ttw=10")
                                            
        member this.KillSource = killSource

        member this.Log = logger.Post
