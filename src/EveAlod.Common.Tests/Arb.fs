namespace EveAlod.Common.Tests

    open System
    open FsCheck
    open FsCheck.Gen
    open FsCheck.Arb
    
    module Arb=
        let toWords count (s: string) =
            s |> Seq.collect (fun c -> [ new string(c, count); ]) |> List.ofSeq

        let charCount ch (str: string)  = 
                str.ToCharArray()
                |> Seq.filter (fun c -> c = ch)
                |> Seq.length

        let nullToEmpty = function
                            | null -> ""
                            | x -> x

        let toAllUniqueChars (s: string) =
            let set = s |> Set.ofSeq |> Array.ofSeq
            new string(set)
            
        let stripWhiteSpace (s: string)=
            let set = s |> Seq.filter Char.IsLetterOrDigit |> Array.ofSeq
            new string(set)
    
    type NonEmptyStrings=
        static member Words()=
            Arb.Default.String()
            |> Arb.filter (String.IsNullOrWhiteSpace >> not)            

    type DigitStrings=
        static member Words()=
            Arb.Default.String()
            |> Arb.filter ((=) null >> not)
            |> Arb.filter (fun s -> s |> Seq.exists (Char.IsDigit))

    type NonDigitStrings=
        static member Words()=
            Arb.Default.String()
            |> Arb.filter ((=) null >> not)
            |> Arb.filter (fun s -> s |> Seq.exists (Char.IsDigit >> not))

    type UniqueNonEmptyStrings=        
        static member Words()=                
            Arb.Default.String().Generator
            |> Gen.map(Arb.nullToEmpty >> Arb.stripWhiteSpace >> Arb.toAllUniqueChars)            
            |> Arb.fromGen    
            |> Arb.filter (fun s -> String.IsNullOrWhiteSpace(s) |> not)

    type PositiveFloats=
        static member Numbers()=
            Arb.Default.Float()
            |> Arb.filter (fun x -> x >  0. && x < 10000.)

    type PositiveInts=
        static member Numbers()=
            Arb.Default.Int32()
            |> Arb.filter (fun x -> x > 0 && x < 10000)