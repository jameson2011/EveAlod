namespace EveAlod.Common
    module Web=

        open System
        open System.IO
        open System.Net
        open FSharp.Data

        type Discord429Payload = JsonProvider<"""{ "message": "a", "retry_after": 0, "global":  false }""">
        type DiscordWebhookPayload = JsonProvider<"./SampleDiscordWebhookResponse.json">
        
        let private userAgent = "EveALOD (https://github.com/jameson2011/EveAlod)"

        let private httpClient()=
            let client = new System.Net.Http.HttpClient()
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent)
            client

        let private getHeaderValue name (response: Http.HttpResponseMessage) = 
            response.Headers
                |> Seq.filter (fun h -> h.Key = name)
                |> Seq.collect (fun h -> h.Value)
                |> Seq.tryHead
        
        let private getDiscordRateLimitReset(response: Http.HttpResponseMessage) =
            response
            |> getHeaderValue "X-RateLimit-Reset"
            |> Strings.toIntValue
            |> DateTime.getUtcFromEpoch
                   
        let private parseDiscordResponse (response: Http.HttpResponseMessage) =
            async {
                match response.StatusCode with
                | HttpStatusCode.OK 
                | HttpStatusCode.NoContent -> 
                    let reset = (getDiscordRateLimitReset response)
                    
                    let clientServerDiff = DateTime.machineTimeOffset DateTime.UtcNow response.Headers.Date
                
                    let wait = (DateTime.UtcNow + clientServerDiff - reset)
                    return wait, (HttpResponse.OK "")

                | x when (int x) = 429 ->                     
                    use content = response.Content
                    let! s = content.ReadAsStringAsync() |> Async.AwaitTask                    
                    let responsePayload = Discord429Payload.Parse(s)
                    
                    let wait = TimeSpan.FromMilliseconds(float responsePayload.RetryAfter)
                    
                    return wait, (HttpResponse.TooManyRequests)
                | HttpStatusCode.Unauthorized ->
                    return TimeSpan.FromSeconds(30.), (HttpResponse.Unauthorized)
                | _ -> 
                    return TimeSpan.FromSeconds(30.), (HttpResponse.Error "Unknown error")
            }

        let private parseDiscordWebhookResponse (response: Http.HttpResponseMessage)=
            async {
                match response.StatusCode with
                | HttpStatusCode.OK  -> 
                    let! json = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                    let data = DiscordWebhookPayload.Parse(json)                    
                    return Choice1Of2 (data.Id, data.Token)
                | HttpStatusCode.Unauthorized ->
                    return Choice2Of2 "Unauthorized"
                | _ -> 
                    return Choice2Of2 ("HTTP error " + (int response.StatusCode).ToString())
            }

        let getDiscordChannel (webhookUri: string)=
            async {
                try
                    use client = httpClient()
                    let! response = client.GetAsync(webhookUri) |> Async.AwaitTask
                    return! parseDiscordWebhookResponse response
                with e -> 
                    return Choice2Of2 e.Message                
            }
        
        let sendDiscord (channelId: string) (token: string) (content: string)=
            async {                
                try
                    let url = sprintf "https://discordapp.com/api/webhooks/%s/%s" channelId token
                                
                    use client = httpClient()
        
                    let values = new System.Collections.Generic.Dictionary<string, string>()
                    values.Add("content", content)
                    let content = new System.Net.Http.FormUrlEncodedContent(values)

                    let! response = client.PostAsync(url, content) |> Async.AwaitTask
                    
                    return! parseDiscordResponse response
                    
                with _ -> 
                    return TimeSpan.FromSeconds(30.), (HttpResponse.Error "Unknown error")
            }
            
        let getData (url: string) =
            async {
                try
                    use client = httpClient()
                    
                    let! resp = client.GetAsync(url) |> Async.AwaitTask
                    
                    let! result = 
                        async {
                            match resp.StatusCode with
                            | HttpStatusCode.OK -> 
                                    use content = resp.Content
                                    let! s = content.ReadAsStringAsync() |> Async.AwaitTask                    
                                    
                                    return HttpResponse.OK (s)
                            | x when (int x) = 429 -> 
                                    return HttpResponse.TooManyRequests
                            | HttpStatusCode.Unauthorized -> 
                                    return HttpResponse.Unauthorized
                            | x -> 
                                    return HttpResponse.Error (sprintf "Error %s getting data" (x.ToString()) )

                             }
                    return result
                with e -> 
                    return HttpResponse.Error e.Message
            }

