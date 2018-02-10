#r @"packages/FAKE/tools/FakeLib.dll"
#r @"packages/FSharpLint.Fake/tools/FSharpLint.Core.dll"
#r @"packages/FSharpLint.Fake/tools/FSharpLint.Fake.dll"

open Fake
open FSharpLint.Fake
open Fake.Testing.XUnit2

let buildDir = "./artifacts/"
let buildTestsDir = "./testartifacts/"

// Targets
Target "ScrubArtifacts" (fun _ -> CleanDirs [ buildDir;
                                              buildTestsDir
                                              ])

Target "BuildApp" (fun _ -> 
                            !! "src/**/*.fsproj"
                            -- "src/**/*.Tests.fsproj"
                            -- "src/**/*.IntegrationTests.fsproj"
                            |> MSBuildRelease buildDir "Build"
                            |> Log "AppBuild-Output: ")

Target "LintApp" (fun _ ->
                            !! "src/**/*.fsproj"
                            -- "src/**/*.Tests.fsproj"
                            -- "src/**/*.IntegrationTests.fsproj"
                            |> Seq.iter (FSharpLint 
                                            (fun o -> { o with FailBuildIfAnyWarnings = true }))
                )

Target "BuildUnitTests" (fun _ -> 
                            !! "src/**/*.Tests.fsproj"
                            |> MSBuildRelease buildTestsDir "Build"
                            |> Log "BuildUnitTests-Output: ")

Target "BuildIntTests" (fun _ -> 
                            !!  "src/**/*.IntegrationTests.fsproj"
                            |> MSBuildRelease buildTestsDir "Build"
                            |> Log "BuildIntTests-Output: ")

Target "UnitTests" (fun _ -> 
                            !! (buildTestsDir @@ "*.Tests.dll")
                            |> xUnit2 (fun p ->
                                            { p with 
                                                ShadowCopy = false;
                                                HtmlOutputPath = Some (buildTestsDir @@ "UnitTestResults.html")
                                            }
                                        )                            
                            )

Target "IntTests" (fun _ -> 
                            !! (buildTestsDir @@ "*.IntegrationTests.dll")
                            |> xUnit2 (fun p ->
                                            { p with 
                                                ShadowCopy = false;
                                                HtmlOutputPath = Some (buildTestsDir @@ "IntegrationTestResults.html")
                                            }
                                        )                            
                            )


Target "BuildRunUnitTests" (fun _ -> trace "Built and unit tests run" )
Target "All" (fun _ -> trace "Built and all tests run" )

// Dependencies

"LintApp"
==> "ScrubArtifacts" 
==> "BuildApp"


// RunUnitTests only needs tests built
"BuildUnitTests"    ==> "UnitTests"

// RunIntTests only needs tests built
"BuildIntTests"     ==> "IntTests"

// BuildRunUnitTests needs build, unit tests run
"BuildApp"          ==> "BuildRunUnitTests"
"BuildIntTests"     ==> "BuildRunUnitTests"
"UnitTests"      ==> "BuildRunUnitTests"

// "All" needs build, unit tests run, integration tests run
"BuildRunUnitTests" ==> "All"
"IntTests"          ==> "All"

RunTargetOrDefault "BuildRunUnitTests"
