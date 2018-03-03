namespace EveAlod.Services

    open System
    open EveAlod.Data
    open EveAlod.Common.Strings

    module Config=
        
        let validateCorpId (config: Configuration)=
            seq {
                    if isNullWhitespace config.CorpId then
                        yield "Missing or empty corp ID. Check the configured ticker."
                }

        let validateDiscordId (config: Configuration) =
            seq {
                    if isNullWhitespace config.ChannelId then
                        yield "Missing or empty Discord channel ID. Check the configured Discord webhook URI."
                }
                
        let validateDiscordToken (config: Configuration) =
            seq {
                    if isNullWhitespace config.ChannelToken then
                        yield "Missing or empty Discord channel token. Check the configured Discord webhook URI."
                }


        let validateKillSource (config: Configuration) =
            seq{
                let ok,_  = System.Uri.TryCreate(config.KillSourceUri, UriKind.Absolute)
                if not ok then
                    yield "Missing or invalid killSourceUri in configuration."
            }

        let validateValuationUri (config: Configuration)=
            seq {
                    let ok,_ = System.Uri.TryCreate(config.KillValuationUri, UriKind.Absolute)
                    if not ok then
                        yield "Missing or invalid killValuationUri in configuration."
                }

        let validate config = 
            let msg = seq { 
                                yield! validateKillSource config;
                                yield! validateValuationUri config;
                                yield! validateDiscordId config;
                                yield! validateDiscordToken config;
                                yield! validateCorpId config;
                            } |> join Environment.NewLine
            if msg.Length > 0 then
                failwith msg
            
            config

        let report (log: PostMessage) (config: Configuration) =
            [ 
                sprintf "Kills source:   %s" config.KillSourceUri;
                sprintf "Kill valuation: %s" config.KillValuationUri;
                sprintf "Minimum score:  %f" config.MinimumScore;
                sprintf "Corp ticker:    %s" config.CorpTicker;
                sprintf "Corp ID:        %s" config.CorpId;
                sprintf "Discord:        %s" config.ChannelId;
                sprintf "Discord token:  %s" config.ChannelToken;
            ]            
            |> Seq.map ActorMessage.Info
            |> Seq.iter log
            config