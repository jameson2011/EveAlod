﻿namespace EveAlod.Common

    open System

    module DateTime=
        
        let epochStart = DateTime(1970, 1, 1)

        let getUtcFromEpoch (seconds) = epochStart.AddSeconds(float seconds)

        let addTimeSpan (y: TimeSpan) (x: DateTime) = 
            x.Add(y)

        let diff (x: DateTime) (y: DateTime) = 
            x - y

        let date (x: DateTime) = 
            x.Date

        let ofDateTimeOffset (defaultTime: DateTime) (time: Nullable<DateTimeOffset>)=            
            if time.HasValue then
                time.Value.UtcDateTime
            else
                defaultTime
            
        let remoteTimeOffset (localUtcStart: DateTime) (localUtcEnd: DateTime) (remoteUtc: DateTime) =
            let localDuration = (localUtcEnd - localUtcStart).TotalMilliseconds / 2.
            let localUtc = localUtcStart.AddMilliseconds(localDuration)

            diff remoteUtc localUtc
            
        
