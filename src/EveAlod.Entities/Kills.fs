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

    type Character={
        Char: Entity;
        Corp: Entity option;
        Alliance: Entity option;
        }

    type Attacker = {
        Char: Character option;
        Ship: Entity;
        Damage: int
        }

    type Kill = {   Id: string; 
                    Occurred: DateTime;
                    ZkbUri: string;
                    Location: Entity option;

                    Victim: Character option;
                    VictimShip: Entity option;
                    TotalValue: float;

                    Attackers: Attacker list;

                    Tags: KillTag list }

