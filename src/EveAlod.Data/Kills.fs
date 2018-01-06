﻿namespace EveAlod.Data

    open System

    type EntityGroupKey =
        | Plex
        | SkillInjector
        | Ecm
        | Capsule

    type KillTag =
        | Awox
        | Solo
        | Npc
        | CorpKill
        | CorpLoss
        | PlexInHold
        | SkillInjectorInHold
        | Ecm
        | FailFit
        | Pod
        | Spendy
        | Expensive
        | Cheap

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
                    Location: Entity option;

                    Victim: Character option;
                    VictimShip: Entity option;
                    Cargo: CargoItem list;
                    TotalValue: float;

                    Attackers: Attacker list;

                    Tags: KillTag list 

                    AlodScore: float;
                    }
