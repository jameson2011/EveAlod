namespace EveAlod.Valuation

open System
open FSharp.Data

type ResponsePayload =
    | Json of JsonValue

module Response =
    let toString (json: ResponsePayload) = 
        match json with
        | Json j -> j.ToString()


