# Score Showing 0 - Debug and Fix

## The Problem

You're seeing:
```
?? Mr & Mrs Test
0
Total Score
Q1: Partner 1: 4 | Partner 2: 4 ?
Q2: Partner 1: as | Partner 2: As ?
Q3: Partner 1: tie | Partner 2: Sam
```

**Expected Score: 2** (Q1 and Q2 matched)
**Actual Score: 0** ?

## Root Causes Found

### Issue 1: In-Memory Score Cache Cleared
The `_coupleScores` dictionary gets cleared when:
- `SetGameMode()` is called
- Service is restarted
- Session changes

So even though matches were evaluated, the scores were lost.

### Issue 2: Fallback Calculation Not Used
The code had:
```csharp
TotalScore = _coupleScores.ContainsKey(lastName) ? _coupleScores[lastName] : matchedCount,
```

But `_coupleScores[lastName]` was 0 or missing, so it returned 0 instead of using `matchedCount`.

## Fixes Applied

### Fix 1: Use Calculated Score Directly ?

Changed `GetCoupleResults()` to ALWAYS use the calculated score:

```csharp
// OLD (Wrong):
TotalScore = _coupleScores.ContainsKey(lastName) ? _coupleScores[lastName] : matchedCount,

// NEW (Correct):
TotalScore = matchedCount, // Always use fresh calculation
```

This recalculates the score by comparing all answers every time, which is more reliable.

### Fix 2: Added Debug Logging ?

Added detailed console logging to `EvaluateCoupleAnswers()`:

```csharp
Console.WriteLine($"[DEBUG] Couple {lastName}: Comparing '{ans1}' vs '{ans2}' => {(matched ? "MATCHED" : "NOT MATCHED")}");
Console.WriteLine($"[DEBUG] Couple {lastName}: Awarded 1 point. Total now: {_coupleScores[lastName]}");
```

This will help you see exactly what's happening during evaluation.

## How to Test

### Step 1: Restart Your App

```powershell
# Stop app (Ctrl+C)
dotnet run
```

### Step 2: Run the Same Test

1. Select "Couple Players"
2. Join as "John Test" and "Jane Test"
3. Answer Q1: Both say "4"
4. Answer Q2: Both say "as" (or "As")
5. Answer Q3: Different answers
6. Click "Show Results"

### Step 3: Check Console Output

Look for these debug messages:

```
[DEBUG] EvaluateCoupleAnswers: Found 1 couples to evaluate
[DEBUG] Couple test: Partner1=John Test, Partner2=Jane Test
[DEBUG] Couple test: Comparing '4' vs '4' => MATCHED
[DEBUG] Couple test: Awarded 1 point. Total now: 1
[DEBUG] Couple test: Comparing 'as' vs 'As' => MATCHED
[DEBUG] Couple test: Awarded 1 point. Total now: 2
[DEBUG] Couple test: Comparing 'tie' vs 'Sam' => NOT MATCHED
[DEBUG] EvaluateCoupleAnswers: Evaluated 3 couple answers
```

### Step 4: Verify Results

You should now see:

```
?? Mr & Mrs Test
2
Total Score
Q1: Partner 1: 4 | Partner 2: 4 ?
Q2: Partner 1: as | Partner 2: As ?
Q3: Partner 1: tie | Partner 2: Sam
```

## Troubleshooting

### Still Showing 0?

**Check 1: Are evaluations happening?**

Look in console for:
```
[DEBUG] EvaluateCoupleAnswers: Found X couples to evaluate
```

If you see 0 couples:
- Make sure both players have the same last name
- Check logs for "Couple formed: Mr & Mrs Test"

**Check 2: Are answers being captured?**

Look for:
```
[DEBUG] Couple test: SKIPPED - One or both partners haven't answered
[DEBUG]   Partner1 answered: false
[DEBUG]   Partner2 answered: false
```

If you see this:
- Make sure you clicked "Next Question" after each question (to trigger evaluation)
- Or ensure "Show Results" triggers final evaluation

**Check 3: Is matching working?**

Look for:
```
[DEBUG] Couple test: Comparing 'X' vs 'Y' => NOT MATCHED
```

If matches aren't detected:
- Check for extra spaces
- Check for special characters
- Verify case-insensitive comparison is working

### Database Check

Verify scores are saved:

```sql
SELECT * FROM CoupleScores WHERE LastName = 'Test';
```

Should show:
```
| Id | LastName | QuestionId | AnswersMatched | PointsAwarded | Partner1Answer | Partner2Answer |
|----|----------|------------|----------------|---------------|----------------|----------------|
| 1  | Test     | 1          | 1              | 1             | 4              | 4              |
| 2  | Test     | 2          | 1              | 1             | as             | As             |
| 3  | Test     | 3          | 0              | 0             | tie            | Sam            |
```

## Why This Happens

### The Score Calculation Flow

1. **During Quiz:**
   - Partner 1 submits answer ? Stored in `_currentAnswers`
   - Partner 2 submits answer ? Stored in `_currentAnswers`
   - Host clicks "Next Question" ? `EvaluateCoupleAnswers()` called
   - Comparison happens ? Score incremented in `_coupleScores`
   - Answers cleared ? `_currentAnswers.Clear()`

2. **Show Results:**
   - `GetAllUserAnswers()` called
   - Final question evaluated if needed
   - `GetCoupleResults()` called
   - **OLD:** Returns `_coupleScores[lastName]` ? Often 0 or missing
   - **NEW:** Recalculates by comparing all answers ? Accurate score

### Why Recalculation is Better

**In-Memory Cache (`_coupleScores`):**
- ? Gets cleared on mode change
- ? Lost if service restarts
- ? Not persisted between evaluations
- ? Can be out of sync

**Recalculation from `_allAnswers`:**
- ? Always accurate
- ? Works even if cache cleared
- ? Based on actual answer data
- ? No sync issues

## Files Modified

1. ? `FunApp/Services/QuizService.cs`
   - `GetCoupleResults()` - Now uses calculated score
   - `EvaluateCoupleAnswers()` - Added debug logging

## Expected Behavior After Fix

### With 2 Matching, 1 Non-Matching:

**Input:**
- Q1: "4" vs "4" ? Match ?
- Q2: "as" vs "As" ? Match ? (case-insensitive)
- Q3: "tie" vs "Sam" ? No match

**Output:**
```
?? Mr & Mrs Test
2 ? Correct score!
Total Score
Q1: Partner 1: 4 | Partner 2: 4 ?
Q2: Partner 1: as | Partner 2: As ?
Q3: Partner 1: tie | Partner 2: Sam
```

**Database:**
```sql
SELECT LastName, SUM(PointsAwarded) as TotalScore
FROM CoupleScores
GROUP BY LastName;

Result: Test | 2
```

## Next Steps

1. ? Restart app
2. ? Run test scenario
3. ? Check console for debug messages
4. ? Verify score shows as 2
5. ? Check database has correct entries

The fix ensures scores are calculated accurately every time, regardless of cache state! ??
