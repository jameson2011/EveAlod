namespace EveAlod.Common

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
        static member ok retry message =
                {   WebResponse.Status = HttpStatus.OK;
                    Retry = retry;
                    Message = message
                }
        static member unauthorized retry = 
                {   Status = HttpStatus.Unauthorized;
                    Retry = retry;
                    Message = "";
                }
        static member tooManyRequests retry = 
                {   Status = HttpStatus.TooManyRequests;
                    Retry = retry;
                    Message = "";
                }
        static member error retry error = 
                {   Status = HttpStatus.Error;
                    Retry = retry;
                    Message = (sprintf "Error %s getting data" (error.ToString()) );
                }
        
    module Web=

        open System.Net
        open System.Net.Http
                
        let private userAgent = "EveALOD (https://github.com/jameson2011/EveAlod)"
        
            
        let httpClient()=
            let client = new System.Net.Http.HttpClient()
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent)            
            client

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
                                    let! s = content.ReadAsStringAsync() |> Async.AwaitTask
                                    return (WebResponse.ok retry s)
                            | x when (int x) = 429 -> 
                                    return (WebResponse.tooManyRequests retry)
                            | HttpStatusCode.Unauthorized -> 
                                    return (WebResponse.unauthorized retry)
                            | x -> 
                                    return (WebResponse.error retry x)
                             }
                    return result
                with e -> 
                    return (WebResponse.error None (e.Message + e.StackTrace))
            }
