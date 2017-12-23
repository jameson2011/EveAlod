namespace EveAlod.Services

    open EveAlod.Entities

    type ActorFactory()=
                
        let mainChannel = { DiscordChannel.Id = ""; Token = ""}
        
        let discordPublisher = new DiscordPublishActor(mainChannel)
        let logger = new LogPublishActor()
        
        // TODOTK: fan out?
        let killFilter = new KillFilterActor(50., fun km -> logger.Post (Log km))

        let killScorer = new KillScorerActor(fun km -> killFilter.Post (Scored km))
        
        let killTagger = new KillTaggerActor(fun km -> killScorer.Post (Score km))

        let killSource = new KillSourceActor((fun km -> killTagger.Post (Tag km)), 
                                            EveAlod.Data.Web.getKm, "https://redisq.zkillboard.com/listen.php?ttw=10")

        
        member this.KillSource = killSource
