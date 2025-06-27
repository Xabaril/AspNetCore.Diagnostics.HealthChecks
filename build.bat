@echo off
powershell.exe -ExecutionPolicy Bypass -File "C:\Toltek\Build\BuildWithoutPrefix.ps1" -Action "build" -SolutionPath "AspNetCore.Diagnostics.HealthChecks.sln"  -PackageFolder "C:\Toltek\TLibrary\Platform-v2"
echo.
echo Script execution finished. Press any key to exit...
pause >nul
