namespace EveAlod.Services

    open EveAlod.Entities

    type ServiceFactory()=
    
        let configProvider = new ConfigProvider()
        let staticData = new StaticEntityProvider()
        let tagger = new KillTagger(staticData :> IStaticEntityProvider)
        let config = configProvider.Configuration
        let mainChannel = { DiscordChannel.Id = config.ChannelId; Token = config.ChannelToken}
        
        let discordPublisher = new DiscordPublishActor(mainChannel)
        let logger = new LogPublishActor()
        
        let killFilter = new KillFilterActor(config.MinimumScore, 
                                                (fun km ->  discordPublisher.Post (SendToDiscord km) ))

        let killScorer = new KillScorerActor(fun km ->  logger.Post (Log km)
                                                        killFilter.Post (Scored km))
        
        let killTagger = new KillTaggerActor(tagger, 
                                                config.CorpId, 
                                                fun km -> killScorer.Post (Score km))

        let killSource = new KillSourceActor((fun km -> killTagger.Post (Tag km)), 
                                            EveAlod.Data.Web.getData, "https://redisq.zkillboard.com/listen.php?ttw=10")

        
        member this.KillSource = killSource
