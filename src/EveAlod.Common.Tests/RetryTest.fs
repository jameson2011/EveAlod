namespace EveAlod.Common.Tests

open Xunit

module RetryTest=
    
    [<Fact>]
    let ``Retry.retryIterAsync exceeds iterations``()=
        let its = 3
        let mutable count = 0
        let func() = async {    count <- count + 1
                                return false }
        let finish x = x = true

        let r = EveAlod.Common.Retry.retryIterAsync its finish func |> Async.RunSynchronously

        Assert.False r
        Assert.Equal(its + 1, count)

    [<Fact>]
    let ``Retry.retryIterAsync zero iterations``()=
        let its = 0
        let mutable count = 0
        let func() = async {    count <- count + 1
                                return false }
        let finish x = x = true

        let r = EveAlod.Common.Retry.retryIterAsync its finish func |> Async.RunSynchronously

        Assert.False r
        Assert.Equal(its + 1, count)

    [<Fact>]
    let ``Retry.retryIterAsync finishes early``()=
        let its = 4
        let mutable count = 0
        let func() = async {    count <- count + 1
                                return false }
        let finish x = count >= 2

        let r = EveAlod.Common.Retry.retryIterAsync its finish func |> Async.RunSynchronously

        Assert.False r
        Assert.Equal(2, count)

    //
    [<Fact>]
    let ``Retry.retryWaitIterAsync exceeds iterations``()=
        let its = 3
        let mutable count = 0
        let delay x = 100 * x
        let func() = async {    count <- count + 1
                                return false }
        let finish x = x = true

        let r = EveAlod.Common.Retry.retryWaitIterAsync its delay finish func |> Async.RunSynchronously

        Assert.False r
        Assert.Equal(its + 1, count)

    [<Fact>]
    let ``Retry.retryWaitIterAsync zero iterations``()=
        let its = 0
        let mutable count = 0
        let delay x = 100 * x
        let func() = async {    count <- count + 1
                                return false }
        let finish x = x = true

        let r = EveAlod.Common.Retry.retryWaitIterAsync its delay finish func |> Async.RunSynchronously

        Assert.False r
        Assert.Equal(its + 1, count)

    [<Fact>]
    let ``Retry.retryWaitIterAsync finishes early``()=
        let its = 4
        let mutable count = 0
        let delay x = 100 * x
        let func() = async {    count <- count + 1
                                return false }
        let finish x = count >= 2

        let r = EveAlod.Common.Retry.retryWaitIterAsync its delay finish func |> Async.RunSynchronously

        Assert.False r
        Assert.Equal(2, count)
