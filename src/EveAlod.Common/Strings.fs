namespace EveAlod.Common
    
    open System
        
    module Strings=                      
        let str x = x.ToString()

        let prettyConcat (txts: string list) =
            let rec concat txts (result: string) =
                match txts with                
                | [] -> result
                | [h] ->  if result.Length > 0 then
                                result + " & " + h
                            else 
                                h
                | h::t ->   if result.Length > 0 then
                                concat t (result + ", " + h)
                            else    
                                concat t h
                                    
            concat txts ""

        let toIntValue (value: string option) =
            value
            |> Option.map (fun s -> Int32.TryParse(s))
            |> Option.map (fun (ok,value) -> if ok then value else 0)
            |> Option.defaultValue 0                
                
        let isNullWhitespace str =
            String.IsNullOrWhiteSpace(str)
            
        let (|NullOrWhitespace|_|) str=
            if isNullWhitespace str then Some str else None

        let join (delimiter: string) (values: seq<string>) = 
            System.String.Join(delimiter, values)

        let split (delim: string) (value: string) = 
            value.Split(delim.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
