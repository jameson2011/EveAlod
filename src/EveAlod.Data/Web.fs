namespace EveAlod.Data
    module Web=

        open System.IO
        open System.Net
    
        let private userAgent = "EveALOD Maintainer:" // TODO: make configurable
            
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
        
        
        let sendDiscord (channelId: string) (token: string) (content: string)=
            async {
                
                try
                    let url = sprintf "https://discordapp.com/api/webhooks/%s/%s" channelId token
                                
                    use client = new System.Net.Http.HttpClient()
                    // TODO: client.DefaultRequestHeaders.Add("User-Agent", userAgent)
        
                    let values = new System.Collections.Generic.Dictionary<string, string>()
                    values.Add("content", content)
                    let content = new System.Net.Http.FormUrlEncodedContent(values)

                    let! response = client.PostAsync(url, content) |> Async.AwaitTask
                    // TODO: interrogate response headers for rate limiting; return error status & duration
                    response |> ignore
                with _ -> 0 |> ignore
            }
