namespace EveAlod.Common.Tests

    open FsCheck.Xunit
    open EveAlod.Common.Combinators

    module CombinatorsTests=
        
        [<Property(Verbose = true)>]
        let DoublePipeCombinatorIsDisjunctive left right =
            let l = (fun () -> left)
            let r = (fun () -> right)

            let f = l <||> r

            f() = (l() || r())

