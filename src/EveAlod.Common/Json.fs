namespace EveAlod.Common
    
    open System
    open FSharp.Data
    

    module Json=
    
        let asArray (json: JsonValue) = json.AsArray()

        let propOption (name : string) (json: JsonValue) = 
            FSharp.Data.JsonExtensions.TryGetProperty(json, name)
            
        let prop (name : string) (json: JsonValue option) = 
            json |> Option.bind (propOption name)
        
        let propStr (name : string) =            
            (prop name) >> Option.map FSharp.Data.JsonExtensions.AsString >> Option.defArg ""

        let propInt (name : string) =            
            (prop name) >> Option.map FSharp.Data.JsonExtensions.AsInteger >> Option.defArg 0

        let propBool (name : string) =            
            (prop name) >> Option.map FSharp.Data.JsonExtensions.AsBoolean >> Option.defArg false

        let propFloat (name : string) =            
            (prop name) >> Option.map FSharp.Data.JsonExtensions.AsFloat >> Option.defArg 0.

        let propDateTime (name : string) =            
            (prop name) >> Option.map FSharp.Data.JsonExtensions.AsDateTime >> Option.defArg DateTime.MinValue


