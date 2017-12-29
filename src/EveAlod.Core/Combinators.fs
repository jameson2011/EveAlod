namespace EveAlod.Core

    [<AutoOpen>]
    module Combinators=
        let (<|>) f g = (fun x -> f x || g x)

