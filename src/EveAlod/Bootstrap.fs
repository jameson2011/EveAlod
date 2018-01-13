namespace EveAlod

    open EveAlod.Data
    open EveAlod.Services

    module Bootstrap =
        
        let private getDiscordChannel webhook = 
            match (EveAlod.Common.Web.getDiscordChannel webhook |> Async.RunSynchronously) with
            | Choice1Of2 (id, token) -> { DiscordChannel.Id = id; Token = token } 
            | Choice2Of2 msg -> failwith ("Error connecting to Discord: " + msg)

        let private applyDiscord config channel = 
            { config with ChannelId = channel.Id; ChannelToken = channel.Token }
            

        let config (webhookUri: string)=
            let c = (new ConfigProvider()).Configuration()
            match webhookUri with
            | "" -> c
            | _ -> webhookUri |> getDiscordChannel |> (applyDiscord c)
            
            

