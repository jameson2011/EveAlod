namespace EveAlod.Data

    type DiscordChannel = { 
        Id: string; 
        Token: string
        }

    type Configuration = {
                    MinimumScore: float;
                    CorpId: string;
                    ChannelId: string;
                    ChannelToken: string;
                    }

