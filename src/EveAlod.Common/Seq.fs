namespace EveAlod.Common
    module Seq=

        let mapSomes (values: seq<'a option>)= 
            values
            |> Seq.filter (fun v -> v.IsSome)
            |> Seq.map (fun v -> v.Value)
            
        // TODO:
        let splitBy (f: 'a -> bool) (values: 'a list) =            
            let rec splitLists (values) (left: 'a list) (right: 'a list)=
                match values with
                | [] -> left |> List.rev, right |> List.rev
                | h::t ->   if f h then splitLists t (h :: left) right
                            else splitLists t left (h :: right)
            splitLists values [] [] 