namespace EveAlod.Valuation

type ValuationActorMessage=
    | ImportKillJson of string
    | GetShipTypeStats of string * AsyncReplyChannel<ShipTypeStatistics>
    | GetShipSummaryStats of AsyncReplyChannel<ShipSummaryStatistics>

type MessageInbox = MailboxProcessor<ValuationActorMessage>
type PostMessage = EveAlod.Data.ActorMessage -> unit

module Actors = 
    open EveAlod.Data

    let forwardMany  (getMsg: 'a -> ActorMessage) (posts: (PostMessage) list) value =
        let msg = getMsg value
        posts |> List.iter (fun p -> p msg)            

    let postException name (log: PostMessage) ex = (ActorMessage.Exception (name, ex)) |> log
