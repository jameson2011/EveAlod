namespace EveAlod.Data

    open System.IO
    open System.Text
    open FSharp.Data

    module Serialization=
        open MBrace.FsPickler.Json

        let private jsonSerializer = FsPickler.CreateJsonSerializer(indent = false)

        let killToJson(kill: Kill)=
            let json = jsonSerializer.PickleToString kill    
            
            (kill.Id, json)

        let killFromJson (json: string) : Kill=
            let r = jsonSerializer.UnPickleOfString json

            r
            

