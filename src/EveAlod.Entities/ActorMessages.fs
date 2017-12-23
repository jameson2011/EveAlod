namespace EveAlod.Entities
    type ActorMessage=
        | Start
        | Stop
        | GetNext of string
        | New of Kill
        | Tag of Kill
        | Tagged of Kill
        | Classify of Kill
        | Classified of Kill
        | Score of Kill
        | Scored of Kill
        | SendToDiscord of Kill
        | Log of Kill
        
