namespace EveAlod.Common
    module DateTime=

        open System

        let epochStart = new DateTime(1970, 1, 1)

        let getUtcFromEpoch (seconds) = epochStart.AddSeconds(float seconds)

        let ofDateTimeOffset (defaultTime: DateTime) (time: Nullable<DateTimeOffset>)=            
            if time.HasValue then
                time.Value.UtcDateTime
            else
                defaultTime

        let machineTimeOffset (localUtcTime: DateTime) (serverUtcTime: Nullable<DateTimeOffset>) =
            let serverTime =    if serverUtcTime.HasValue then
                                    serverUtcTime.Value.UtcDateTime
                                else
                                    localUtcTime
            
            localUtcTime - serverTime
            
        let remoteTimeOffset (localUtcStart: DateTime) (localUtcEnd: DateTime) (remoteUtc: DateTime) =
            let localDuration = (localUtcEnd - localUtcStart).TotalMilliseconds / 2.
            let localUtc = localUtcStart.AddMilliseconds(localDuration)

            remoteUtc - localUtc
            
