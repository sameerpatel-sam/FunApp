@echo off
echo ================================================
echo Individual Scoring Feature - Manual Database Update
echo ================================================
echo.
echo This will manually add the required columns and tables
echo to your existing database.
echo.
echo IMPORTANT: Make sure the app is STOPPED before running this!
echo Press Ctrl+C to cancel, or
pause

echo.
echo Applying database changes...

cd FunApp

:: Check if SQLite command-line tool is available
where sqlite3 >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo Using sqlite3 command-line tool...
    sqlite3 quiz.db < add-scoring-columns.sql
    if %ERRORLEVEL% EQU 0 (
        echo SUCCESS! Database updated.
    ) else (
        echo ERROR: Failed to update database with sqlite3.
        echo Trying alternative method...
        goto DOTNET_METHOD
    )
) else (
    echo sqlite3 not found, using dotnet tool...
    goto DOTNET_METHOD
)

goto END

:DOTNET_METHOD
echo.
echo Installing dotnet-ef tool if not present...
dotnet tool install --global dotnet-ef 2>nul

echo.
echo Creating and applying migration...
dotnet ef migrations add AddIndividualScoring --force
dotnet ef database update --force

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Migration failed!
    echo.
    echo Please try the manual SQL approach:
    echo 1. Download DB Browser for SQLite from: https://sqlitebrowser.org/
    echo 2. Open quiz.db file
    echo 3. Execute the SQL in add-scoring-columns.sql
    pause
    exit /b 1
)

:END
echo.
echo ================================================
echo SUCCESS! Database updated successfully!
echo ================================================
echo.
echo You can now start the app with:
echo   dotnet run
echo.
pause
