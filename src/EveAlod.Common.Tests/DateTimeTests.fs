namespace EveAlod.Common.Tests

    open System
    open Xunit
    open FsCheck
    open FsCheck.Xunit
    open EveAlod.Common.DateTime    
    
    module DateTimeTests =
        open EveAlod.Common
    
        let baseDate = epochStart

        [<Property(Verbose = true)>]
        let EpochIsBasedFrom1970(secs)=
            let r = getUtcFromEpoch secs
            if secs >= 0 then
                r.Year >= epochStart.Year
            else    
                r.Year < epochStart.Year
                
        [<Fact>]
        let ``epochStart is first day of 1970``() =
            let r = getUtcFromEpoch 0
            Assert.Equal(epochStart, r)            

        [<Property(Verbose = true)>]
        let ``getUtcFromEpoch Parameter is treated as seconds``(secs) =
            let diff = (getUtcFromEpoch secs) - System.TimeSpan.FromSeconds(float secs)

            baseDate = diff

        [<Property(Verbose = true)>]
        let ``remoteTimeOffset when remote is mid way offset is always zero``(localSpread: float)=
            let now = DateTime.UtcNow
            let seconds = int localSpread |> float |> abs
            let startTime = now.AddSeconds(-seconds)
            let endTime = now.AddSeconds(seconds)

            let remote = now

            let result = DateTime.remoteTimeOffset startTime endTime remote

            result = TimeSpan.Zero

        [<Property(Verbose = true)>]
        let ``remoteTimeOffset the given non-zero remote offset is returned as the result``(localSpread: int, remoteOffset: int)=
            let now = DateTime.UtcNow
            let seconds = float localSpread |> abs

            let startTime = now.AddSeconds(-seconds)
            let endTime = now.AddSeconds(seconds)

            let remote = now.AddSeconds(float remoteOffset)

            let result = DateTime.remoteTimeOffset startTime endTime remote

            result = TimeSpan.FromSeconds(float remoteOffset)
    
        [<Fact>]
        let ``ofDateTimeOffset null value yields default``()=
            let def = DateTime.UtcNow
            let value = System.Nullable<DateTimeOffset>()

            let result = DateTime.ofDateTimeOffset def value

            result = def

        [<Fact>]
        let ``ofDateTimeOffset non-null value yields value``()=
            let now = DateTime.UtcNow
            let def = now
            let dto = DateTimeOffset.UtcNow.AddDays(-1.)
            let value = System.Nullable<DateTimeOffset>(dto)

            let result = DateTime.ofDateTimeOffset def value

            result = dto.UtcDateTime

        [<Property(Verbose=true)>]
        let ``diff is the same as DateTime.op_subtract``(x: DateTime, y: DateTime)=
            (diff x y) = (x - y)
            
        [<Property(Verbose=true, Arbitrary = [| typeof<PositiveFloats> |])>]
        let ``addTimeSpan is the same as DateTime.Add``(x: DateTime, secs: float)=
            let y = TimeSpan.FromSeconds(secs)
            (addTimeSpan y x) = (x.Add(y))
