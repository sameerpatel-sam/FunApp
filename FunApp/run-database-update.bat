@echo off
echo ================================================
echo Updating Database for Individual Scoring
echo ================================================
echo.
pause

cd FunApp
dotnet script UpdateDatabase.csx

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Database updated successfully!
    echo You can now start the app.
) else (
    echo.
    echo Update failed. Check the error above.
)

pause
