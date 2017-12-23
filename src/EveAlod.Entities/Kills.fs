namespace EveAlod.Entities

    open System

    type KillTag =
        | Awox of string
        | Solo of string
        | Npc of string
        | PlexInHold of string
        | SkillInjectorInHold of string

    type Entity = {
                    Id: string;
                    Name: string;
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

                    Tags: KillTag list }

