# Eve Awful Loss of the Day
_Please note that this is a work in progress. Not suitable for production use!_

EveALOD finds awful losses & corp kills and pushes to Discord. zKB Redisq (https://github.com/zKillboard/RedisQ) is polled for the latest kills, categorised and pushed to a designated Discord webhook.

## How it work?
Very simply, a handful of simple predicates are applied to each killmail. Each kill is tagged and scored accordingly: those that pass a configured threshold make it to Discord for analysis, amusement, troll, whatever.

## What rules are these?
Corp kill
Corp loss
Plex/Skill injector in hold
Awox
ECM in hold (because ECM is evil)
Expensive (>10bn ISK)
Pods - for Organic Mass Granulator services
_more to come_


## How do I run on Windows?
It's a standard console application:
```fsharp
dotnet evealod.dll
```
## How do I run it in the background?
Simply configure the above command line as a scheduled task, triggered to start on system boot up, and left alone.

## Implementation
F# (FTW!)
.Net Core 2 
VS 2017 or similar is needed.

## Builds
[![Build status](https://ci.appveyor.com/api/projects/status/cro5s0i3nectf4bs?svg=true)](https://ci.appveyor.com/project/jameson2011/evealod)
_Builds are work in progress!_
