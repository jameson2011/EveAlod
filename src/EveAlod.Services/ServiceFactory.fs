namespace EveAlod.Services

    open System
    open EveAlod.Data

    type ServiceFactory()=
    
        let configProvider = new ConfigProvider()
        let staticData = new StaticEntityProvider()

        let config = configProvider.Configuration()
        let tagger = new KillTagger(staticData :> IStaticEntityProvider, config.CorpId)
        let mainChannel = { DiscordChannel.Id = config.ChannelId; Token = config.ChannelToken}
        
        let discordPublisher = new DiscordPublishActor(mainChannel, TimeSpan.FromSeconds(5.))
        let logger = new LogPublishActor()
        
        let killFilter = new KillFilterActor(config.MinimumScore, 
                                                (fun km ->  discordPublisher.Post (SendToDiscord km) ))

        let killScorer = new KillScorerActor(fun km ->  logger.Post (Log km)
                                                        killFilter.Post (Scored km))
        
        let killTagger = new KillTaggerActor(tagger, 
                                                config.CorpId, 
                                                fun km -> killScorer.Post (Score km))

        let killSource = new KillSourceActor((fun km -> killTagger.Post (Tag km)), 
                                            EveAlod.Common.Web.getData, "https://redisq.zkillboard.com/listen.php?ttw=10")
                                            
        member this.KillSource = killSource
