namespace EveAlod.Entities

    // TODO: temporary... literals are evil. need more robust classification
    module EntityTypes=

        
        let private skillInjectorIds = 
            [ "40520"; "42523"; "43630"; "45635"; "46375" ]
            |> Set.ofSeq

        let private ecmIds=
            [ "1948"; "1955"; "1956"; "1957"; "1958"; "2559"; "2563"; "2567"; "2571"; "2575";
                "9518"; "9519"; "9520"; "9521"; "9522"; "19923"; "19925"; "19927"; "19929"; 
                "19931"; "19933"; "19935"; "19937"; "19939"; "19942"; "19944"; "19946";
                "19948"; "19950"; "19952"; "20199"; "20201"; "20203"; "20205"; "20207"; 
                "20573"; "20575"; "20577"; "20579"; "28729"; "28731"; "28733"; "28735";
                "28737" ]
            |> Set.ofSeq

        let private plexIds = 
            [ "44992"; ]
            |> Set.ofSeq

        let private podIds = 
            [ "670" ]
            |> Set.ofSeq

        let private isMatch (ids: Set<string>) (e: Entity) =
            Set.contains e.Id ids

        let isPod = isMatch podIds 
            
        let isPlex = isMatch plexIds

        let isSkillInjector = isMatch skillInjectorIds
        
        let isEcm = isMatch ecmIds
