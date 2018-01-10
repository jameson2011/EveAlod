namespace EveAlod.Common
    module Seq=

        let exceptNones (values: seq<'a option>)= 
            values
            |> Seq.filter (fun v -> v.IsSome)
            |> Seq.map (fun v -> v.Value)
            |> List.ofSeq

