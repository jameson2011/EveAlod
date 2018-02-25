namespace EveAlod.Valuation.Backfill

type ServiceFactory(config: BackfillConfiguration)=
    
    let logger = EveAlod.Services.LogPublishActor()

    let crawler = new HistoryCrawlActor(logger.Post, config)

    member __.Log = logger.Post

    member __.Crawler = crawler
