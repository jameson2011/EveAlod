namespace EveAlod.Common

    open System
    open FSharp.Data

    type HttpStatus =
        | OK
        | TooManyRequests
        | Unauthorized
        | Error
        | NotFound
    
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
        static member NotFound =
                {
                    Status = HttpStatus.NotFound;
                    Retry = None;
                    Message = "";
                }
        static member Error retry error = 
                {   Status = HttpStatus.Error;
                    Retry = retry;
                    Message = (sprintf "Error %s getting data" (error.ToString()) );
                }
    
    type EntityWebResponse<'a> =
        | OK of 'a 
        | TooManyRequests of TimeSpan option
        | NotFound
        | Unauthorized
        | SystemError of string
                
    module EntityWebResponse =
        let ofWebResponse<'a> (map: string -> 'a) (value: WebResponse)= 
            match value.Status with
            | HttpStatus.OK -> map value.Message |> EntityWebResponse.OK
            | HttpStatus.TooManyRequests ->  value.Retry |> EntityWebResponse.TooManyRequests
            | HttpStatus.Error -> value.Message |> EntityWebResponse.SystemError
            | HttpStatus.Unauthorized -> EntityWebResponse.Unauthorized
            | HttpStatus.NotFound -> EntityWebResponse.NotFound

    module Web=

        open System.Net
        open System.Net.Http
        open System.IO
        open System.IO.Compression
                
        let private userAgent = "EveALOD (https://github.com/jameson2011/EveAlod)"
        let private gzip = "gzip"        

        let jsonMimeType = "application/json"
            
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

        let postData (client: HttpClient) (url: string) (jsonContent: string)=
            async {
                try   
                    let bytes = System.Text.UTF8Encoding.UTF8.GetBytes(jsonContent)
                    use content = new System.Net.Http.ByteArrayContent(bytes)

                    use! resp = client.PostAsync(url, content) |> Async.AwaitTask
                    
                    let! result = 
                        async {                       
                            let retry = Some TimeSpan.Zero
                            match resp.StatusCode with
                            | HttpStatusCode.OK ->                                     
                                    use content = resp.Content
                                    let! s = extractContent content
                                    return (WebResponse.Ok retry s)
                            | HttpStatusCode.NoContent ->
                                    return (WebResponse.Ok retry "")
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
