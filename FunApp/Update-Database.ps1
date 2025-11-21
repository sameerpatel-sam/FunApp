Write-Host "================================================"
Write-Host "Updating Database for Individual Scoring"
Write-Host "================================================"
Write-Host ""
Write-Host "Checking database..."

$dbPath = "quiz.db"

if (!(Test-Path $dbPath)) {
    Write-Host "ERROR: quiz.db not found in current directory!"
    Write-Host "Make sure you're running this from the FunApp folder."
    pause
    exit 1
}

# Use .NET to load SQLite and update database
Add-Type -Path "C:\Users\samee\.nuget\packages\microsoft.data.sqlite.core\8.0.11\lib\net8.0\Microsoft.Data.Sqlite.dll"

try {
    $connectionString = "Data Source=$dbPath"
    $connection = New-Object Microsoft.Data.Sqlite.SqliteConnection($connectionString)
    $connection.Open()
    
    Write-Host "Connected to database successfully!"
    Write-Host ""
    
    # Check if CorrectAnswer column exists
    $checkCmd = $connection.CreateCommand()
    $checkCmd.CommandText = "PRAGMA table_info(Questions);"
    $reader = $checkCmd.ExecuteReader()
    
    $hasCorrectAnswer = $false
    while ($reader.Read()) {
        if ($reader.GetString(1) -eq "CorrectAnswer") {
            $hasCorrectAnswer = $true
            break
        }
    }
    $reader.Close()
    
    if (!$hasCorrectAnswer) {
        Write-Host "Adding CorrectAnswer column to Questions table..."
        $alterCmd = $connection.CreateCommand()
        $alterCmd.CommandText = "ALTER TABLE Questions ADD COLUMN CorrectAnswer TEXT NULL;"
        $alterCmd.ExecuteNonQuery() | Out-Null
        Write-Host "? CorrectAnswer column added successfully!"
    } else {
        Write-Host "? CorrectAnswer column already exists"
    }
    
    Write-Host ""
    
    # Check if IndividualScores table exists
    $checkTableCmd = $connection.CreateCommand()
    $checkTableCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='IndividualScores';"
    $tableExists = $checkTableCmd.ExecuteScalar()
    
    if (!$tableExists) {
        Write-Host "Creating IndividualScores table..."
        $createCmd = $connection.CreateCommand()
        $createCmd.CommandText = @"
CREATE TABLE IndividualScores (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    QuizSessionId INTEGER NOT NULL,
    ParticipantName TEXT NOT NULL,
    QuestionId INTEGER NOT NULL,
    UserAnswer TEXT NOT NULL,
    CorrectAnswer TEXT NOT NULL,
    IsCorrect INTEGER NOT NULL,
    PointsAwarded INTEGER NOT NULL,
    CreatedAt TEXT NOT NULL,
    FOREIGN KEY (QuizSessionId) REFERENCES QuizSessions(Id) ON DELETE CASCADE,
    FOREIGN KEY (QuestionId) REFERENCES Questions(Id) ON DELETE CASCADE
);
"@
        $createCmd.ExecuteNonQuery() | Out-Null
        Write-Host "? IndividualScores table created successfully!"
    } else {
        Write-Host "? IndividualScores table already exists"
    }
    
    $connection.Close()
    
    Write-Host ""
    Write-Host "================================================"
    Write-Host "SUCCESS! Database updated successfully!"
    Write-Host "================================================"
    Write-Host ""
    Write-Host "You can now start the app with:"
    Write-Host "  dotnet run"
    Write-Host ""
    
} catch {
    Write-Host "ERROR: $_"
    Write-Host $_.Exception
    pause
    exit 1
}

pause
