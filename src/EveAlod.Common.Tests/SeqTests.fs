namespace EveAlod.Common.Tests

    open System
    open FsCheck.Xunit
    open EveAlod.Common
    open EveAlod.Common.Seq

    module SeqTests=

        [<Property(Verbose=true)>]
        let ``splitBy``(xs: int list)=
            let splitter = (fun x -> x % 2 = 0)

            let l,r = Seq.splitBy splitter xs
            
            
            l = (xs |> List.filter splitter) &&
            r = (xs |> List.filter (not << splitter) )
            