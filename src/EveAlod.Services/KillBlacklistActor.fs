namespace EveAlod.Services

    open EveAlod.Common
    open EveAlod.Data
    open EveAlod.Common
        
    type KillBlacklistActor(config: Configuration, log: PostMessage, forward: PostKill) =

        let logTrace = Actors.postTrace typeof<KillValuationActor>.Name log
        let logException = Actors.postException typeof<KillFilterActor>.Name log

        let isBlacklisted (kill: Kill) = 
            kill.VictimShip |> Option.bind EntityTransforms.itemType
                            |> Option.map (ShipTransforms.isDrone <||> 
                                            (ShipTransforms.shipIsFittable >> not) )
                            |> Option.defaultValue false
            
        let isTooOld (kill: Kill) =
            System.DateTime.UtcNow - config.IgnoredKillAge >= kill.Occurred 

        let pipe = MessageInbox.Start(fun inbox -> 
            let rec getNext() = async {
                
                let! msg = inbox.Receive()

                try
                    match msg with                                    
                    | Killmail km ->    
                                if isBlacklisted km then
                                    km.Id |> sprintf "Kill %s ignored as blacklisted" |> logTrace
                                elif isTooOld km then
                                    km.Occurred.ToString() |> sprintf "Kill %s ignored as too old (%s)" km.Id |> logTrace
                                else                                     
                                    forward km
                    | _ ->      ignore 0
                                    
                with e ->   logException e                                            
                
                return! getNext()
                }
        
            getNext()
        )
        
        do pipe.Error.Add(logException)
        
        member this.Post(msg: ActorMessage) = pipe.Post msg
