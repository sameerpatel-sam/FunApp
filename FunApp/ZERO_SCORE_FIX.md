# Zero Score & Question Mark Fix

## Issues Found

### Issue 1: Couple Score Shows 0 ?
**Root Cause:** Case-sensitivity mismatch between database storage and lookup.

**The Problem:**
1. Database stored LastName as lowercase (e.g., "kr", "smith")
2. Display logic tried to lookup with proper case (e.g., "Kr", "Smith")
3. Dictionary used default case-sensitive comparer
4. Lookup failed ? Score defaulted to 0

**Example:**
```
Database: "kr" ? 3 points
Memory: "Kr" 
Lookup: dbScores["Kr"] ? Not found! ? Score = 0
```

### Issue 2: Matched Answers Show "?" Instead of ? ?
**Root Cause:** The comparison was correct, but not showing properly in UI.

**The Problem:**
```csharp
// This comparison WAS working correctly:
string.Equals(a.Trim(), partner2Answer.Trim(), StringComparison.OrdinalIgnoreCase)

// But answers like:
Partner 1: "rr" vs Partner 2: "Rr"
Partner 1: "tty" vs Partner 2: "Tty"

// Should match (case-insensitive) but weren't showing checkmarks
```

## Fixes Applied

### Fix 1: Use Case-Insensitive Dictionary ?

**File: `PersistentQuizService.cs`**

Changed `GetCoupleTotalScoresAsync()` to return a case-insensitive dictionary:

```csharp
public async Task<Dictionary<string, int>> GetCoupleTotalScoresAsync(int sessionId)
{
    // ... query database ...
    
    // Use case-insensitive grouping but preserve original casing
    var grouped = allScores
        .GroupBy(c => c.LastName, StringComparer.OrdinalIgnoreCase)
        .Select(g => new { LastName = g.First().LastName, TotalScore = g.Sum(c => c.PointsAwarded) })
        .ToList();

    // Create result dictionary with case-insensitive key comparer
    var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    foreach (var item in grouped)
    {
        result[item.LastName] = item.TotalScore;
    }
    
    return result;
}
```

**What This Does:**
- Groups database entries case-insensitively (kr, Kr, KR all grouped together)
- Returns dictionary that accepts any case for lookup
- `dbScores["kr"]`, `dbScores["Kr"]`, `dbScores["KR"]` all work!

### Fix 2: Store LastName With Proper Case ?

**File: `QuizService.cs`**

Changed `EvaluateCoupleAnswers()` to return proper case LastName:

```csharp
public List<(string lastNameKey, string partner1Answer, string partner2Answer, bool matched, string properLastName)> EvaluateCoupleAnswers()
{
    foreach (var (lastName, partners) in couples)
    {
        var partner1 = partners[0];
        var properLastName = partner1.LastName; // Get proper case from user object
        
        // ...evaluation logic...
        
        results.Add((lastName, answer1.Answer, answer2.Answer, matched, properLastName));
    }
}
```

**File: `QuizHub.cs`**

Updated `EvaluateCoupleAnswersForCurrentQuestion()` to use proper case:

```csharp
private async Task EvaluateCoupleAnswersForCurrentQuestion()
{
    foreach (var (lastNameKey, partner1Answer, partner2Answer, matched, properLastName) in evaluations)
    {
        // Use properLastName (with correct case) for database storage
        await _persistent.SaveCoupleScoreAsync(properLastName, questionId, matched, partner1Answer, partner2Answer);
    }
}
```

**What This Does:**
- Database now stores "Kr" or "Smith" (proper case)
- Makes debugging easier
- Matches the display format

### Fix 3: Simplify Score Lookup ?

**File: `QuizHub.cs`**

Simplified `GetAllUserAnswers()` lookup:

```csharp
foreach (var (lastName, result) in coupleResults)
{
    // Dictionary is now case-insensitive, so direct lookup works
    var dbScore = -1;
    if (dbScores.TryGetValue(result.LastName, out var score))
    {
        dbScore = score;
    }
    
    // Use DATABASE score if available
    var finalScore = dbScore >= 0 ? dbScore : result.TotalScore;
}
```

**What This Does:**
- Simple `TryGetValue()` call works regardless of case
- No need for complex LINQ searches
- Cleaner, more maintainable code

### Fix 4: Fixed Matched Indicator Logic ?

**File: `QuizHub.cs`**

Updated the Answers property to properly compare:

```csharp
Answers = result.Partner1Answers.Select((a, i) =>
{
    var partner2Answer = i < result.Partner2Answers.Count ? result.Partner2Answers[i] : "N/A";
    return new
    {
        Answer = $"Partner 1: {a} | Partner 2: {partner2Answer}",
        Matched = i < result.Partner2Answers.Count && 
                 string.Equals(a.Trim(), partner2Answer.Trim(), StringComparison.OrdinalIgnoreCase)
    };
}).ToList()
```

**What This Does:**
- Properly uses `partner2Answer` variable (not directly from list)
- Case-insensitive comparison
- Trims whitespace before comparing

## Before vs After

### Before Fix (Bad) ?

```
Mr & Mrs kr 0 Total Score
Q1: Partner 1: 5 | Partner 2: 4
Q2: Partner 1: rr | Partner 2: Rr ?
Q3: Partner 1: tty | Partner 2: Tty ?
```

**Database:**
```
LastName: "kr", QuestionId: 1, PointsAwarded: 0
LastName: "kr", QuestionId: 2, PointsAwarded: 1
LastName: "kr", QuestionId: 3, PointsAwarded: 1
```

**Lookup:**
```
dbScores["kr"] exists ? 2 points
result.LastName = "Kr" (proper case)
dbScores["Kr"] ? NOT FOUND! ? Score = 0
```

### After Fix (Good) ?

```
Mr & Mrs Kr 3 Total Score
Q1: Partner 1: 5 | Partner 2: 4
Q2: Partner 1: rr | Partner 2: Rr ?
Q3: Partner 1: tty | Partner 2: Tty ?
```

**Database:**
```
LastName: "Kr", QuestionId: 1, PointsAwarded: 0
LastName: "Kr", QuestionId: 2, PointsAwarded: 1
LastName: "Kr", QuestionId: 3, PointsAwarded: 1
```

**Lookup:**
```
dbScores = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
dbScores["Kr"] = 2
result.LastName = "Kr"
dbScores["Kr"] ? FOUND! ? Score = 2 ?
```

## Testing the Fixes

### Test Scenario: 3 Questions, Couple with Mixed Case

**Setup:**
1. Clear database: `dotnet ef database drop --force` (or delete FunApp/quiz.db)
2. Recreate database: `dotnet ef database update`
3. Start app: `dotnet run`
4. Select "Couple Players"
5. Add 3 couple questions in Admin
6. Join as "John Kr" and "Jane Kr" (note lowercase 'r')

**Test Steps:**

1. **Question 1: Different Answers**
   - John: "5"
   - Jane: "4"
   - Click "Next Question"
   - ? Database should save: PointsAwarded=0

2. **Question 2: Same Answers (Different Case)**
   - John: "rr"
   - Jane: "Rr"
   - Click "Next Question"
   - ? Database should save: PointsAwarded=1 (case-insensitive match)

3. **Question 3: Same Answers (Different Case)**
   - John: "tty"
   - Jane: "Tty"
   - Click "Show Results"
   - ? Database should save: PointsAwarded=1

4. **Check Results Display:**
   - ? Name: "Mr & Mrs Kr" (proper case)
   - ? Score: 2 (not 0!)
   - ? Q1: Partner 1: 5 | Partner 2: 4 (no checkmark)
   - ? Q2: Partner 1: rr | Partner 2: Rr ? (with checkmark!)
   - ? Q3: Partner 1: tty | Partner 2: Tty ? (with checkmark!)

### Database Verification

After the quiz, check the database:

```bash
# Windows
cd FunApp
dotnet tool install --global dotnet-ef
dotnet ef dbcontext info
# Then open quiz.db with DB Browser for SQLite
```

```sql
-- Should show 3 entries with proper case
SELECT * FROM CoupleScores WHERE LastName = 'Kr';

-- Expected result:
-- LastName | QuestionId | AnswersMatched | PointsAwarded
-- 'Kr'     | 1          | 0              | 0
-- 'Kr'     | 2          | 1              | 1
-- 'Kr'     | 3          | 1              | 1

-- Total score = 0 + 1 + 1 = 2 ?
```

## What You'll See in Logs

### After Fix (Good) ?

```
[DB QUERY] GetCoupleTotalScores(1): Found 3 total score entries
[DB QUERY]   - LastName='Kr', QuestionId=1, PointsAwarded=0
[DB QUERY]   - LastName='Kr', QuestionId=2, PointsAwarded=1
[DB QUERY]   - LastName='Kr', QuestionId=3, PointsAwarded=1
[DB QUERY] Grouped into 1 couples:
[DB QUERY]   - 'Kr': 2 points
[DB QUERY] Returning 1 couple totals:
[DB QUERY]   - 'Kr': 2 points
info: Couple 'Kr': DB Score=2, Calculated Score=2, Final Score=2
```

## Files Modified

1. ? `FunApp/Services/PersistentQuizService.cs`
   - `GetCoupleTotalScoresAsync()` - Use case-insensitive dictionary

2. ? `FunApp/Services/QuizService.cs`
   - `EvaluateCoupleAnswers()` - Return proper case LastName

3. ? `FunApp/Hubs/QuizHub.cs`
   - `EvaluateCoupleAnswersForCurrentQuestion()` - Use proper case for DB
   - `GetAllUserAnswers()` - Simplified lookup
   - Fixed Matched property calculation

## Summary

### Root Causes:
- ? Case-sensitive dictionary lookup
- ? Database stored lowercase, display used proper case
- ? No case-insensitive comparer

### Solutions:
- ? Case-insensitive dictionary with `StringComparer.OrdinalIgnoreCase`
- ? Store proper case LastName in database
- ? Simplified lookup logic
- ? Proper case-insensitive string comparisons

### Benefits:
- ? Score displays correctly
- ? Matched answers show ? checkmark
- ? Works with any case combination
- ? Cleaner, more maintainable code
- ? Better debugging with proper case in logs

## Edge Cases Now Handled

1. **Mixed Case Last Names:**
   - "smith", "Smith", "SMITH" ? All work
   - Database can have any case
   - Display shows proper case

2. **Mixed Case Answers:**
   - "yes" vs "YES" ? Matched ?
   - "Pizza" vs "pizza" ? Matched ?
   - "Hawaii" vs "HAWAII" ? Matched ?

3. **Whitespace:**
   - "yes " vs "yes" ? Matched ?
   - " no" vs "no" ? Matched ?

## Next Steps

1. **Test with fresh database** (recommended)
2. **Test with various case combinations**
3. **Verify all scores show correctly**
4. **Check matched indicators (?) appear**

The fixes are complete and ready! ??

## Technical Notes

### Why StringComparer.OrdinalIgnoreCase?

```csharp
// Default dictionary - Case-sensitive
var dict = new Dictionary<string, int>();
dict["Kr"] = 2;
dict["kr"] // KeyNotFoundException!

// Case-insensitive dictionary
var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
dict["Kr"] = 2;
dict["kr"] // Returns 2! ?
dict["KR"] // Returns 2! ?
dict["kR"] // Returns 2! ?
```

### Why Not Just Use .ToLower() Everywhere?

```csharp
// Option 1: ToLower() everywhere (Bad)
dbScores[result.LastName.ToLower()]
// Problem: Display shows "kr" instead of "Kr"
// Problem: Database has "kr" instead of proper names

// Option 2: Case-insensitive comparer (Good)
var dbScores = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
// Benefit: Display shows "Kr" (proper case)
// Benefit: Database has "Kr" (readable)
// Benefit: Lookup works with any case
```

The case-insensitive comparer is the cleanest solution!
