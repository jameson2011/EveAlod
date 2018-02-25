namespace EveAlod.Valuation.Backfill

type ServiceFactory(config: BackfillConfiguration)=
    
    let logger = EveAlod.Services.LogPublishActor()

    let crawler = HistoryCrawlActor(logger.Post, config)

    member __.Log = logger.Post

    member __.Crawler = crawler
