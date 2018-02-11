﻿namespace EveAlod.Common

    open System

    type HttpStatus =
        | OK
        | TooManyRequests
        | Unauthorized
        | Error
    
    type WebResponse=
        {
            Status: HttpStatus;
            Retry: TimeSpan option;
            Message: string
        } with
        static member Ok retry message =
                {   WebResponse.Status = HttpStatus.OK;
                    Retry = retry;
                    Message = message
                }
        static member Unauthorized retry = 
                {   Status = HttpStatus.Unauthorized;
                    Retry = retry;
                    Message = "";
                }
        static member TooManyRequests retry = 
                {   Status = HttpStatus.TooManyRequests;
                    Retry = retry;
                    Message = "";
                }
        static member Error retry error = 
                {   Status = HttpStatus.Error;
                    Retry = retry;
                    Message = (sprintf "Error %s getting data" (error.ToString()) );
                }
        
    module Web=

        open System.Net
        open System.Net.Http
        open System.IO
        open System.IO.Compression
                
        let private userAgent = "EveALOD (https://github.com/jameson2011/EveAlod)"
        let private gzip = "gzip"        
            
        let httpClient()=
            let client = new System.Net.Http.HttpClient()
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent)
            client.DefaultRequestHeaders.AcceptEncoding.Add(System.Net.Http.Headers.StringWithQualityHeaderValue(gzip))
            client

        let decompressGzip(content: HttpContent)=
            async {
                use! stream = content.ReadAsStreamAsync() |> Async.AwaitTask
                use gzipStream = new GZipStream(stream, CompressionMode.Decompress)
                use rdr = new StreamReader(gzipStream)
                return! rdr.ReadToEndAsync() |> Async.AwaitTask
            }

        let extractContent (content: HttpContent) =
            let isGzip = content.Headers.ContentEncoding
                            |> Seq.contains(gzip)
            match isGzip with
            | false -> content.ReadAsStringAsync() |> Async.AwaitTask
            | _ -> decompressGzip content

        let getHeaderValue name (response: Http.HttpResponseMessage) = 
            response.Headers
                |> Seq.filter (fun h -> h.Key = name)
                |> Seq.collect (fun h -> h.Value)
                |> Seq.tryHead
        
                   
        let getAge (response: Net.Http.HttpResponseMessage)=
            Option.ofNullable response.Headers.Age 

        let getServerTime (response: Net.Http.HttpResponseMessage)=
            let age = getAge response
            Option.ofNullable response.Headers.Date
                |> Option.map DateTimeOffset.toUtc
                |> Option.map2 DateTime.addTimeSpan age
                    
        let getExpires (response: Net.Http.HttpResponseMessage)=
            Option.ofNullable response.Content.Headers.Expires 
                |> Option.map DateTimeOffset.toUtc
            
        let getWait (response: Net.Http.HttpResponseMessage) =           
            let expires = getExpires response            
            getServerTime response
                |> Option.map2 (DateTime.diff) expires 
                |> Option.map (max TimeSpan.Zero)
                    
        let getData (client: HttpClient) (url: string) =
            async {
                try                    
                    use! resp = client.GetAsync(url) |> Async.AwaitTask
                    
                    let! result = 
                        async {                       
                            let retry = getWait resp
                            match resp.StatusCode with
                            | HttpStatusCode.OK ->                                     
                                    use content = resp.Content
                                    let! s = extractContent content
                                    return (WebResponse.Ok retry s)
                            | x when (int x) = 429 -> 
                                    return (WebResponse.TooManyRequests retry)
                            | HttpStatusCode.Unauthorized -> 
                                    return (WebResponse.Unauthorized retry)
                            | x -> 
                                    return (WebResponse.Error retry x)
                             }
                    return result
                with e -> 
                    return (WebResponse.Error None (e.Message + e.StackTrace))
            }
