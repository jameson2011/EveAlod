namespace EveAlod.Common
    module Http =

        open System
        open System.Net
        
        let getHeaderValue name (response: Http.HttpResponseMessage) = 
            response.Headers
                |> Seq.filter (fun h -> h.Key = name)
                |> Seq.collect (fun h -> h.Value)
                |> Seq.tryHead
        
        let getIntValue (value: string option) =
            value
            |> Option.map (fun s -> Int32.TryParse(s))
            |> Option.map (fun (ok,value) -> if ok then value else 0)
            |> Option.defaultValue 0                
