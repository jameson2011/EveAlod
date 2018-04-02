namespace EveAlod.Common
    module Seq=

        let mapSomes (values: seq<'a option>)= 
            values
            |> Seq.filter Option.isSome
            |> Seq.map Option.get
                    
        let splitTuples values =
            let rec splitInner values left right =
                match values with
                | [] -> left|> List.rev, right |> List.rev
                | (l,r)::t -> splitInner t (l::left) (r:: right)
            splitInner values [] [] 
            

        let splitBy (f: 'a -> bool) (values: 'a list) =            
            let rec splitLists (values) (left: 'a list) (right: 'a list)=
                match values with
                | [] -> left |> List.rev, right |> List.rev
                | h::t ->   if f h then splitLists t (h :: left) right
                            else splitLists t left (h :: right)
            splitLists values [] [] 

        let tryTake count values =
            let values = values |> Seq.truncate count
                                |> List.ofSeq
            if List.length values = count then Some values
            else None


        let conjunctMatch (comparands: seq<'a>) (values: seq<'a>) = 
            
            let valuesCount = values |> Seq.length
            let hits = comparands 
                        |> Seq.allPairs values
                        |> Seq.filter (fun (x,y) -> x = y)
                        |> tryTake valuesCount
               
            Option.isSome hits
            
            
    module Set=
        let rec append (xs: 'a list) (acc: Set<'a>) =
            match xs with
            | [] -> acc
            | h::t -> acc.Add(h) |> append t 
