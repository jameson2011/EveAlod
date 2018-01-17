namespace EveAlod.Common

    open System

    module DateTimeOffset=

        let toUtc (x: DateTimeOffset) = x.UtcDateTime

