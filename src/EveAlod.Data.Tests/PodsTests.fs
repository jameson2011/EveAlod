namespace EveAlod.Data.Tests

open System
open System.Linq
open Xunit
open FSharp.Data
open FsCheck
open FsCheck.Xunit
open EveAlod.Common
open EveAlod.Common.Strings
open EveAlod.Common.Tests
open EveAlod.Data

module PodsTests=
    
    [<Theory>]
    [<InlineData(20160, ImplantSet.Crystal, ImplantGrade.HighGrade)>]
    [<InlineData(22110, ImplantSet.Crystal, ImplantGrade.MidGrade)>]
    [<InlineData(33924, ImplantSet.Crystal, ImplantGrade.LowGrade)>]
    [<InlineData(31962, ImplantSet.Talon, ImplantGrade.HighGrade)>]
    [<InlineData(32112, ImplantSet.Talon, ImplantGrade.LowGrade)>]
    let ImplantReturnsSet(id, set, grade) =
        let result = id 
                        |> IronSde.ItemTypes.itemType
                        |> Option.get
                        |> Pods.implantSet

        match result with
        | Some (s,g) -> Assert.Equal(set, s)
                        Assert.Equal(grade, g)
        | _ -> failwith "None returned"

    
    [<Theory>]
    [<InlineData(2082)>] // Geno CA-1
    [<InlineData(587)>] // Rifter
    let UnrecognisedImplantReturnsNone(id) =
        let result = id 
                        |> IronSde.ItemTypes.itemType
                        |> Option.get
                        |> Pods.implantSet
        
        match result with
        | None -> ignore 0
        | Some (s,g) -> failwith "Some returned"
        
    [<Fact>]
    let GradedImplantsReturnsPartiallyFullList()=
        let implants = Pods.implants
        
        let himidlow = [| ImplantGrade.HighGrade; ImplantGrade.MidGrade; ImplantGrade.LowGrade |]
        let himid = [| ImplantGrade.HighGrade; ImplantGrade.MidGrade; |]
        let hilow = [| ImplantGrade.HighGrade; ImplantGrade.LowGrade |]
        let midlow = [| ImplantGrade.MidGrade; ImplantGrade.LowGrade |]

        let expected = [| 
                            ImplantSet.Halo, himidlow;
                            ImplantSet.Talisman, himidlow;
                            ImplantSet.Talon, hilow;
                            ImplantSet.Spur, hilow;
                            ImplantSet.Crystal, himidlow;
                            ImplantSet.Grail, hilow;
                            ImplantSet.Centurion, midlow;
                            ImplantSet.Harvest, midlow;
                            ImplantSet.Jackal, hilow;
                            ImplantSet.Slave, himidlow;
                            ImplantSet.Snake, himidlow;
                            ImplantSet.Asklepian, himidlow;
                            ImplantSet.Virtue, midlow;
                            ImplantSet.Edge, midlow;
                            ImplantSet.Nomad, midlow;
                            ImplantSet.Ascendancy, himid;
                            |]

        let expectedPairs = expected 
                             |> Seq.collect (fun (s,gs) -> gs |> Seq.map (fun g -> s,g))
                             |> Array.ofSeq
                             
        let actuals = implants  |> Seq.map Pods.implantSet
                                |> Seq.mapSomes
                                |> Set.ofSeq

        let matches = expectedPairs
                                |> Seq.filter (fun t -> actuals |> Set.contains t)
                                |> Seq.sort
                                |> Array.ofSeq

        Assert.Equal(expectedPairs.Length, matches.Length)


    [<Fact>]
    let ImplantsReturnsNonEmptyList() =    
        let implants = Pods.implants 
                            |> Seq.sortBy (fun i -> i.name)
                            |> Array.ofSeq

        Assert.NotEqual(0, implants.Length)