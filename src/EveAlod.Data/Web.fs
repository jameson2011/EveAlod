namespace EveAlod.Data
    module Web=

        open System
        open System.IO
        open System.Net
        open FSharp.Data

        type Discord429Payload = JsonProvider<"""{ "message": "a", "retry_after": 0, "global":  false }""">
        
        let private userAgent = "EveALOD (https://github.com/jameson2011/EveAlod)"
            
        let getData (url: string) =
            async {
                try
                    let req = HttpWebRequest.Create(url) :?> HttpWebRequest
                    req.UserAgent <- userAgent 
                    let! resp = req.AsyncGetResponse()
                    let result = 
                            match (resp :?> HttpWebResponse).StatusCode with
                            | HttpStatusCode.OK -> 
                                    use stream = resp.GetResponseStream()
                                    let rdr = new StreamReader(stream)
                                    HttpResponse.OK (rdr.ReadToEnd())
                            | x when (int x) = 429 -> 
                                    HttpResponse.TooManyRequests
                            | _ -> 
                                    HttpResponse.Error "Unknown error"
                    return result
                with e -> return HttpResponse.Error e.Message
            }

        let private getDiscordRateLimitReset(response: Http.HttpResponseMessage) =
            response
            |> EveAlod.Core.Http.getHeaderValue "X-RateLimit-Reset"
            |> EveAlod.Core.Http.getIntValue
            |> EveAlod.Core.DateTime.getUtcFromEpoch
                   
        let private parseDiscordResponse (response: Http.HttpResponseMessage) =
            async {
                match response.StatusCode with
                | HttpStatusCode.OK 
                | HttpStatusCode.NoContent -> 
                    let reset = (getDiscordRateLimitReset response)
                    
                    let clientServerDiff = EveAlod.Core.DateTime.machineTimeOffset DateTime.UtcNow response.Headers.Date
                
                    let wait = (DateTime.UtcNow + clientServerDiff - reset)
                    return wait, (HttpResponse.OK "")

                | x when (int x) = 429 ->                     
                    use content = response.Content
                    let! s = content.ReadAsStringAsync() |> Async.AwaitTask                    
                    let responsePayload = Discord429Payload.Parse(s)
                    
                    let wait = TimeSpan.FromMilliseconds(float responsePayload.RetryAfter)
                    
                    return wait, (HttpResponse.TooManyRequests)

                | _ -> 
                    return TimeSpan.FromSeconds(30.), (HttpResponse.Error "Unknown error")
            }
        
        let sendDiscord (channelId: string) (token: string) (content: string)=
            async {
                
                try
                    let url = sprintf "https://discordapp.com/api/webhooks/%s/%s" channelId token
                                
                    use client = new System.Net.Http.HttpClient()                    
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent)
                    
        
                    let values = new System.Collections.Generic.Dictionary<string, string>()
                    values.Add("content", content)
                    let content = new System.Net.Http.FormUrlEncodedContent(values)

                    let! response = client.PostAsync(url, content) |> Async.AwaitTask
                    
                    return! parseDiscordResponse response
                    
                with _ -> return TimeSpan.FromSeconds(30.), (HttpResponse.Error "Unknown error")
            }
