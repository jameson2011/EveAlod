namespace EveAlod.Data
    open System

    type ActorMessage=
        | Start
        | Stop
        | GetNext of string * TimeSpan
        | New of Kill
        | Tag of Kill
        | Tagged of Kill
        | Classify of Kill
        | Classified of Kill
        | Score of Kill
        | Scored of Kill
        | Publish of Kill
        | SendToDiscord of string
        | Log of Kill
        | Info of string
        | Exception of string * Exception
        | Error of string * string
        | Warning of string * string
        

    module Messages =
        let info = ActorMessage.Info