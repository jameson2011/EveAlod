namespace EveAlod.ValuationService

open EveAlod.Services

type ServiceFactory()=

    let logger = LogPublishActor()
    
    member __.Log = logger.Post

