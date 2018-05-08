namespace EveAlod.Common.Tests

open System
open FsCheck.Xunit
open EveAlod.Common
open EveAlod.Common.Strings

module StringsTests=
        
    [<Property(Verbose = true, Arbitrary = [| typeof<NonEmptyStrings> |])>]
    let ``prettyConcat contains all terms``(xs: string list)=
        let result = prettyConcat xs

        let idxs = xs   |> Seq.map result.IndexOf
                        |> List.ofSeq
        idxs.Length = xs.Length
        
    [<Property(Verbose = true, Arbitrary = [| typeof<UniqueNonEmptyStrings> |])>]
    let ``prettyConcat contains terms in sequence``(x: string)=
        let xs = x |> Arb.toWords 3
        let result = prettyConcat xs

        let idxs = xs |> List.map result.IndexOf

        idxs |> (Seq.exists (fun i -> i < 0) >> not)
        
    [<Property(Verbose = true, Arbitrary = [| typeof<UniqueNonEmptyStrings> |])>]
    let ``prettyConcat contains correct comma ampersand counts``(xs: string list)=

        let l = List.length xs

        let commas,amps =   match l with
                            | x when x <= 1 -> 0,0
                            | x when x <= 2 -> 0,1
                            | _ -> l-2,1
            
        let f = ((Arb.charCount ',') >> (=) commas) <&&> ((Arb.charCount '&') >>(=) amps)

        prettyConcat xs |> f 


