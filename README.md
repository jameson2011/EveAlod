# Eve Awful Loss of the Day

EveALOD finds awful losses & corp kills and pushes to Discord for your amusement and delectation.
Now bragging is automatic!

## How does it work?
Very simply, zKB Redisq (https://github.com/zKillboard/RedisQ) is polled for the latest kills, categorised and pushed to a designated Discord webhook.

The categorisation is a handful of predicates, applied to each killmail. Each kill is tagged and scored accordingly: those over a threshold make it to Discord for analysis, amusement, trolling, whatever suits your fancy.

## What rules are these?
  * Corp kill: _Because who doesn't like good news?_
  * Corp loss: _CEOs need reasons to be angry_
  * Plex/Skill injector in hold: _Because this never gets old_
  * Awox: _Required for a long running scientific study on just how hard API checks can be_
  * ECM in hold: _Because ECM is evil_
  * Expensive (>10bn ISK): _No reason needed_
  * Pods: _Because the Pod God needs his sacrifice_
  * Solo: _Medals all round_
  * NPC losses: _Losing to a gate camp is one thing. Losing to a rat is quite another_


## Why doesn't it work for alliances?
That's in the backlog.

## How do I run on Windows?
It's a standard console application:

```
evealod.exe run
```

or

```
evealod.exe -?
```

## How do I run it in the background?
Simply configure the above command line as a scheduled task, triggered to start on system boot up, and left alone.

## Logging & Configuration
Logging is to local files, as per log4net, under the `\logs` folder.

Configuration is applied by a `settings.json` file in the exexcutables' folder:

```
{
  "minimumScore": 40.0,
  "corpTicker": "R1FTA",
  "discordWebhookUri": "https://discordapp.com/api/webhooks/xxxx",
  "killSourceUri": "https://redisq.zkillboard.com/listen.php?ttw=10"
}
```

where:

* `minimumScore`: the minimum score for all kills. 40 will show corp kills & losses, and kills over 1bn ISK. A threshold of 50 will show corp kills & losses, and losses over 10bn ISK.
* `corpTicker`: your corp's corpTicker.
* `discordWebhookUri`: the Discord webhook URI.
* `killSourceUri`: optional; provide a proxy's URI, otherwise zKB's RedisQ service will be used.

## Implementation
  * F#
  * .Net Framework 4.6.1
  * VS 2017 or similar is needed.

## Credits

Contact me (Jameson2011) in game for questions or enhancement requests.

EveALOD is released under the MIT license (`LICENSE`).
Data is sourced from https://zkillboard.com/  and EVE-Online, covered by a separate license from [CCP](http://www.ccpgames.com/en/home). You can see the full license in the `CCP.MD` file.
EveALOD uses various 3rd party libraries carrying their own licensing. Please refer to each individually for more information.

## Feature backlog

* Move away from predicates: while simple enough to start with, this method simply doesn't scale
* Multiple corps & alliances
* Improve the tag lines
* _...& more_

## Builds
Builds are available from AppVeyor:
[![Build status](https://ci.appveyor.com/api/projects/status/cro5s0i3nectf4bs?svg=true)](https://ci.appveyor.com/project/jameson2011/evealod)
