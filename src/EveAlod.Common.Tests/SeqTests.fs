namespace EveAlod.Common.Tests

    open System
    open Xunit
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
            
        [<Property(Verbose=true, Arbitrary = [| typeof<PositiveInts> |] )>]
        let ``tryTake`` size count =
            let xs = [ 1 .. size ]
            let result = xs |> Seq.tryTake count

            let check = match size >= count with
                        | true -> Some (xs |> List.truncate count)
                        |_ -> None

            check = result

        [<Fact>]
        let ``conjunctMatch xs and ys are empty``() =
            let xs = [  ]
            let ys = [  ]
            
            let r = xs |> Seq.conjunctMatch ys

            Assert.True r

        [<Fact>]
        let ``conjunctMatch ys is empty``() =
            let xs = [ 1; 2; 3; ]
            let ys = [  ]
            
            let r = xs |> Seq.conjunctMatch ys

            Assert.False r

        [<Fact>]
        let ``conjunctMatch xs is empty``() =
            let ys = [ 1; 2; 3; ]
            let xs = [  ]
            
            let r = xs |> Seq.conjunctMatch ys

            Assert.True r

        [<Fact>]
        let ``conjunctMatch xs ys are equal``() =
            let xs = [ 1;  ]
            let ys = [ 1;  ]
            
            let r = xs |> Seq.conjunctMatch ys

            Assert.True r

        [<Fact>]
        let ``conjunctMatch ys contains xs ``() =
            let ys = [ 1; 2; 3; ]
            let xs = [ 2; 3; ]
            
            let r = xs |> Seq.conjunctMatch ys

            Assert.True r

        [<Fact>]
        let ``conjunctMatch ys overlaps xs ``() =
            let ys = [ 1; 2; 3; ]
            let xs = [ 2; 3; 4; ]
            
            let r = xs |> Seq.conjunctMatch ys

            Assert.False r