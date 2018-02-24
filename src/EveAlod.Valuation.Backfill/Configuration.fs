namespace EveAlod.Valuation.Backfill

open System

type BackfillConfiguration = {
    From: DateTime
    To: DateTime
    DestinationUri: Uri
    } with 
    static member Empty = { From = DateTime.MinValue; To = DateTime.MinValue; DestinationUri = new Uri("http://empty.com") }

