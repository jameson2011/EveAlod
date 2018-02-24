namespace EveAlod.Data
    open System

    type ActorMessage=
        | Start
        | Stop
        | GetNextKill of string * TimeSpan
        | KillmailJson of string
        | Killmail of Kill
        | SendToDiscord of string
        | Trace of string * string
        | Info of string
        | Exception of string * Exception
        | Error of string * string
        | Warning of string * string
        
        
    