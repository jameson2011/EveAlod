namespace EveAlod.Data
    open System

    type ActorMessage=
        | Start
        | Stop
        | GetNextKill of string * TimeSpan
        | KillJson of string
        | Kill of Kill
        | SendToDiscord of string
        | Trace of string * string
        | Info of string
        | Exception of string * Exception
        | Error of string * string
        | Warning of string * string
        
        
    