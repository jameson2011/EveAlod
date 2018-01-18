namespace EveAlod.Common
    module Seq=

        let mapSomes (values: seq<'a option>)= 
            values
            |> Seq.filter (fun v -> v.IsSome)
            |> Seq.map (fun v -> v.Value)
            

