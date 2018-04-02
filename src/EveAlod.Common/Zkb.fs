﻿namespace EveAlod.Common

module Zkb=
    
    [<Literal>]
    let private zkbDomain = "https://zkillboard.com"

    let regionKillsUri id = 
        sprintf "%s/region/%i/" zkbDomain id

    let solarSystemKillsUrl id =
        sprintf "%s/system/%i/" zkbDomain id

    let locationKillsUri id =
        sprintf "%s/location/%i/" zkbDomain id

    let shipTypeKillsUri id = 
        sprintf "%s/ship/%s/" zkbDomain id

    let characterKillboardUri id =
        sprintf "%s/character/%s/" zkbDomain id
