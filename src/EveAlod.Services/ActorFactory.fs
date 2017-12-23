namespace EveAlod.Services

    open EveAlod.Entities

    type ActorFactory()=
                
        let mainChannel = { DiscordChannel.Id = ""; Token = ""}
        
        let discordPublisher = new DiscordPublishActor(mainChannel)
        let logger = new LogPublishActor()
        
        // TODOTK: fan out?
        let killScorer = new KillScorerActor(fun km -> logger.Post (Log km))
        
        let killTagger = new KillTaggerActor(fun km -> killScorer.Post (Score km))

        let killSource = new KillSourceActor((fun km -> killTagger.Post (Tag km)), 
                                            EveAlod.Data.Web.getKm, "https://redisq.zkillboard.com/listen.php?ttw=10")

        
        member this.KillSource = killSource
