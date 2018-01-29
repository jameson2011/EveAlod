namespace EveAlod.Services

    open EveAlod.Data

    type Post= ActorMessage -> unit

    module Actors = 
        (*
        let forward (msg: 'a -> ActorMessage) (post: ActorMessage -> unit) value =
            value |> msg |> post
        *)
        let forwardMany  (getMsg: 'a -> ActorMessage) (posts: (ActorMessage -> unit) list) value =
            let msg = getMsg value
            posts |> List.iter (fun p -> p msg)            

        let postException name (log: ActorMessage -> unit) ex = (ActorMessage.Exception (name, ex)) |> log
        
