# Real-Time Diagnostic - Couple Score Issue

## Your Current Results

```
Mr & Mrs ccc
0                                    ? Should show actual score
Total Score
Q1: Partner 1: 5 | Partner 2: 5 ?   ? Should show ? (both answered same)
Q2: Partner 1: 5 | Partner 2: 3     ? Correct (no match)
Q3: Partner 1: 3 | Partner 2: Xl    ? Correct (no match, case-insensitive)
Q4: Partner 1: xl | Partner 2: N/A  ? Partner 2 didn't answer Q4
```

## Analysis

### Expected Score: 1
- Q1: "5" vs "5" ? **MATCH** ? ? 1 point
- Q2: "5" vs "3" ? No match ? 0 points
- Q3: "3" vs "Xl" ? No match ? 0 points
- Q4: "xl" vs "N/A" ? No match (partner didn't answer) ? 0 points

**Total Expected: 1 point**

### Why Score Shows 0

The issue is that the score is being pulled from the **database**, but the database uses a **case-insensitive dictionary lookup**. Let me check if there's a mismatch.

## The Real Problem

Looking at your output, the core issue is:

1. **Database Lookup is Working** (case-insensitive dictionary is in place)
2. **Scores ARE Being Saved** (database was modified at 8:38 PM)
3. **But Display Shows 0** 

This means either:
- ? The database query is returning 0 scores
- ? The session ID mismatch
- ? The couple name in DB doesn't match display name

## Diagnostic Steps

### Step 1: Check What's in the Database

Open a new PowerShell window and run:

```powershell
cd C:\Users\samee\source\repos\FunApp\FunApp

# Install sqlite3 if you don't have it
# Download from: https://www.sqlite.org/download.html
# Or use: winget install SQLite.SQLite

# Query the database
sqlite3 quiz.db "SELECT * FROM QuizSessions ORDER BY Id DESC LIMIT 1;"
sqlite3 quiz.db "SELECT * FROM CoupleScores WHERE LastName LIKE '%ccc%';"
```

Expected output:
```
SessionId | LastName | QuestionId | AnswersMatched | PointsAwarded
1         | ccc      | 1          | 1              | 1
1         | ccc      | 2          | 0              | 0
1         | ccc      | 3          | 0              | 0
1         | ccc      | 4          | 0              | 0
```

### Step 2: Check the Logs

In the PowerShell window where `dotnet run` is running, look for these messages when you click "Show Results":

**Expected logs:**
```
[DB QUERY] GetCoupleTotalScores(1): Found 4 total score entries
[DB QUERY]   - LastName='ccc', QuestionId=1, PointsAwarded=1
[DB QUERY]   - LastName='ccc', QuestionId=2, PointsAwarded=0
[DB QUERY]   - LastName='ccc', QuestionId=3, PointsAwarded=0
[DB QUERY]   - LastName='ccc', QuestionId=4, PointsAwarded=0
[DB QUERY] Grouped into 1 couples:
[DB QUERY]   - 'ccc': 1 points
[DB QUERY] Returning 1 couple totals:
[DB QUERY]   - 'ccc': 1 points
info: Couple 'ccc': DB Score=1, Calculated Score=1, Final Score=1
```

**If you see:**
```
[DB QUERY] GetCoupleTotalScores(1): Found 0 total score entries
```

This means the session ID is wrong or scores weren't saved.

### Step 3: Verify Case Matching

The database stores LastName as entered (e.g., "ccc", "Ccc", "CCC"). Make sure when you joined:
- Both players used **exactly** "ccc" as last name
- Not "Ccc" or "CCC" (though case-insensitive lookup should handle this)

## Most Likely Issues

### Issue 1: Multiple Sessions

Each time you restart the app or change game mode, a new session is created. If you have old data:

```sql
-- Check all sessions
SELECT * FROM QuizSessions ORDER BY Id DESC;

-- Check which session has your couple scores
SELECT SessionId, COUNT(*) as ScoreCount FROM CoupleScores GROUP BY SessionId;
```

**Solution:** Delete old database and start fresh:
```powershell
cd C:\Users\samee\source\repos\FunApp\FunApp
# Stop app (Ctrl+C in the dotnet run window)
del quiz.db
# Restart app
dotnet run
```

### Issue 2: LastName Case Mismatch in Database

Even with case-insensitive lookup, if the database has "Ccc" but display shows "ccc", the lookup might fail.

**Check:**
```sql
SELECT DISTINCT LastName FROM CoupleScores;
```

If you see "Ccc" but joined as "ccc", the case-insensitive dictionary should still work. If it doesn't, the issue is the dictionary creation.

### Issue 3: Checkmarks Not Showing

The `?` symbols indicate the `Matched` property is `false` or `undefined`. This is calculated in `QuizHub.cs`:

```csharp
Matched = i < result.Partner2Answers.Count && 
         string.Equals(a.Trim(), partner2Answer.Trim(), StringComparison.OrdinalIgnoreCase)
```

For Q1: "5" vs "5" should definitely match. If `?` shows instead of ?, then:
- JavaScript isn't reading the `Matched` property correctly
- OR the property is `false` when it should be `true`

## Quick Fix Test

### Test with Fresh Start

1. **Stop the app** (Ctrl+C in the dotnet run window)

2. **Delete the database:**
   ```powershell
   cd C:\Users\samee\source\repos\FunApp\FunApp
   del quiz.db
   ```

3. **Start the app:**
   ```powershell
   dotnet run
   ```

4. **Test scenario:**
   - Navigate to http://localhost:5000
   - Click "Couple Players"
   - Add 2 questions (not 4, keep it simple)
   - Join as:
     - Player 1: "John Test"
     - Player 2: "Jane Test"
   - Q1: Both answer "yes"
   - Click "Next Question"
   - Q2: Both answer "no"
   - Click "Show Results"

5. **Expected result:**
   ```
   Mr & Mrs Test
   2
   Total Score
   Q1: Partner 1: yes | Partner 2: yes ?
   Q2: Partner 1: no | Partner 2: no ?
   ```

6. **Check logs for:**
   ```
   [DB QUERY] Returning 1 couple totals:
   [DB QUERY]   - 'Test': 2 points
   info: Couple 'Test': DB Score=2, Calculated Score=2, Final Score=2
   ```

## Browser Console Check

Open browser DevTools (F12) and check Console for JavaScript errors:

```javascript
// Check if results are being received
// You should see objects with:
{
  Name: "Mr & Mrs ccc",
  Score: 1,
  Answers: [
    { Answer: "Partner 1: 5 | Partner 2: 5", Matched: true },
    { Answer: "Partner 1: 5 | Partner 2: 3", Matched: false },
    { Answer: "Partner 1: 3 | Partner 2: Xl", Matched: false },
    { Answer: "Partner 1: xl | Partner 2: N/A", Matched: false }
  ]
}
```

If `Matched: true` is there but `?` shows instead of ?, the issue is in the JavaScript rendering logic in `Index.cshtml`.

## The JavaScript Rendering

In `FunApp/Pages/Index.cshtml`, the results are rendered around line 290:

```javascript
const matchIcon = matched ? ' ?' : '';
```

This should add ? when `matched` is true. If it's showing `?` instead, either:
- `matched` is `undefined` (property name mismatch)
- `matched` is `false` when it should be `true`
- JavaScript logic has changed

**Check the actual line:**
```javascript
const matched = a.Matched || a.matched || false;
```

This tries both `PascalCase` and `camelCase`. If neither exists, it defaults to `false`.

## Summary of What to Check

1. ? **Code is correct** - We've verified this
2. ? **App restarted** - Done at 8:32 PM
3. ? **Database exists** - Modified at 8:38 PM
4. ? **Scores in database** - Need to verify
5. ? **Session ID match** - Need to verify
6. ? **Case matching** - Need to verify
7. ? **Checkmark logic** - Need to verify

## Next Action

**PLEASE DO THIS:**

1. In the PowerShell window where your app is running, look for the logs after clicking "Show Results"

2. Copy and paste everything that appears after you click "Show Results", especially lines containing:
   - `[DB QUERY]`
   - `[GetCoupleResults]`
   - `Couple 'ccc':`

3. Also run this and share the output:
   ```powershell
   cd C:\Users\samee\source\repos\FunApp\FunApp
   Get-Content quiz.db | Select-String "ccc" -SimpleMatch
   ```

This will help us see exactly what's happening!

## Temporary Workaround

If the database scores aren't working, the `GetCoupleResults()` method should fall back to the **calculated score** from `_allAnswers`. 

In `QuizHub.cs` line 229:
```csharp
var finalScore = dbScore >= 0 ? dbScore : result.TotalScore;
```

So even if database lookup fails (dbScore = -1), it should use `result.TotalScore` which is calculated from answers.

If BOTH are showing 0, then the issue is in `GetCoupleResults()` calculation logic in `QuizService.cs`.

Let's verify the calculation is working by checking if the logs show:
```
[GetCoupleResults] Couple ccc Q1: '5' vs '5' => MATCH
```

If this log appears, the calculation IS working, and the issue is the transfer to the UI.
