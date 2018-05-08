namespace EveAlod.Common

[<AutoOpen>]
module Operators=
    let (<||>) f g = (fun x -> f x || g x)

    let (<&&>) f g = (fun x -> f x && g x)

    let (<++>) f g = (fun x -> (f x, g x))