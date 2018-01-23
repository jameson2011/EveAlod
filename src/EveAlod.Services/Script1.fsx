#r "..\\..\\artifacts\\EveAlod.Common.dll"
#r "..\\..\\artifacts\\EveAlod.Data.dll"
#r "..\\..\\artifacts\\EveAlod.Services.dll"

open EveAlod.Services

let p = new EveAlod.Services.StaticEntityProvider() :> IStaticEntityProvider

// thera
p.SolarSystem "31000005" |> Async.RunSynchronously
// jita
p.SolarSystem "30000142" |> Async.RunSynchronously

// OMS
p.SolarSystem "30005000" |> Async.RunSynchronously

// Poitot
p.SolarSystem "30003271" |> Async.RunSynchronously
