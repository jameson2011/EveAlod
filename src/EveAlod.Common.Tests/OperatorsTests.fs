namespace EveAlod.Common.Tests

open FsCheck.Xunit
open EveAlod.Common.Operators

module OperatorsTests=
        
    [<Property(Verbose = true)>]
    let DoublePipeCombinatorIsDisjunctive left right =            
        let f = (fun () -> left) <||> (fun () -> right)

        f() = (left || right)


    [<Property(Verbose = true)>]
    let DoubleAmpersandCombinatorIsConjunctive left right =
            
        let f = (fun () -> left) <&&> (fun () -> right)

        f() = (left && right)


