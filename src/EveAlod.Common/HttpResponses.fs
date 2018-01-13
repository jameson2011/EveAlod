namespace EveAlod.Common
    type HttpResponse =
    | OK of string
    | TooManyRequests
    | Unauthorized
    | Error of string 

