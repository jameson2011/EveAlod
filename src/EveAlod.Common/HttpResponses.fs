namespace EveAlod.Common
    type HttpResponse =
    | OK of string
    | TooManyRequests
    | Error of string 

