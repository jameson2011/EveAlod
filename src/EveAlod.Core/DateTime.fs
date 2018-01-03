﻿namespace EveAlod.Core
    module DateTime=

        open System

        let private epochStart = new DateTime(1970, 1, 1)

        let getUtcFromEpoch (seconds) = epochStart.AddSeconds(float seconds)

        let machineTimeOffset (localUtcTime: DateTime) (serverUtcTime: Nullable<DateTimeOffset>) =
            let serverTime =    if serverUtcTime.HasValue then
                                    serverUtcTime.Value.UtcDateTime
                                else
                                    localUtcTime
            
            localUtcTime - serverTime
            

