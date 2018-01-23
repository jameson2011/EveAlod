namespace EveAlod.Services

    open System
    open FSharp.Data
    open EveAlod.Common
    open EveAlod.Data

    type IStaticEntityProvider=
        abstract member EntityIds: EntityGroupKey -> Set<string>
        abstract member Entity: string -> Async<Entity option>
        abstract member Character: string -> Async<Character option>
        abstract member SolarSystem: string -> Async<SolarSystem option>


