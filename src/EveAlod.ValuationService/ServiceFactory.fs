namespace EveAlod.ValuationService

open EveAlod.Services
open EveAlod.Valuation

type ValuationServiceFactory(config: ValuationConfiguration)=

    let logger = LogPublishActor("valuation.log4net.config")

    let shipStatsActor = ShipStatsActor(config, logger.Post)
    let sourceForward msg = msg |> ValuationActorMessage.ImportKillJson |> shipStatsActor.Post


    let importActor = KillSourceActor(logger.Post, sourceForward, EveAlod.Common.Web.getData, config.KillSourceUri)
    
    member __.Log = logger.Post

    member __.Source = importActor

    member __.ShipStats = shipStatsActor


