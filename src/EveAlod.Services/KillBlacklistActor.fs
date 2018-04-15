﻿namespace EveAlod.Services

    open EveAlod.Common
    open EveAlod.Data
    open EveAlod.Common
        
    type KillBlacklistActor(config: Configuration, log: PostMessage, forward: PostKill) =

        let logTrace = Actors.postTrace typeof<KillValuationActor>.Name log
        let logException = Actors.postException typeof<KillFilterActor>.Name log

        let blacklistedItemTypes = 
            config.IgnoredItemTypes 
                |> Seq.map Strings.toString
                |> Set.ofSeq

        let isBlacklisted (itemType: Entity option)= 
            itemType |> Option.map (fun t -> t.Id)
                     |> Option.map blacklistedItemTypes.Contains
                     |> Option.defaultValue false

        let isBlacklisted (kill: Kill) = 
            kill.VictimShip |> isBlacklisted
            

        let pipe = MessageInbox.Start(fun inbox -> 
            let rec getNext(buffer: MutableCappedBuffer<string>) = async {
                
                let! msg = inbox.Receive()

                let newBuffer = try
                                    match msg with                                    
                                    | Killmail km ->    
                                                if isBlacklisted km then
                                                    km.Id |> sprintf "Kill %s ignored as blacklisted" |> logTrace
                                                    buffer
                                                else if buffer.Contains(km.Id) then
                                                    km.Id |> sprintf "Kill %s ignored as a duplicate." |> logTrace
                                                    buffer
                                                else                                     
                                                    forward km
                                                    buffer.Add(km.Id)
                                    | _ ->      buffer
                                    
                                with e ->   logException e
                                            buffer
                
                return! getNext(newBuffer)
                }
        
            getNext(new MutableCappedBuffer<string>(10))
        )
        
        do pipe.Error.Add(logException)
        
        member this.Post(msg: ActorMessage) = pipe.Post msg