﻿namespace EveAlod.Entities
    
    open System
    open FSharp.Data

    [<AutoOpen>]
    module Json=
        let getProp (name : string) (json: JsonValue option) = 
            json |> Option.bind (fun j -> j.TryGetProperty(name))
        
        let getBool (json: JsonValue option)=
            defaultArg (json |> Option.map (fun j -> j.AsBoolean())) false

        let getFloat (json: JsonValue option)=
            defaultArg (json |> Option.map (fun j -> j.AsFloat())) 0.

        let getString (json: JsonValue option)=
            defaultArg (json |> Option.map (fun j -> j.AsString())) ""

        let getDateTime (json: JsonValue option)=
            defaultArg (json |> Option.map (fun j -> j.AsDateTime())) DateTime.MinValue

