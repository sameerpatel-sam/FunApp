-- Add CorrectAnswer column to Questions table
ALTER TABLE Questions ADD COLUMN CorrectAnswer TEXT NULL;

-- Create IndividualScores table
CREATE TABLE IF NOT EXISTS IndividualScores (
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
