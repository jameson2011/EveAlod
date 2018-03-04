namespace EveAlod.Data

    type DiscordChannel = { 
        Id: string; 
        Token: string
        }

    type Configuration = {
                    MinimumScore: float;
                    
                    CorpTicker: string;
                    CorpId: string;
                    MinCorpDamage: float;

                    DiscordWebhookUri: string;
                    ChannelId: string;
                    ChannelToken: string;

                    KillSourceUri: string;
                    KillValuationUri: string;
                    
                    ValuationLimit: float;
                    ValuationSpread: float;

                    }

