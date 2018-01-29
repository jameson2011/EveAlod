namespace EveAlod.Services

    open EveAlod.Data

    type PostMessage= ActorMessage -> unit
    type PostKill = Kill -> unit
    type MessageInbox = MailboxProcessor<ActorMessage>

    module Actors = 
        let forwardMany  (getMsg: 'a -> ActorMessage) (posts: (PostMessage) list) value =
            let msg = getMsg value
            posts |> List.iter (fun p -> p msg)            

        let postException name (log: PostMessage) ex = (ActorMessage.Exception (name, ex)) |> log
        
