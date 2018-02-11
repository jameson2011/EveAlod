namespace EveAlod.Valuation

open FSharp.Data

module Samples=

    [<Literal>]
    let ShipStatsSample = """
    {
	    "allTimeSum": 1020486,
	    "dangerRatio": 41,
	    "gangRatio": 86,
	    "groups": {
		    "25": {
			    "groupID": 25,
			    "shipsLost": 815748,
			    "pointsLost": 5953562,
			    "iskLost": 5741361236839,
			    "shipsDestroyed": 348081,
			    "pointsDestroyed": 1979554,
			    "iskDestroyed": 3394961342569
		    },
		    "351210": {
			    "groupID": 351210,
			    "shipsDestroyed": 1,
			    "pointsDestroyed": 1,
			    "iskDestroyed": 78610
		    }
	    },
	    "id": 587,
	    "iskDestroyed": 77249301008862,
	    "iskLost": 5741361236839,
	    "months": {
		    "200712": {
			    "year": 2007,
			    "month": 12,
			    "shipsLost": 1521,
			    "pointsLost": 7530,
			    "iskLost": 29328812640,
			    "shipsDestroyed": 1710,
			    "pointsDestroyed": 5223,
			    "iskDestroyed": 141314410404
		    },
		    "201802": {
			    "year": 2018,
			    "month": 2,
			    "shipsLost": 1198,
			    "pointsLost": 11070,
			    "iskLost": 10407186989,
			    "shipsDestroyed": 1274,
			    "pointsDestroyed": 6511,
			    "iskDestroyed": 96510014609
		    }
	    },
	    "nextTopRecalc": 1030690,
	    "pointsDestroyed": 3844391,
	    "pointsLost": 5953562,
	    "sequence": 46588544,
	    "shipsDestroyed": 1023324,
	    "shipsLost": 815748,
	    "soloKills": 144080,
	    "soloLosses": 421512,
	    "topAllTime": [
		    {
			    "type": "character",
			    "data": [
				    {
					    "kills": 3156,
					    "characterID": 94363754,
					    "characterName": "Sem Skord"
				    },
				    {
					    "kills": 2648,
					    "characterID": 375726214,
					    "characterName": "Yankunytjatjara"
				    },
				    {
					    "kills": 1708,
					    "characterID": 1026503468,
					    "characterName": "Melkor Valor"
				    },
				    {
					    "kills": 481,
					    "characterID": 1129326529,
					    "characterName": "Kirith Darkblade"
				    }
			    ]
		    },
		    {
			    "type": "corporation",
			    "data": [
				    {
					    "kills": 109957,
					    "corporationID": 1699307293,
					    "corporationName": "Red Federation",
					    "cticker": "R-FED"
				    },
				    {
					    "kills": 994,
					    "corporationID": 98125391,
					    "corporationName": "Justified Chaos",
					    "cticker": "JUSTK"
				    },
				    {
					    "kills": 987,
					    "corporationID": 1642689248,
					    "corporationName": "Noob Mercs",
					    "cticker": "PHAIL"
				    }
			    ]
		    },
		    {
			    "type": "alliance",
			    "data": [
				    {
					    "kills": 90294,
					    "allianceID": 99000652,
					    "allianceName": "RvB - BLUE Republic",
					    "aticker": "RVB-B"
				    },
				    {
					    "kills": 12316,
					    "allianceID": 99002217,
					    "allianceName": "The Devil's Tattoo",
					    "aticker": "R1DER"
				    },
				    {
					    "kills": 967,
					    "allianceID": 99001317,
					    "allianceName": "Banderlogs Alliance",
					    "aticker": ".RU"
				    }
			    ]
		    },
		    {
			    "type": "faction",
			    "data": [
				    {
					    "kills": 60196,
					    "factionID": 500002,
					    "factionName": "Minmatar Republic"
				    },
				    {
					    "kills": 31569,
					    "factionID": 500004,
					    "factionName": "Gallente Federation"
				    }
			    ]
		    },
		    {
			    "type": "ship",
			    "data": [
				    {
					    "kills": 1020486,
					    "shipTypeID": 587,
					    "shipName": "Rifter",
					    "groupID": "25",
					    "groupName": "Frigate"
				    }
			    ]
		    },
		    {
			    "type": "system",
			    "data": [
				    {
					    "kills": 43047,
					    "solarSystemID": 30000153,
					    "solarSystemName": "Poinen",
					    "sunTypeID": "3802",
					    "solarSystemSecurity": "0.6",
					    "systemColorCode": "#96F933",
					    "regionID": 10000002,
					    "regionName": "The Forge"
				    },				
				    {
					    "kills": 1792,
					    "solarSystemID": 30003086,
					    "solarSystemName": "Sahtogas",
					    "sunTypeID": "3802",
					    "solarSystemSecurity": "0.3",
					    "systemColorCode": "#F66301",
					    "regionID": 10000038,
					    "regionName": "The Bleak Lands"
				    }
			    ]
		    }
	    ],
	    "type": "shipTypeID",
	    "activepvp": {
		    "characters": {
			    "type": "Characters",
			    "count": 595
		    },
		    "corporations": {
			    "type": "Corporations",
			    "count": 317
		    },
		    "alliances": {
			    "type": "Alliances",
			    "count": 122
		    },
		    "systems": {
			    "type": "Systems",
			    "count": 324
		    },
		    "regions": {
			    "type": "Regions",
			    "count": 48
		    },
		    "kills": {
			    "type": "Total Kills",
			    "count": 1637
		    }
	    },
	    "info": null,
	    "topLists": [
		    {
			    "type": "character",
			    "title": "Top Characters",
			    "values": [
				    {
					    "kills": 58,
					    "characterID": 91606237,
					    "characterName": "h0tsauce 0nyaD0G",
					    "id": 91606237,
					    "typeID": null,
					    "name": "h0tsauce 0nyaD0G"
				    },
				    {
					    "kills": 29,
					    "characterID": 93629008,
					    "characterName": "darkone040",
					    "id": 93629008,
					    "typeID": null,
					    "name": "darkone040"
				    },				
				    {
					    "kills": 16,
					    "characterID": 94242608,
					    "characterName": "xxMACKxx",
					    "id": 94242608,
					    "typeID": null,
					    "name": "xxMACKxx"
				    }
			    ]
		    },
		    {
			    "type": "corporation",
			    "title": "Top Corporations",
			    "values": [
				    {
					    "kills": 82,
					    "corporationID": 423280073,
					    "corporationName": "Black Rebel Rifter Club",
					    "cticker": "R1FTA",
					    "id": 423280073,
					    "typeID": null,
					    "name": "Black Rebel Rifter Club"
				    },				
				    {
					    "kills": 19,
					    "corporationID": 98547192,
					    "corporationName": "Wolf Pack of Dal",
					    "cticker": "WPOFD",
					    "id": 98547192,
					    "typeID": null,
					    "name": "Wolf Pack of Dal"
				    }
			    ]
		    },
		    {
			    "type": "alliance",
			    "title": "Top Alliances",
			    "values": [
				    {
					    "kills": 82,
					    "allianceID": 99002217,
					    "allianceName": "The Devil's Tattoo",
					    "aticker": "R1DER",
					    "id": 99002217,
					    "typeID": null,
					    "name": "The Devil's Tattoo"
				    },
				    {
					    "kills": 74,
					    "allianceID": 99004295,
					    "allianceName": "A Band Apart.",
					    "aticker": "-ABA-",
					    "id": 99004295,
					    "typeID": null,
					    "name": "A Band Apart."
				    }
			    ]
		    },
		    {
			    "type": "shipType",
			    "title": "Top Ships",
			    "values": [
				    {
					    "kills": 862,
					    "shipTypeID": 587,
					    "shipName": "Rifter",
					    "groupID": "25",
					    "groupName": "Frigate",
					    "id": 587,
					    "typeID": null,
					    "name": "Rifter"
				    }
			    ]
		    },
		    {
			    "type": "solarSystem",
			    "title": "Top Systems",
			    "values": [
				    {
					    "kills": 37,
					    "solarSystemID": 30002756,
					    "solarSystemName": "Ishomilken",
					    "sunTypeID": "3802",
					    "solarSystemSecurity": "0.4",
					    "systemColorCode": "#E58000",
					    "regionID": 10000033,
					    "regionName": "The Citadel",
					    "id": 30002756,
					    "typeID": null,
					    "name": "Ishomilken"
				    },
				    {
					    "kills": 15,
					    "solarSystemID": 30045354,
					    "solarSystemName": "Reitsato",
					    "sunTypeID": "3802",
					    "solarSystemSecurity": "0.2",
					    "systemColorCode": "#EB4903",
					    "regionID": 10000069,
					    "regionName": "Black Rise",
					    "id": 30045354,
					    "typeID": null,
					    "name": "Reitsato"
				    }
			    ]
		    },
		    {
			    "type": "location",
			    "title": "Top Locations",
			    "values": [
				    {
					    "kills": 21,
					    "locationID": 50008454,
					    "itemName": "Stargate (C4C-Z4)",
					    "locationName": "Stargate (C4C-Z4)",
					    "typeID": "3874",
					    "id": 50008454,
					    "name": "Stargate (C4C-Z4)"
				    },
				    {
					    "kills": 7,
					    "locationID": 50008253,
					    "itemName": "Stargate (VRH-H7)",
					    "locationName": "Stargate (VRH-H7)",
					    "typeID": "3875",
					    "id": 50008253,
					    "name": "Stargate (VRH-H7)"
				    }
			    ]
		    }
	    ],
	    "topIskKillIDs": [ 67868220, 67913390, 67891601, 67913385, 67836086, 67943201 ]
    }
    """

type ShipStatisticsProvider = JsonProvider<Samples.ShipStatsSample>
