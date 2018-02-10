namespace EveAlod.Common

module Discord=

    open System
    open System.Net
    open System.Net.Http
    open FSharp.Data
    open EveAlod.Common.Web

    type Discord429Payload = JsonProvider<"""{ "message": "a", "retry_after": 0, "global":  false }""">
    type DiscordWebhookPayload = JsonProvider<"./SampleDiscordWebhookResponse.json">
        
    let getDiscordRateLimitReset(response: Http.HttpResponseMessage) =
            response
                |> getHeaderValue "X-RateLimit-Reset"
                |> Strings.toIntValue
                |> DateTime.getUtcFromEpoch
        
    let parseDiscordResponse (response: Http.HttpResponseMessage) =
            async {
                match response.StatusCode with
                | HttpStatusCode.OK 
                | HttpStatusCode.NoContent -> 
                    let remoteTime = response.Headers.Date |> DateTime.ofDateTimeOffset (DateTime.UtcNow.AddMinutes(-2.)) 
                    let remoteResetTime = (getDiscordRateLimitReset response)                    
                    let wait = remoteResetTime - remoteTime
                    
                    return { WebResponse.Status = HttpStatus.OK;
                                                Retry = Some wait;
                                                Message = ""
                                            }                    

                | x when (int x) = 429 ->                     
                    use content = response.Content
                    let! s = content.ReadAsStringAsync() |> Async.AwaitTask                    
                    let responsePayload = Discord429Payload.Parse(s)
                    
                    let wait = TimeSpan.FromMilliseconds(float responsePayload.RetryAfter)
                    
                    return { WebResponse.Status = HttpStatus.TooManyRequests;
                                                Retry = Some wait;
                                                Message = s
                                            }
                | HttpStatusCode.Unauthorized ->
                    return { WebResponse.Status = HttpStatus.Unauthorized;
                                Retry = Some (TimeSpan.FromSeconds(30.));
                                Message = ""
                            }
                | _ -> 
                    return { WebResponse.Status = HttpStatus.Error;
                                Retry = Some (TimeSpan.FromSeconds(30.));
                                Message = "Unknown error"
                            }
            }
    
    let parseDiscordWebhookResponse (response: Http.HttpResponseMessage)=
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
                    use! response = client.GetAsync(webhookUri) |> Async.AwaitTask
                    return! parseDiscordWebhookResponse response
                with e -> 
                    return Choice2Of2 e.Message                
            }
    
    let sendDiscord (client: HttpClient) (channelId: string) (token: string) (content: string)=
            async {                
                try
                    let url = sprintf "https://discordapp.com/api/webhooks/%s/%s" channelId token
        
                    let values = new System.Collections.Generic.Dictionary<string, string>()
                    values.Add("content", content)
                    let content = new System.Net.Http.FormUrlEncodedContent(values)
                    
                    use! response = client.PostAsync(url, content) |> Async.AwaitTask
                    
                    return! parseDiscordResponse response
                    
                with e -> 
                    return { WebResponse.Status = HttpStatus.Error;
                                Retry = Some (TimeSpan.FromSeconds(30.));
                                Message = "Unknown error: " + e.Message + e.StackTrace
                            }
            }
            