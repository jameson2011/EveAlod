namespace EveAlod.Common

    module Combinators=
        let (<||>) f g = (fun x -> f x || g x)

        let (<&&>) f g = (fun x -> f x && g x)

    