# Eve Awful Loss of the Day
_Please note that this is a work in progress. Not suitable for production use!_

EveALOD finds awful losses & corp kills and pushes to Discord for your amusement and delectation. 
Now bragging is automatic!

## How does it work?
Very simply, zKB Redisq (https://github.com/zKillboard/RedisQ) is polled for the latest kills, categorised and pushed to a designated Discord webhook.

The categorisation is a handful of predicates, applied to each killmail. Each kill is tagged and scored accordingly: those over a threshold make it to Discord for analysis, amusement, trolling, whatever suits your fancy.

## What rules are these?
⋅⋅* Corp kill: _Because who doesn't like good news?_
⋅⋅* Corp loss: _CEOs need reasons to be angry_
⋅⋅* Plex/Skill injector in hold: _Because this never gets old_
⋅⋅* Awox: _required for a long running scientific study on just how hard API checks can be_
⋅⋅* ECM in hold: _because ECM is evil_
⋅⋅* Expensive (>10bn ISK): _no reason needed_
⋅⋅* Pods: _because the Pod God needs his sacrifice_

_more to come!_

## Why doesn't it work for alliances?
This is by design.

## How do I run on Windows?
It's a standard console application:
```fsharp
dotnet evealod.dll
```

## How do I run it in the background?
Simply configure the above command line as a scheduled task, triggered to start on system boot up, and left alone.

## Logging & Configuration
_Work in progress!_

## Implementation
F# (FTW!)
.Net Core 2 
VS 2017 or similar is needed.

## Builds
[![Build status](https://ci.appveyor.com/api/projects/status/cro5s0i3nectf4bs?svg=true)](https://ci.appveyor.com/project/jameson2011/evealod)
_Builds are work in progress!_
