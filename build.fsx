#r @"packages/FAKE/tools/FakeLib.dll"
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
                            |> MSBuildRelease buildDir "Build"
                            |> Log "AppBuild-Output: ")

Target "LintApp" (fun _ ->
                            !! "src/**/*.fsproj"
                            |> Seq.iter (FSharpLint id)
                )

Target "BuildTests" (fun _ -> 
                            !! "tests/**/*.fsproj"
                            |> MSBuildRelease buildTestsDir "Build"
                            |> Log "TestsBuild-Output: ")

Target "RunUnitTests" (fun _ -> 
                            !! (buildTestsDir @@ "*.UnitTests.dll")
                            |> xUnit2 (fun p ->
                                            { p with 
                                                ShadowCopy = false;
                                                HtmlOutputPath = Some (buildTestsDir @@ "TestResults.html")
                                            }
                                        )                            
                            )

Target "RunIntegrationTests" (fun _ -> 
                            !! (buildTestsDir @@ "*.IntegrationTests.dll")
                            |> xUnit2 (fun p ->
                                            { p with 
                                                ShadowCopy = false;
                                                HtmlOutputPath = Some (buildTestsDir @@ "TestResults.html")
                                                NoAppDomain = false;
                                                MaxThreads = CollectionConcurrencyMode.MaxThreads 1;
                                            }
                                        )                            
                            )


Target "Default" (fun _ -> trace "Done!" )


// Dependencies

"ScrubArtifacts" 
//==> "LintApp"
==> "BuildApp"
==> "BuildTests"
//==> "RunUnitTests"
==> "Default"



RunTargetOrDefault "Default"