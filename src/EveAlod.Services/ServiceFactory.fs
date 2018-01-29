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
                                                    [ discordPublisher.Post; ] |> Actors.forwardMany (SendToDiscord)
                                                    )
        
        let killFilter = KillFilterActor(logger.Post, 
                                                config.MinimumScore, 
                                                [ killPublisher.Post; ] |> Actors.forwardMany (Kill))

        let killScorer = KillScorerActor(logger.Post, 
                                                [ logger.Post; killFilter.Post ] |> Actors.forwardMany (Kill)
                                                ) 
        
        let killTagger = KillTaggerActor(logger.Post, 
                                                KillTagger(dataActor, config.CorpId), 
                                                //Actors.forward Kill killScorer.Post
                                                [ killScorer.Post ] |> Actors.forwardMany (Kill)
                                                )
               

        let killSource = KillSourceActor(logger.Post,
                                                [ dumpActor.Post; killTagger.Post ] |> Actors.forwardMany (Kill),
                                                EveAlod.Common.Web.getData,
                                                "https://redisq.zkillboard.com/listen.php?ttw=10")
                                            
        member this.KillSource = killSource

        member this.Log = logger.Post
