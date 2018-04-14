namespace EveAlod.Services

    open System
    open FSharp.Data
    open IronSde
    open EveAlod.Data
    open EveAlod.Common
    open EveAlod.Common.Combinators
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
        let formatIsk (value: float) = value.ToString("N0")

        let getCharacters (chars: seq<Character>) =            
            chars 
                |> Seq.map (fun c -> c.Char.Id |> staticEntities.Character)
                |> Async.Parallel
                |> Async.RunSynchronously                  
                |> Array.filter Option.isSome
                |> Seq.map Option.get
                |> List.ofSeq

        let getCharacter (char: Character option) = 
            char |> Option.bind (fun c ->  [c] |> getCharacters |> Seq.tryHead ) 

        let getEntities (entities: seq<Entity>) =
            let entity id = 
                IronSde.ItemTypes.itemtype id
                |> Option.map (fun it -> { Entity.Id = it.id.ToString(); Name = it.name; })

            entities
                |> Seq.map (fun e -> e.Id |> Strings.toInt |> Option.bind entity)
                |> Seq.mapSomes
                |> List.ofSeq
        
        let titleLink (kill: Kill) = getTagText kill.Tags 
        let title kill = ("title", JsonValue.String(titleLink kill))

        let urlLink (kill: Kill) = kill.ZkbUri
        let url kill = ("url", JsonValue.String(urlLink kill))
        let color color = ("color", JsonValue.Float(float color))
        
        let footer() = 
            ("footer", JsonValue.Record([| "text", JsonValue.String("provided by evealod, jameson2011, zkillboard.com & ccp") |] ))

        let regionLink (value: Region) =
            value.Id |> Zkb.regionKillsUri |> sprintf "[%s](%s)" value.Name

        let regionStatsLink (value: Region) =
            value.Id |> Zkb.regionStatsUri |> sprintf "[%s](%s)" value.Name

        let constellationLink (value: Constellation) =
            sprintf "%s" value.Name

        let solarSystemLink (value: SolarSystem) =
            value.Id |> Zkb.solarSystemKillsUrl |> sprintf "[%s](%s)" value.Name 

        let solarSystemStatsLink (value: SolarSystem) =
            value.Id |> Zkb.solarSystemStatsUrl |> sprintf "[%s](%s)" value.Name 

        let solarSystemSecurity (value: SolarSystem) =
            sprintf "%A: %s" value.Security (value.SecurityLevel.ToString("N1"))

        let celestialLink (value: Celestial) =
            value.Id |> Zkb.locationKillsUri |> sprintf "[%s](%s)" value.Name 

        let celestialStatsLink (value: Celestial) =
            value.Id |> Zkb.locationStatsUri |> sprintf "[%s](%s)" value.Name 

        let characterLink (value: Character) =
            value.Char.Id |> Zkb.characterKillboardUri |> sprintf "[%s](%s)" value.Char.Name

        let characterStatsLink (value: Character) =
            value.Char.Id |> Zkb.characterKillboardStatsUri |> sprintf "[%s](%s)" value.Char.Name

        let shipTypeStatsLink (value: Entity) =
            value.Id |> Zkb.shipTypeStatsUri |> sprintf "[%s](%s)" value.Name

        let shipTypeLink (value: Entity) =
            value.Id |> Zkb.shipTypeKillsUri |> sprintf "[%s](%s)" value.Name 

        let distanceText (value: float<m>) = 
            let km,au = value |> (Units.metresToKm <++> Units.metresToAU)
            if System.Math.Round(decimal au, 2) >= 0.01m then 
                sprintf "%.2f AU" au
            else
                km  |> float 
                    |> Strings.expandFloat
                    |> Option.map (sprintf "%s km")
                    |> Option.defaultWith (fun () -> sprintf "%.2f m" value)
            
        let getLocationText (location: Location option) =
            match location with
            | Some l ->     let cel = l.Celestial |> Option.map celestialLink |> Option.defaultValue ""
                            let distance = l.Distance |> Option.map distanceText |> Option.defaultValue ""
                            let region = l.Region |> regionLink
                            let cons = l.Constellation |> constellationLink
                            let sys = l.SolarSystem |> solarSystemLink
                            let sec = l.SolarSystem |> solarSystemSecurity

                            match cel, distance with
                            | "","" -> sprintf "%s - %s - %s (%s)" sys cons region sec
                            | c,"" -> sprintf "%s - %s - %s - %s (%s)" c sys cons region  sec
                            | c,d -> sprintf "%s (%s) - %s - %s - %s (%s)" c d sys cons region  sec
            | _ -> ""

            

        let valueField (kill: Kill) = 
            let value = formatIsk kill.TotalValue
            match kill.TotalValueValuation with
            | Some v -> [| ("name", toJsonValueString "value");
                            ("value", (v * 100.) 
                                        |> sprintf "**%s ISK**, **%.2f%%** of highest value for this ship type" value 
                                        |> toJsonValueString) |] 
            | _ -> Array.empty
            

        let viewableTags (tags: KillTag list) =
            let viewable (tag: KillTag) =
                match tag with
                | PlayerKill | CorpKill | CorpLoss
                | Spendy | Expensive | Cheap | ZeroValue
                | WideMarginShipType | NarrowMarginShipType -> false
                | _ -> true
            tags |> List.filter viewable

        let tagsField (tags: KillTag list) =
            match tags with
            | [] -> Array.empty 
            | _ ->  
                    let tags = tags |> Seq.map (sprintf "%A") |> Strings.join ", "
                    [| 
                        ("name", toJsonValueString "tags");
                        ("value", toJsonValueString tags);
                    |]


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

        let statsField (kill: Kill)=
            
            let text = [    kill.Victim  |> Option.map characterStatsLink;
                            kill.VictimShip |> Option.map shipTypeStatsLink;
                            kill.Location |> Option.map (fun l -> l.SolarSystem) |> Option.map solarSystemStatsLink;                  
                        ] 
                        |> Seq.mapSomes   
                        |> Strings.join " / "
            match text with
            | "" -> [||]
            | t -> [|   ("name", toJsonValueString "stats");
                        ("value", toJsonValueString t) |]

        let corpMatesField chars = attackersField (function 
                                                    | [ _ ] -> "glorious victor"
                                                    | _ -> "brothers in arms") chars
            
        let killWhoresField chars =  attackersField (fun _ -> "kill whores") chars            

        let opponentsField chars = attackersField (function             
                                                    | [ _ ] -> "opponent"
                                                    | [ _;_ ] -> "opponents"
                                                    | _ -> "blobbers") chars            

        let victimLink (character: Character option) =            
            character |> Option.map characterLink |> Option.defaultValue ""

            
        let shipTypeThumbnailLink (shipType: Entity option) =
            (match shipType with
            | Some e -> e.Id
            | _ -> "0") |> sprintf "https://image.eveonline.com/Render/%s_128.png" 

        let shipTypeThumbnail (shipType: Entity option)= 
            ("thumbnail", JsonValue.Record([| "url", JsonValue.String(shipTypeThumbnailLink shipType) |] ))            

        let descriptionField (kill: Kill) =
            let location = getLocationText kill.Location
            
            let value = kill.TotalValue
            let victimShipTypeLink = kill.VictimShip |> Option.map shipTypeLink                                        
            let victim = victimLink kill.Victim
            
            let text = match victim, victimShipTypeLink with
                        | "", None ->       sprintf "**%s ISK** gone\n%s" (formatIsk value) location
                        | "", Some vst ->   sprintf "**%s ISK** %s gone\n%s" (formatIsk value) vst location
                        | v, Some vst ->    sprintf "%s lost a **%s ISK** %s\n%s" v (formatIsk value) vst location
                        | v, None ->        sprintf "%s lost **%s ISK**\n%s" v (formatIsk value) location

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
            let kill = { kill with Victim = getCharacter kill.Victim; } 
            
            let attackers = getKillNonCorpCharacters corpId kill |> opponentsField 
            let tagsField = kill.Tags |> viewableTags |> tagsField
            let valueField = valueField kill
            let statsField = statsField kill

            let fields =  [| attackers; valueField; statsField; tagsField |] 
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
            let kill = { kill with Victim = getCharacter kill.Victim ; } 
            
            let corpMates = getKillCorpCharacters corpId kill |> corpMatesField 
            let killwhores = getKillNonCorpCharacters corpId kill |> killWhoresField 
            let valueField = valueField kill
            let statsField = statsField kill
            let tagsField = kill.Tags |> viewableTags |> tagsField

            let fields =  [| corpMates; killwhores; valueField; statsField; tagsField |] 
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
            let kill = { kill with Victim = getCharacter kill.Victim;  }

            let opponents = getKillNonCorpCharacters corpId kill |> opponentsField 
            let tagsField = kill.Tags |> viewableTags |> tagsField
            let valueField = valueField kill
            let statsField = statsField kill

            let fields =  [| opponents; valueField; statsField; tagsField |] 
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
            
            
