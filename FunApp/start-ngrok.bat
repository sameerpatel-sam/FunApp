@echo off
echo ========================================
echo   FunApp - ngrok Launcher
echo ========================================
echo.

REM Check if ngrok authtoken argument provided
if "%~1"=="" (
    echo Starting with existing ngrok configuration...
    echo.
    echo If this is your first time, you need an authtoken!
    echo Get it from: https://dashboard.ngrok.com/get-started/your-authtoken
    echo Then run: start-ngrok.bat YOUR_TOKEN_HERE
    echo.
    timeout /t 3
    powershell -ExecutionPolicy Bypass -File "%~dp0run-ngrok.ps1"
) else (
    echo Starting with authtoken: %~1
    echo.
    powershell -ExecutionPolicy Bypass -File "%~dp0run-ngrok.ps1" -NgrokAuthtoken "%~1"
)

echo.
echo ========================================
echo   ngrok and FunApp are now running!
echo ========================================
echo.
echo Two windows should have opened:
echo   1. dotnet window - Your ASP.NET app
echo   2. ngrok window - Public tunnel
echo.
echo Your browser should open with the ngrok URL.
echo Share that URL with quiz participants!
echo.
echo Press any key to close this window...
pause >nul
