namespace EveAlod.ValuationService

open EveAlod.Services
open EveAlod.Valuation

type ServiceFactory(config: ValuationConfiguration)=

    let logger = LogPublishActor()

    let shipStatsActor = ShipStatsActor(config, logger.Post)
    let sourceForward msg = msg |> ValuationActorMessage.ImportKillJson |> shipStatsActor.Post


    let importActor = KillSourceActor(logger.Post, sourceForward, EveAlod.Common.Web.getData, config.KillSourceUri)
    
    member __.Log = logger.Post

    member __.Source = importActor

