namespace EveAlod.Services

    open EveAlod.Data

    type Post= ActorMessage -> unit

    module Actors = 
        let forward (msg: 'a -> ActorMessage) (post: ActorMessage -> unit) km =
            km |> msg |> post
        
        let postException name (log: ActorMessage -> unit) ex = (ActorMessage.Exception (name, ex)) |> log
