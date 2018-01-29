namespace EveAlod.Services

    open System
    open EveAlod.Data

    type ServiceFactory(configProvider: unit -> Configuration)=
    
        let config = configProvider()
        let mainChannel = { DiscordChannel.Id = config.ChannelId; Token = config.ChannelToken}

        let dataProvider = StaticEntityProvider() :> IStaticEntityProvider
        let logger = LogPublishActor()
        let dataActor = StaticDataActor(logger.Post, dataProvider)
        let dumpActor = KillDumpActor(logger.Post, config.DumpFolder)

        let discordPublisher = DiscordPublishActor(logger.Post, mainChannel, EveAlod.Common.Web.sendDiscord)
        
        let killPublisher = KillPublisherActor(logger.Post, 
                                                    KillMessageBuilder(dataActor, config.CorpId), 
                                                    Actors.forward SendToDiscord discordPublisher.Post
                                                    )
        
        let killFilter = KillFilterActor(logger.Post, 
                                                config.MinimumScore, 
                                                Actors.forward Kill killPublisher.Post)

        let killScorer = KillScorerActor(logger.Post, 
                                                fun km ->   logger.Post (Kill km)
                                                            killFilter.Post (Kill km)) 
        
        let killTagger = KillTaggerActor(logger.Post, 
                                                KillTagger(dataActor, config.CorpId), 
                                                Actors.forward Kill killScorer.Post)

        let forward km = Actors.forward Kill dumpActor.Post km
                         Actors.forward Kill killTagger.Post km 
                         

        let killSource = KillSourceActor(logger.Post,
                                                forward,
                                                EveAlod.Common.Web.getData,
                                                "https://redisq.zkillboard.com/listen.php?ttw=10")
                                            
        member this.KillSource = killSource

        member this.Log = logger.Post
