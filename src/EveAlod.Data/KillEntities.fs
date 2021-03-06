﻿namespace EveAlod.Data

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
    | AboveNormalPrice
    | NormalPrice
    | Cheap
    | ZeroValue
    | WideMarginShipType
    | NarrowMarginShipType
    | Lowsec
    | Highsec
    | Nullsec
    | Wormhole
    | Abyssal
    | Gatecamp
    | MissingLows
    | MissingMids
    | NoRigs
    | MixedTank
    | Stabbed
    | Industrial
        
type SpaceSecurity = 
    | Lowsec
    | Nullsec
    | Wormhole
    | Highsec
    | Abyssal
    
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
    
type ImplantGrade =
    | HighGrade = 1
    | MidGrade = 2
    | LowGrade = 3

type ImplantSet =
    | Halo = 1
    | Talisman = 2
    | Talon = 3
    | Christmas = 4
    | Spur = 5
    | Crystal = 6
    | Grail= 7 
    | Centurion = 8
    | Harvest = 9
    | Jackal = 10
    | Slave = 11
    | Snake = 12
    | Asklepian = 13
    | Virtue = 14
    | Edge = 15
    | Nomad = 16
    | Ascendancy = 17

type SolarSystem=
    {
        Id: int;
        Name: string;
        SecurityLevel: float;
        Security: SpaceSecurity;
    }
type Region = { Id: int; Name: string; }
type Constellation = { Id: int; Name: string; }
    
type Location = {
        Celestial: IronSde.Celestial option;
        Distance: float<IronSde.m> option;
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

