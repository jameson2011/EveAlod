@echo off
cls


SET TARGET="BuildRunUnitTests"

IF NOT [%1]==[] (set TARGET="%1")


".\packages\FAKE\tools\Fake.exe" "build.fsx" "target=%TARGET%"
