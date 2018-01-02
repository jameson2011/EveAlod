namespace EveAlod.Data
    type HttpResponse =
    | OK of string
    | TooManyRequests
    | Error of string

