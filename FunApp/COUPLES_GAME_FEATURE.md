# Couples Game Mode - Feature Documentation

## Overview
The Couples Game Mode allows pairs of players with matching last names to play together, with scoring based on answer matching.

## Features Implemented

### 1. Automatic Couple Pairing
- **Players with the same last name are automatically paired as couples**
- Name parsing: "John Smith" ? First Name: "John", Last Name: "Smith"
- Works with any format: "FirstName LastName"

### 2. Answer Matching & Scoring
- When both partners submit answers for a question, the system compares them
- **Matching answers** (case-insensitive): +1 point to the couple
- **Non-matching answers**: 0 points
- Scores are tracked per couple, not individual players

### 3. Results Display Format
- Couples displayed as: **"Mr & Mrs {LastName}"**
- Score shown prominently in results
- All answer pairs shown with match indicators (? for matches)

### 4. Database Persistence
All couple game transactions are saved to the database:
- `CoupleScore` table tracks:
  - Quiz session ID
  - Last name
  - Question ID
  - Whether answers matched
  - Points awarded
  - Both partners' answers
  - Timestamp

## How to Use

### For Participants (Couple Mode)

1. **Join with Full Name**
   ```
   Example: "John Smith" and "Jane Smith"
   ```
   - Both partners must include the same last name
   - System automatically detects the couple

2. **Answer Questions**
   - Both partners submit their answers independently
   - System evaluates when both have answered

3. **View Results**
   - Couples see: "Mr & Mrs Smith - Score"
   - Individual answer pairs with match indicators

### For Host

1. **Select Couple Mode**
   - Click "Couple Players" button on main screen
   - Mode indicator shows current game type

2. **Add Couple Questions**
   - Go to Admin panel
   - Add questions under "Couple Game Questions"

3. **During Quiz**
   - Click "Next Question" to advance
   - System automatically evaluates couple answers
   - Scores calculated and persisted

4. **View Results**
   - Click "Show Results" to see couple scores
   - Export to CSV for records

## Database Schema

### New Models

#### User (Updated)
```csharp
- ConnectionId
- Name (full name)
- FirstName (parsed)
- LastName (parsed)
- SwitchCount
- Score (new)
```

#### CoupleScore (New)
```csharp
- Id
- QuizSessionId
- LastName
- QuestionId
- AnswersMatched (bool)
- PointsAwarded (int)
- Partner1Answer
- Partner2Answer
- CreatedAt
```

#### CoupleResult (New - In-Memory)
```csharp
- LastName
- TotalScore
- Partner1Answers (list)
- Partner2Answers (list)
- MatchedAnswers (count)
```

## API Methods

### QuizService (New/Updated)

```csharp
// Get all couples grouped by last name
Dictionary<string, List<User>> GetCouplesByLastName()

// Evaluate answers and award points
List<(string lastName, string partner1Answer, string partner2Answer, bool matched)> EvaluateCoupleAnswers()

// Get final couple results
Dictionary<string, CoupleResult> GetCoupleResults()

// Get specific couple's score
int GetCoupleScore(string lastName)
```

### PersistentQuizService (New)

```csharp
// Save couple score to database
Task<CoupleScore> SaveCoupleScoreAsync(...)

// Get all couple scores for a session
Task<List<CoupleScore>> GetCoupleScoresForSessionAsync(int sessionId)

// Get total scores by couple
Task<Dictionary<string, int>> GetCoupleTotalScoresAsync(int sessionId)
```

### QuizHub (Updated)

```csharp
// Join with name parsing
Task JoinQuiz(string userName)

// Submit with couple evaluation trigger
Task SubmitAnswer(string answer)

// Next question with couple scoring
Task NextQuestion()

// Get results (couple or individual mode)
Task<List<dynamic>> GetAllUserAnswers()
```

## SignalR Events (New)

### Server ? Client

```javascript
// Triggered when a couple is formed
connection.on("CoupleFormed", (data) => {
    // data: { LastName: "Smith" }
});

// Triggered when both partners have answered
connection.on("CoupleAnswered", (data) => {
    // data: { LastName: "Smith" }
});
```

## Example Usage Flow

### Scenario: John and Jane Smith Playing

1. **Join Phase**
   ```
   John joins: "John Smith" ?
   Jane joins: "Jane Smith" ?
   ? System: Couple "Smith" formed
   ```

2. **Question 1: "What's your favorite color?"**
   ```
   John answers: "Blue"
   Jane answers: "Blue"
   ? Match! ? +1 point to Mr & Mrs Smith
   ```

3. **Question 2: "What's your favorite food?"**
   ```
   John answers: "Pizza"
   Jane answers: "Pasta"
   ? No match ? 0 points
   ```

4. **Results Display**
   ```
   Mr & Mrs Smith - 1
   Q1: Partner 1: Blue | Partner 2: Blue ?
   Q2: Partner 1: Pizza | Partner 2: Pasta
   ```

## Technical Details

### Name Parsing Logic
```csharp
"John Smith" ? FirstName: "John", LastName: "Smith"
"Mary Jane Watson" ? FirstName: "Mary", LastName: "Watson"
"Prince" ? FirstName: "Prince", LastName: "" (no couple matching)
```

### Answer Matching Logic
- **Case-insensitive**: "Blue" matches "blue" matches "BLUE"
- **Whitespace trimmed**: " Pizza " matches "Pizza"
- **Exact match**: "Pizza" ? "Pizzas"

### Scoring Persistence
Every question evaluation is saved to database:
```sql
INSERT INTO CoupleScores 
VALUES (sessionId, lastName, questionId, matched, points, answer1, answer2, timestamp)
```

## Testing

### Test Couples Mode

1. Start app and select "Couple Players"
2. Join with two users:
   - User 1: "John Test"
   - User 2: "Jane Test"
3. Verify couple formed in logs
4. Ask a question
5. Have both submit the same answer
6. Click "Next Question" to evaluate
7. Check logs for "MATCHED" message
8. View results to see "Mr & Mrs Test - 1"

### Database Verification

```sql
-- View all couple scores
SELECT * FROM CoupleScores;

-- View couple totals
SELECT LastName, SUM(PointsAwarded) as TotalScore
FROM CoupleScores
WHERE QuizSessionId = {sessionId}
GROUP BY LastName;
```

## Migration Notes

**?? Database Schema Change**

The app will automatically recreate the database on first run to add the new `CoupleScores` table. This will:
- ? Add new CoupleScores table
- ? Update User model (added FirstName, LastName, Score)
- ?? **Delete existing data** (questions, sessions, responses)

**Before Running:**
1. Backup your `quiz.db` if you have important data
2. Or manually migrate using EF Core migrations

## Future Enhancements

Potential additions:
- [ ] Real-time score display during quiz
- [ ] Leaderboard for couples
- [ ] Different point values for questions
- [ ] Bonus points for consecutive matches
- [ ] Team mode (groups > 2 people)
- [ ] Historical couple performance tracking

## Troubleshooting

### Couple Not Pairing
- **Issue**: Players with same last name not recognized as couple
- **Check**: Both names have exactly matching last names (case-insensitive)
- **Example**: "John SMITH" and "Jane Smith" will pair ?

### Scores Not Updating
- **Issue**: Points not awarded despite matching answers
- **Check**: 
  - Both partners submitted answers
  - Clicked "Next Question" to trigger evaluation
  - Check database logs for evaluation messages

### Results Not Showing
- **Issue**: Couple results not appearing
- **Check**:
  - In Couple mode (not Individual)
  - At least one question answered by both partners
  - Database has CoupleScores entries

## Support

For issues or questions:
1. Check application logs for error messages
2. Verify database schema with: `SELECT * FROM sqlite_master WHERE type='table';`
3. Review SignalR connection logs in browser console

---

**Couples Game Mode** - Making quiz night fun for pairs! ??
