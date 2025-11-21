@echo off
echo ================================================
echo Individual Scoring Feature - Database Migration
echo ================================================
echo.
echo This script will:
echo 1. Create database migration
echo 2. Update the database schema
echo.
echo IMPORTANT: Make sure the app is STOPPED before running this!
echo Press Ctrl+C to cancel, or
pause

echo.
echo Creating migration...
cd FunApp
dotnet ef migrations add AddIndividualScoring

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Migration creation failed!
    echo Make sure the app is stopped and try again.
    pause
    exit /b 1
)

echo.
echo Applying migration to database...
dotnet ef database update

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Database update failed!
    pause
    exit /b 1
)

echo.
echo ================================================
echo SUCCESS! Database updated successfully!
echo ================================================
echo.
echo You can now start the app with:
echo   dotnet run --project FunApp/FunApp.csproj
echo.
echo Or simply run: start-app.bat
echo.
pause
