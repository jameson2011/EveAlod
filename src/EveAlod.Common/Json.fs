namespace EveAlod.Common
    
    open System
    open FSharp.Data

    module Json=
        let getProp (name : string) (json: JsonValue) = 
            FSharp.Data.JsonExtensions.TryGetProperty(json, name)
            
        let getPropOption (name : string) (json: JsonValue option) = 
            json |> Option.bind (getProp name)
        
        let getPropStr (name : string) =            
            (getPropOption name) >> Option.map FSharp.Data.JsonExtensions.AsString >> Option.defArg ""

        let getPropInt (name : string) =            
            (getPropOption name) >> Option.map FSharp.Data.JsonExtensions.AsInteger >> Option.defArg 0

        let getPropBool (name : string) =            
            (getPropOption name) >> Option.map FSharp.Data.JsonExtensions.AsBoolean >> Option.defArg false

        let getPropFloat (name : string) =            
            (getPropOption name) >> Option.map FSharp.Data.JsonExtensions.AsFloat >> Option.defArg 0.

        let getPropDateTime (name : string) =            
            (getPropOption name) >> Option.map FSharp.Data.JsonExtensions.AsDateTime >> Option.defArg DateTime.MinValue


