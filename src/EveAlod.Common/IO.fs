namespace EveAlod.Common

    open System.IO
    
    module IO=

        let combine subfolder folder =
            Path.Combine(folder, subfolder)

        let createDirectory folder =
            if not (Directory.Exists folder) then
                Directory.CreateDirectory folder |> ignore
            folder