namespace EveAlod.Services

    open EveAlod.Data

    type PostMessage= ActorMessage -> unit
    type PostKill = Kill -> unit
    type PostString = string -> unit
    type MessageInbox = MailboxProcessor<ActorMessage>

    module Actors = 
        let forwardMany  (getMsg: 'a -> ActorMessage) (posts: (PostMessage) list) value =
            let msg = getMsg value
            posts |> List.iter (fun p -> p msg)            

        let postException name (log: PostMessage) ex = (ActorMessage.Exception (name, ex)) |> log
        let postError name (log: PostMessage) msg = (ActorMessage.Error (name, msg)) |> log
        let postTrace name (log: PostMessage) ex = (ActorMessage.Trace (name, ex)) |> log
        let postInfo (log: PostMessage) = ActorMessage.Info >> log
        
