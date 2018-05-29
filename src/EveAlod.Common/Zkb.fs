namespace EveAlod.Common

module Zkb=
    
    [<Literal>]
    let private ZkbDomain = "https://zkillboard.com"

    let redisqUri = 
        "https://redisq.zkillboard.com/listen.php?ttw=10"

    let killUri id =
        sprintf "%s/kill/%s/" ZkbDomain id

    let regionKillsUri id = 
        sprintf "%s/region/%i/" ZkbDomain id

    let regionStatsUri id = 
        sprintf "%s/region/%i/stats/" ZkbDomain id

    let abyssalKillsUrl() = 
        sprintf "%s/kills/abyssal/" ZkbDomain

    let solarSystemKillsUrl id =
        sprintf "%s/system/%i/" ZkbDomain id

    let solarSystemStatsUrl id =
        sprintf "%s/system/%i/stats/" ZkbDomain id

    let locationKillsUri id =
        sprintf "%s/location/%i/" ZkbDomain id

    let locationStatsUri id =
        sprintf "%s/location/%i/stats/" ZkbDomain id

    let shipTypeKillsUri id = 
        sprintf "%s/ship/%s/" ZkbDomain id

    let shipTypeStatsUri id = 
        sprintf "%s/ship/%s/stats/" ZkbDomain id    

    let characterKillboardUri id =
        sprintf "%s/character/%s/" ZkbDomain id

    let characterKillboardStatsUri id =
        sprintf "%s/character/%s/stats/" ZkbDomain id
