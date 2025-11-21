using Microsoft.Data.Sqlite;

Console.WriteLine("================================================");
Console.WriteLine("Updating Database for Individual Scoring");
Console.WriteLine("================================================");
Console.WriteLine();

var dbPath = "../quiz.db";
if (!File.Exists(dbPath))
{
    Console.WriteLine("ERROR: quiz.db not found!");
    Console.WriteLine($"Looking for: {Path.GetFullPath(dbPath)}");
    Console.WriteLine("Make sure quiz.db exists in the FunApp folder.");
    Console.WriteLine();
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
    return 1;
}

var connectionString = $"Data Source={dbPath}";

try
{
    using var connection = new SqliteConnection(connectionString);
    connection.Open();
    Console.WriteLine("? Connected to database");
    Console.WriteLine();

    // Add CorrectAnswer column
    try
    {
        var cmd1 = connection.CreateCommand();
        cmd1.CommandText = "ALTER TABLE Questions ADD COLUMN CorrectAnswer TEXT NULL;";
        cmd1.ExecuteNonQuery();
        Console.WriteLine("? Added CorrectAnswer column to Questions table");
    }
    catch (SqliteException ex) when (ex.Message.Contains("duplicate column"))
    {
        Console.WriteLine("? CorrectAnswer column already exists");
    }

    // Create IndividualScores table
    try
    {
        var cmd2 = connection.CreateCommand();
        cmd2.CommandText = @"
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
            );";
        cmd2.ExecuteNonQuery();
        Console.WriteLine("? Created IndividualScores table");
    }
    catch (SqliteException ex) when (ex.Message.Contains("already exists"))
    {
        Console.WriteLine("? IndividualScores table already exists");
    }

    Console.WriteLine();
    Console.WriteLine("================================================");
    Console.WriteLine("SUCCESS! Database updated successfully!");
    Console.WriteLine("================================================");
    Console.WriteLine();
    Console.WriteLine("You can now start the app with: dotnet run");
    Console.WriteLine();
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR: {ex.Message}");
    Console.WriteLine();
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
    return 1;
}
