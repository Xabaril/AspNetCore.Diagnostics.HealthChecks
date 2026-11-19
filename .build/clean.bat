@echo off
pwsh -NoProfile -ExecutionPolicy Bypass -File "%~dp0build.ps1" clean %*
set EC=%errorlevel%
echo.
pause
exit /b %EC%
