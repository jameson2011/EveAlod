namespace EveAlod.Data
    module Web=

        open System.IO
        open System.Net
    
        let private userAgent = "Maintainer:" // TODOTK: make configurable
            
        let private getData (url: string) =
            async {
                try
                    let req = HttpWebRequest.Create(url) :?> HttpWebRequest
                    req.UserAgent <- userAgent 
                    let! resp = req.AsyncGetResponse()
                    use stream = resp.GetResponseStream()
                    let rdr = new StreamReader(stream)
                    return Some (rdr.ReadToEnd())
                with _ -> return None
            }

        let private getEsiData (uri) (id: string) =        
            id
            |> sprintf uri 
            |> getData

        
        let getKm (url: string) =
            getData url
            

        
        let sendDiscord (channelId: string) (token: string) (content: string)=
            async {
                // TODOTK: error handling
                // post to this...
                let url = sprintf "https://discordapp.com/api/webhooks/%s/%s" channelId token
                                
                use client = new System.Net.Http.HttpClient()
                // TODOTK: client.DefaultRequestHeaders.Add("User-Agent", userAgent)
        
                let values = new System.Collections.Generic.Dictionary<string, string>()
                values.Add("content", content)
                let content = new System.Net.Http.FormUrlEncodedContent(values)

                let! response = client.PostAsync(url, content) |> Async.AwaitTask
                response |> ignore
            }