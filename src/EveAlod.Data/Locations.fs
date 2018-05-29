namespace EveAlod.Data

module Locations=

    let private security (system: IronSde.SolarSystem) = 
        match system.level with
        | IronSde.SecurityLevel.Lowsec -> Lowsec
        | IronSde.SecurityLevel.Nullsec -> Nullsec
        | IronSde.SecurityLevel.Wormhole -> Wormhole
        | IronSde.SecurityLevel.Highsec -> Highsec
        | IronSde.SecurityLevel.Abyssal -> Abyssal

    
    let private findCelestial solarSystemId position =
        IronSde.MapSearch.findClosestCelestial solarSystemId position 
            
    let region solarSystemId =
        solarSystemId   |> IronSde.SolarSystems.region 
                        |> (fun r -> { Region.Id = r.id; Name = r.name })

    let constellation solarSystemId =
        solarSystemId   |> IronSde.SolarSystems.constellation 
                        |> (fun c -> { Constellation.Id = c.id; Name = c.name })
    
    let toLocation (solarSystemId: int) (x: float) (y: float) (z: float) =
        solarSystemId 
            |> IronSde.SolarSystems.ofId 
            |> Option.map (fun sys -> 
                                        let system = { SolarSystem.Id = sys.id; Name = sys.name; 
                                                                    SecurityLevel = sys.security;
                                                                    Security = security sys}                        
                            
                                        let celestial,distance = IronSde.Position.ofCoordinates (x, y, z) 
                                                                    |> findCelestial solarSystemId 
                                                                    |> function
                                                                        | Some (c,d) -> Some c, Some d
                                                                        | _ -> None, None

                                        { Location.Region = region solarSystemId ; 
                                                        Constellation = constellation solarSystemId; 
                                                        SolarSystem = system; 
                                                        Celestial = celestial; 
                                                        Distance = distance } )
        