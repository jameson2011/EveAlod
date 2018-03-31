namespace EveAlod.Services

    open System
    open FSharp.Data
    open EveAlod.Data
    open EveAlod.Common.Strings

    type DiscordKillMessageBuilder(staticEntities: StaticDataActor, corpId: string)=

        let red = 16711680
        let green = 65280
        let blue = 255

        let toJsonString (js: JsonValue) = js.ToString()
        let toJsonValueString s = JsonValue.String(s)
        let toJsonRecord js = JsonValue.Record js
        let toEmbedsJson xs = (JsonValue.Record [| ("embeds", JsonValue.Array [| JsonValue.Record xs |] ) |])
        let toFieldsJson xs = ("fields", JsonValue.Array xs)

        let getTagText =  Commentary.getText |> Commentary.getTagsText 

        let getCharacters (chars: seq<Character>) =            
            chars 
                |> Seq.map (fun c -> c.Char.Id |> staticEntities.Character)
                |> Async.Parallel
                |> Async.RunSynchronously                  
                |> Array.filter (fun c -> c.IsSome)
                |> Seq.map (fun c -> c.Value)
                |> List.ofSeq

            
        let titleLink (kill: Kill) = getTagText kill.Tags 
        let title kill = ("title", JsonValue.String(titleLink kill))

        let urlLink (kill: Kill) = kill.ZkbUri
        let url kill = ("url", JsonValue.String(urlLink kill))
        let color color = ("color", JsonValue.Float(float color))
        
        let footer() = 
            ("footer", JsonValue.Record([| "text", JsonValue.String("provided by evealod, jameson2011, zkillboard.com & ccp") |] ))

        let regionLink (value: Region) =
            sprintf "[%s](https://zkillboard.com/region/%i/)" value.Name value.Id
        let constellationLink (value: Constellation) =
            sprintf "%s" value.Name
        let solarSystemLink (value: SolarSystem) =
            sprintf "[%s](https://zkillboard.com/system/%i/)" value.Name value.Id
        let solarSystemSecurity (value: SolarSystem) =
            sprintf "%A: %s" value.Security (value.SecurityLevel.ToString("N1"))
        let celestialLink (value: Celestial) =
            sprintf "[%s](https://zkillboard.com/location/%i/)" value.Name value.Id
        let characterLink (value: Character) =
            sprintf "[%s](https://zkillboard.com/character/%s/)" value.Char.Name value.Char.Id
        let getLocationText (location: Location option) =
            match location with
            | Some l ->     let cel = l.Celestial |> Option.map celestialLink |> Option.defaultValue ""
                            let r = l.Region |> regionLink
                            let c = l.Constellation |> constellationLink
                            let s = l.SolarSystem |> solarSystemLink
                            let sec = l.SolarSystem |> solarSystemSecurity

                            match cel with
                            | "" -> sprintf "%s - %s - %s (%s)" s c r sec
                            | _ -> sprintf "%s - %s - %s - %s (%s)" cel s c r  sec
            | _ -> ""

        let composeCharNames (characters: Character list) = 
            characters 
                |> Seq.filter (fun c ->  not (String.IsNullOrWhiteSpace(c.Char.Name)))
                |> Seq.map characterLink
                |> List.ofSeq
                |> prettyConcat            

        let getCharNames maxCount chars =
            let group = chars |> Seq.truncate (maxCount + 1) |> Array.ofSeq
            let suffix = if group.Length > maxCount then " + others" else ""
            group |> Seq.truncate maxCount
                |> getCharacters 
                |> composeCharNames 
                |> (fun s -> s + suffix)

        let attackersField (title: Character list -> string) chars = 
            match chars with
            | [] -> Array.empty
            | chars -> 
                        [| 
                            ("name", toJsonValueString (title chars));
                            ("value", chars |> getCharNames 3 |> toJsonValueString )  
                        |]

        let corpMatesField chars = attackersField (function 
                                                    | [ _ ] -> "glorious victor"
                                                    | _ -> "brothers in arms") chars
            
        let killWhoresField chars =  attackersField (fun _ -> "kill whores") chars            

        let opponentsField chars = attackersField (function             
                                                    | [ _ ] -> "opponent"
                                                    | [ _;_ ] -> "opponents"
                                                    | _ -> "blobbers") chars            

        let victimLink (character: Character option) =            
            match character |> Option.map (fun c -> getCharacters [ c ] ) |> Option.defaultValue [] with
            | [ character ] -> characterLink character
            | _ -> ""
            
        let shipTypeThumbnailLink (shipType: Entity option) =
            match shipType with
            | Some e -> sprintf "https://image.eveonline.com/Render/%s_128.png" e.Id
            | _ -> ""

        let shipTypeThumbnail (shipType: Entity option)= 
            ("thumbnail", JsonValue.Record([| "url", JsonValue.String(shipTypeThumbnailLink shipType) |] ))            

        let descriptionField (kill: Kill)=
            let location = getLocationText kill.Location                                    
            let victim = victimLink kill.Victim
            let value = kill.TotalValue

            let text = match victim with
                        | "" -> sprintf "**%s ISK** gone\n%s" (value.ToString("N2")) location
                        | v ->  sprintf "%s lost **%s ISK**\n%s" v (value.ToString("N2")) location

            ("description", text |> toJsonValueString)

        let isCorpie (corpId) (attacker: Attacker): bool=
            let attackerCorpId = attacker.Char 
                                    |> Option.bind (fun c -> c.Corp)
                                    |> Option.map (fun c -> c.Id)
            match attackerCorpId with
            | Some id -> id = corpId
            | _ -> false
                                
        let getKillCorpCharacters corpId km =
            km.Attackers 
            |> Seq.sortByDescending (fun a -> a.Damage)
            |> Seq.filter (isCorpie corpId)
            |> Seq.filter (fun a -> a.Char.IsSome)
            |> Seq.map (fun a -> a.Char.Value)
            |> List.ofSeq

        let getKillNonCorpCharacters corpId km =
            km.Attackers 
            |> Seq.sortByDescending (fun a -> a.Damage)
            |> Seq.filter ((isCorpie corpId) >> not)
            |> Seq.filter (fun a -> a.Char.IsSome)
            |> Seq.map (fun a -> a.Char.Value)
            |> List.ofSeq
            
        
        let getCorpLossMsg (kill: Kill) =             
                                   
            let attackers = getKillNonCorpCharacters corpId kill |> opponentsField 
            let fields =  [| attackers |] 
                            |> Array.filter (Array.isEmpty >> not)
                            |> Array.map toJsonRecord
                            |> toFieldsJson

            let elements = [|   title kill; 
                                url kill; 
                                color red; 
                                footer(); 
                                shipTypeThumbnail kill.VictimShip; 
                                descriptionField kill;
                                fields|]
            elements |> toEmbedsJson |> toJsonString
            
        let getCorpWinMsg (kill: Kill) =             
                        
            let corpMates = getKillCorpCharacters corpId kill |> corpMatesField 
            let killwhores = getKillNonCorpCharacters corpId kill |> killWhoresField 

            let fields =  [| corpMates; killwhores |] 
                            |> Array.filter (Array.isEmpty >> not)
                            |> Array.map toJsonRecord
                            |> toFieldsJson
            
            let elements = [|   title kill; 
                                url kill; 
                                color green; 
                                footer(); 
                                shipTypeThumbnail kill.VictimShip; 
                                descriptionField kill;
                                fields|]

            elements |> toEmbedsJson |> toJsonString

        let getGenericMsg kill = 
                        
            let opponents = getKillNonCorpCharacters corpId kill |> opponentsField 

            let fields =  [| opponents |] 
                            |> Array.filter (Array.isEmpty >> not)
                            |> Array.map toJsonRecord
                            |> toFieldsJson
            
            let elements = [|   title kill; 
                                url kill; 
                                color blue; 
                                footer(); 
                                shipTypeThumbnail kill.VictimShip; 
                                descriptionField kill;
                                fields|]

            elements |> toEmbedsJson |> toJsonString
        
        
        member __.CreateMessage(kill) = 
            let check t = kill.Tags |> List.contains t
            
            kill |> (if check KillTag.CorpKill then
                        getCorpWinMsg
                    else if check KillTag.CorpLoss then
                        getCorpLossMsg 
                    else
                        getGenericMsg
                        )
            
            
