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
        | SendToDiscord of Kill
        | Log of Kill
        | Exception of string * Exception
        | Error of string * string
        | Warning of string * string
        
