namespace EveAlod.Services

    open System
    open EveAlod.Data

    type ServiceFactory()=
    
        
        let configProvider = new ConfigProvider()
        let config = configProvider.Configuration()
        let mainChannel = { DiscordChannel.Id = config.ChannelId; Token = config.ChannelToken}

        let staticData = new StaticEntityProvider() :> IStaticEntityProvider

        let logger = new LogPublishActor()
        let sendLog = logger.Post
        
        let tagger = new KillTagger(staticData, config.CorpId)
        let killMessageBuilder = new KillMessageBuilder(staticData)
        
        
        let discordPublisher = new DiscordPublishActor(sendLog, mainChannel, TimeSpan.FromSeconds(5.))
        
        let killPublisher = new KillPublisherActor(killMessageBuilder, 
                                                    fun s -> discordPublisher.Post (SendToDiscord s))
        
        let killFilter = new KillFilterActor(config.MinimumScore, 
                                                (fun km ->  killPublisher.Post (Publish km) ))

        let killScorer = new KillScorerActor(fun km ->  sendLog (Log km)
                                                        killFilter.Post (Scored km))
        
        let killTagger = new KillTaggerActor(tagger, 
                                                config.CorpId, 
                                                fun km -> killScorer.Post (Score km))

        let killSource = new KillSourceActor((fun km -> killTagger.Post (Tag km)), 
                                                sendLog,
                                                EveAlod.Common.Web.getData, "https://redisq.zkillboard.com/listen.php?ttw=10")
                                            
        member this.KillSource = killSource

        member this.Log = sendLog
