﻿namespace EveAlod.Common.Tests

    open System
    open FsCheck.Xunit
    open FSharp.Data
    open EveAlod.Common
    open EveAlod.Common.Json

    module JsonTests=

        let toJsonValue name value = 
            [| (name, JsonValue.String(value)) |]            
            |> JsonValue.Record

        [<Property(Verbose = true, Arbitrary = [| typeof<NonEmptyStrings> |])>]
        let ``prop value located by name``(name: string) (value: string)=
            let json =  toJsonValue name value|> Some                            

            let propValue = json |> prop name
            
            match propValue with
            | Some pv -> pv.AsString() = value
            | _ -> false

        [<Property(Verbose = true, Arbitrary = [| typeof<NonEmptyStrings> |])>]
        let ``prop options``(name: string) isSome (value: string)=
            let json =  match isSome with
                        | true -> toJsonValue name value |> Some                            
                        | _ -> None

            let propValue = json |> prop name
            
            match propValue with
            | Some pv -> pv.AsString() = value
            | _ -> not isSome

        let ``propStr`` name value =
            let json = toJsonValue name value

            let result = Some json |> (propStr name)

            value = result
