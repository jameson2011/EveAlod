namespace EveAlod.Common
    
open System

module Retry=

    let retryIter iterations (func: unit -> 'a) (finish: 'a -> bool)=
        let rec retry =
            function
            | 0 ->  func()
            | x ->  let r = func() 
                    if finish r then r
                    else retry (x-1)
        retry iterations
                    
    let retryIterAsync iterations finish (func: unit -> Async<'a>)=
        let rec retry iteration =
            async {
                let! r = func()
                if (finish r) || (iteration >= iterations) then return r
                else return! retry (iteration + 1)
            }
        retry 0
                    
    let retryWaitIterAsync iterations (delay: int -> int) finish (func: unit -> Async<'a>) =
        let rec retry iteration =
            async {
                let! r = func() 
                if (finish r) || (iteration >= iterations) then return r
                else 
                    do! Async.Sleep (delay iteration)
                    return! retry (iteration+1)
            }
        retry 0

                    
    