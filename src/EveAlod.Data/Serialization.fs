namespace EveAlod.Data

    open System.IO
    open System.Text
    open FSharp.Data

    module Serialization=
        let private jsonSerializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof<Kill>)
        
        let toJson(kill: Kill)=
            (*
            @ in name (unions?) causes failure

            use ms = new MemoryStream()

            jsonSerializer.WriteObject(ms, kill)

            let json = Encoding.UTF8.GetString(ms.ToArray())
            *)
            
            let json = kill.ZkbUri
            
            (kill.Id, json)
            

