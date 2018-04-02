namespace EveAlod.Services
    
    open EveAlod.Data

    type IStaticEntityProvider=
        abstract member EntityIds: EntityGroupKey -> Async<Set<string> option>
        abstract member Entity: string -> Async<Entity option>
        abstract member Character: string -> Async<Character option>
        abstract member CorporationByTicker: string -> Async<Entity option>

