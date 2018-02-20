﻿namespace EveAlod.ValuationService

open Suave
open Suave.Operators
open EveAlod.Common.Strings

module WebServices=
    let jsonMimeType = Writers.setMimeType "application/json; charset=utf-8"

    let setExpiry (age: int ) = age |> toString |> Writers.setHeader "Expires" 
    let setPragmaNoCache = Writers.setHeader "Pragma" "no-cache"
    let setNoCacheControl = Writers.setHeader "Cache-Control" "no-cache, no-store, must-revalidate"

    let setCacheControl (age: int) = 
        age * 60 |> toString |> sprintf "public, max-age=%s" |> Writers.setHeader "Cache-Control" 

    let setNoCache = setNoCacheControl >=> setPragmaNoCache >=> setExpiry 0
    let setCache age = setCacheControl age

    let getShipTypeStats (id: string) (ctx: HttpContext)=
        async {
                                
            let response = sprintf """{ "stats": "%s" }""" id
                
            return! Successful.OK response ctx
        }