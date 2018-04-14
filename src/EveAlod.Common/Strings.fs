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

        let toInt (value: string) =
            value
            |> Int32.TryParse
            |> (fun (ok,value) -> if ok then Some value else None)
            

        let toIntValue (value: string option) =
            value
            |> Option.map (fun s -> Int32.TryParse(s))
            |> Option.map (fun (ok,value) -> if ok then value else 0)
            |> Option.defaultValue 0                
                
        let isNullWhitespace str =
            String.IsNullOrWhiteSpace(str)
            
        let (|NullOrWhitespace|_|) str=
            if isNullWhitespace str then Some str else None

        let (|Suffix|_|) (suffix: string) (str: string) =
            if str.LastIndexOf(suffix) >= 0 then Some str else None

        let (|Split|_|) (prefix: string) (suffix: string) (str: string) =
            if str.StartsWith(prefix) && str.LastIndexOf(suffix) >= 0 then Some str else None

        let join (delimiter: string) (values: seq<string>) = 
            System.String.Join(delimiter, values)

        let split (delim: string) (value: string) = 
            value.Split(delim.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)

        let toString (value: 'a) = value.ToString()
        
        let expandFloat value =                     
            let xs = [ (1000000000.0, " billion"); (1000000.0, " million"); (1000.0, " thousand"); (1.0, "") ]
            xs |> Seq.filter (fun (d,_) -> value / d >= 1.0)
            |> Seq.map (fun (d,s) -> sprintf "%s%s" ((value / d).ToString("N2")) s)
            |> Seq.tryHead
