#r @"packages/FAKE/tools/FakeLib.dll"

open Fake
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
                            |> MSBuildRelease buildDir "Build"
                            |> Log "AppBuild-Output: ")

Target "BuildTests" (fun _ -> 
                            !! "src/**/*.Tests.fsproj"
                            |> MSBuildRelease buildTestsDir "Build"
                            |> Log "TestsBuild-Output: ")

Target "RunUnitTests" (fun _ -> 
                            !! (buildTestsDir @@ "*.Tests.dll")
                            |> xUnit2 (fun p ->
                                            { p with 
                                                ShadowCopy = false;
                                                HtmlOutputPath = Some (buildTestsDir @@ "TestResults.html")
                                            }
                                        )                            
                            )
                            

Target "Default" (fun _ -> trace "Done!" )


// Dependencies

"ScrubArtifacts" 
==> "BuildApp"
==> "BuildTests"
==> "RunUnitTests"
==> "Default"

RunTargetOrDefault "Default"