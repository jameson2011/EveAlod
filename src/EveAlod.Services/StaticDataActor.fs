namespace EveAlod.Services

    open System
    open EveAlod.Common
    open EveAlod.Data

    type private StaticDataCache=
        {
            Characters: Map<string, Character option>;
            SolarSystems: Map<string, SolarSystem option>
        }

    type private DataActorMessage=
        | GetCharacter of string * AsyncReplyChannel<Character option>
        | GetSolarSystem of string * AsyncReplyChannel<SolarSystem option>
        

    type StaticDataActor(log: Post, provider: IStaticEntityProvider)=
        
        let onException = Actors.postException typeof<StaticDataActor>.Name log

        let cacheGetOrSet (cache: Map<string, 'a option>) (get: string -> Async<'a option>) id =
            async {
                    match cache.TryFind(id) with
                    | Some x -> return cache, x
                    | _ ->
                        let! x = get id
                        let m = cache.Add(id, x)
                        return m, x
                }

        let solarSystem (cache: StaticDataCache) id=
            async {
                let! (m, s) = cacheGetOrSet cache.SolarSystems provider.SolarSystem id
                return { cache with SolarSystems = m }, s                
            }

        let character (cache: StaticDataCache) id =
            async {
                let! (m,c) = cacheGetOrSet cache.Characters provider.Character id
                return { cache with Characters = m}, c
            }

        let onRequest (cache: StaticDataCache) get id (chnl: AsyncReplyChannel<'a>) : Async<StaticDataCache> =
            async {             
                try
                    let! newCache, s = get cache id
                    chnl.Reply s
                    return newCache
                with e ->
                    onException e
                    return cache
            }


        let agent = MailboxProcessor<DataActorMessage>.Start(fun inbox ->
            let rec loop(cache: StaticDataCache) = async{
                let! msg = inbox.Receive()

                let! newCache = async {
                                        match msg with
                                        | GetSolarSystem (id, ch) -> 
                                            return! onRequest cache solarSystem id ch                                            
                                            
                                        | GetCharacter (id, ch) ->
                                            return! onRequest cache character id ch
                                    }

                return! loop(newCache)
            }

            let cache = { StaticDataCache.Characters = Map.empty<string, Character option>;
                            SolarSystems = Map.empty<string, SolarSystem option>
                            }
            loop(cache)
            )

        do agent.Error.Add(onException)

        member __.SolarSystem(id: string)=
            agent.PostAndAsyncReply (fun ch -> DataActorMessage.GetSolarSystem (id,ch))
            
        member __.Character(id: string) = 
            agent.PostAndAsyncReply (fun ch -> DataActorMessage.GetCharacter (id,ch))
