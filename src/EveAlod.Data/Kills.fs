namespace EveAlod.Data

    open System

    type EntityGroupKey =
        | Plex
        | SkillInjector
        | Ecm
        | Capsule
        | Ship

    type KillTag =
        | AwoxKill
        | SoloKill
        | NpcKill
        | PlayerKill
        | CorpKill
        | CorpLoss
        | PlexInHold
        | SkillInjectorInHold
        | EcmFitted
        | FailFit
        | Pod
        | Spendy
        | Expensive
        | Cheap
        | ZeroValue
        | WideMarginShipType
        | NarrowMarginShipType

    
    type SpaceSecurity = 
        | Lowsec
        | Nullsec
        | Wormhole
        | Highsec

    type SolarSystem=
        {
            Id: int;
            Name: string;
            SecurityLevel: float;
            Security: SpaceSecurity;
        }

    type ItemLocation =
        | Unknown
        | NoLocation // None
        | Hangar
        | CargoHold
        | HighSlot
        | MidSlot
        | LowSlot
        | FixedSlot
        | Capsule
        | Pilot
        | RigSlot
        | Subsystem
        | Implant
        | DroneBay
        | ShipHangar
        | ShipHold
        | SecondaryStorage
        | FleetHangar
        | FighterBay
        | FighterTube
        | SubsystemBay
    
    
    type Region = { Id: int; Name: string; }
    type Constellation = { Id: int; Name: string; }
    type Celestial = { Id: int; Name: string; }

    type Location = {
            Celestial: Celestial option;
            SolarSystem: SolarSystem;
            Constellation: Constellation;
            Region: Region;
        }

    

    type Entity = {
                    Id: string;
                    Name: string;
                    }

    type EntityGroup = {
                    Id: string;
                    Name: string;
                    EntityIds: string []
                    }

    type CargoItem = {
                        Item: Entity;
                        Quantity: int;
                        Location: ItemLocation;
                    }

    type Character={
        Char: Entity;
        Corp: Entity option;
        Alliance: Entity option;
        }

    type Attacker = {
        Char: Character option;
        Ship: Entity option;
        Damage: int
        }

    type Kill = {   Id: string; 
                    Occurred: DateTime;
                    ZkbUri: string;
                    Location: Location option;

                    Victim: Character option;
                    VictimShip: Entity option;
                    Fittings: CargoItem list;
                    Cargo: CargoItem list;
                    Attackers: Attacker list;

                    Tags: KillTag list 
                    FittedValue: float;
                    TotalValue: float;
                    
                    AlodScore: float;
                    TotalValueValuation: float option
                    TotalValueSpread: float option
                    }
        with static member empty =
                { 
                    Kill.Id = "";
                    Occurred = System.DateTime.UtcNow;
                    Tags = [];
                    ZkbUri = "";
                    Location = None;
                    Victim = None;
                    VictimShip = None;
                    AlodScore = 0.;
                    Attackers = [];
                    Fittings = [];
                    Cargo = [];
                    FittedValue = 0.;
                    TotalValue = 0.;
                    TotalValueValuation = None;
                    TotalValueSpread = None;
                }            

