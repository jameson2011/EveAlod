namespace EveAlod.Common.Tests

    open Xunit
    open FsCheck
    open FsCheck.Xunit
    open EveAlod.Common.DateTime    
    
    module DateTimeTests =
    
        let baseDate = epochStart

        [<Property(Verbose = true)>]
        let EpochIsBasedFrom1970(secs)=
            let r = getUtcFromEpoch secs
            if secs >= 0 then
                r.Year >= epochStart.Year
            else    
                r.Year < epochStart.Year
                
        [<Fact>]
        let EpochIsFirstDayOf1970() =
            let r = getUtcFromEpoch 0
            Assert.Equal(epochStart, r)            

        [<Property(Verbose = true)>]
        let ParameterIsTreatedAsSeconds(secs) =
            let diff = (getUtcFromEpoch secs) - System.TimeSpan.FromSeconds(float secs)

            Assert.Equal(baseDate, diff)