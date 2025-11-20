# Quick Testing Guide for Couple Scores

## Step 1: Restart Your App
1. Stop the current app (press Ctrl+C in the terminal)
2. Run: `dotnet run --project FunApp/FunApp.csproj`
3. Wait for "Now listening on: http://localhost:5000"

## Step 2: Open Test Page
1. Open `couple-score-test.html` in your browser
2. Click "Connect to Server"
3. You should see "? Connected to Server"

## Step 3: Run Automated Test
Click the buttons in order:
1. **Set Couple Mode** - Should see "Game Mode Changed: Couple"
2. **Join as "John Test"** - Should see "User Joined: John Test"
3. **Join as "Jane Test"** - Should see "User Joined: Jane Test" and "Couple Formed: Mr & Mrs Test"
4. **Next Question** - Should see a question appear
5. **John Answers "Pizza"** - Should see "Answer Received"
6. **Jane Answers "Pizza"** - Should see "Answer Received"
7. **Next Question (Evaluate)** - This evaluates the previous question
8. **Show Results** - Should see the score!

## Step 4: Check Server Logs
Look at your terminal where the app is running. You should see:

After Step 7 (Next Question/Evaluate):
```
[DB SAVE] Saved couple score: SessionId=X, LastName=Test, QuestionId=Y, Matched=True, Points=1
```

After Step 8 (Show Results):
```
[GetCoupleTotalScores] Starting query for session X
[GetCoupleTotalScores] Found 1 total score entries in database
[GetCoupleTotalScores]   - LastName='Test', QuestionId=Y, PointsAwarded=1
[GetCoupleTotalScores] Grouped into 1 couples:
[GetCoupleTotalScores]   - 'Test': 1 points
[GetCoupleTotalScores] Returning 1 couple totals
```

## Step 5: Check Browser Console
In the test page, press F12 to open Developer Tools, go to Console tab.

After Step 8 (Show Results), you should see:
```javascript
[SUCCESS] Results received:
[
  {
    "Name": "Mr & Mrs Test",
    "Score": 1
  }
]
Couple 1: Mr & Mrs Test - Score: 1
```

## What Should Happen
- ? John and Jane join as a couple
- ? They both answer "Pizza" (same answer)
- ? When you advance to next question, their answers are evaluated
- ? Database saves: PointsAwarded=1 (because they matched)
- ? Show Results displays: "Mr & Mrs Test" with Score: 1

## If Scores Are Still Zero

### Check 1: Are Scores Being Saved?
Look for `[DB SAVE]` in server logs after Step 7.
- **If missing**: The evaluation isn't happening
- **If present**: Scores are being saved

### Check 2: Are Scores Being Retrieved?
Look for `[GetCoupleTotalScores]` in server logs after Step 8.
- **If "Found 0 total score entries"**: Database is empty
- **If "Found X entries"**: Scores exist but might not be reaching UI

### Check 3: What Does JavaScript Receive?
Look at browser console after Step 8.
- **If Score is 0**: Server is sending wrong data
- **If Score is missing**: Server isn't sending Score property
- **If Score is 1**: JavaScript has correct data, rendering might be wrong

## Alternative: Manual Testing

If the automated test doesn't work (since it uses one connection for both partners), test manually:

1. **Host Computer:**
   - Open Index page (http://localhost:5000)
   - Click "Couple Players"
   - Click "Next Question"

2. **Phone/Tablet 1 (John):**
   - Open Join page
   - Enter name: "John Test"
   - Answer the question: "Pizza"

3. **Phone/Tablet 2 (Jane):**
   - Open Join page  
   - Enter name: "Jane Test"
   - Answer the question: "Pizza"

4. **Host Computer:**
   - Click "Next Question" (this evaluates)
   - Check server logs for [DB SAVE]
   - Click "Show Results"
   - Check browser console (F12)

## Expected Server Log Output

Complete flow should show:
```
[INFO] Game mode set to: Couple
[INFO] User 'John Test' joined the quiz (FirstName: John, LastName: Test)
[INFO] User 'Jane Test' joined the quiz (FirstName: Jane, LastName: Test)
[INFO] Couple formed: Mr & Mrs Test
[INFO] Advanced to question 1/X: [Question text]
[INFO] [DEBUG] EvaluateCoupleAnswers: Found 1 couples to evaluate
[INFO] [DEBUG] Couple test: Comparing 'Pizza' vs 'Pizza' => MATCHED
[INFO] [DEBUG] Couple Test: Awarded 1 point. Total now: 1
[INFO] [DB SAVE] Saved couple score: SessionId=1, LastName=Test, QuestionId=1, Matched=True, Points=1
[INFO] Getting couple scores from database for session 1
[INFO] [GetCoupleTotalScores] Found 1 total score entries in database
[INFO] [GetCoupleTotalScores]   - LastName='Test', QuestionId=1, PointsAwarded=1
[INFO] [GetCoupleTotalScores] Grouped into 1 couples:
[INFO] [GetCoupleTotalScores]   - 'Test': 1 points
[INFO] Returning 1 couple results to UI
```

## If You See "0 switches" and "No answers submitted"

This means the JavaScript is treating it as Individual mode, not Couple mode.

**Possible causes:**
1. The result doesn't have `Name` starting with "Mr & Mrs"
2. The result doesn't have a `Score` property
3. The result is in wrong format

**Check browser console for:**
```javascript
=== GetAllUserAnswers RESPONSE ===
Is couple mode: false  // <-- Should be TRUE!
```

If it says `false`, then the Name field is wrong. Check server logs to see what Name is being sent.

## Success Criteria

? Server logs show scores being saved
? Server logs show scores being retrieved
? Browser console shows correct data structure
? UI displays "Mr & Mrs Test" with correct score

Let me know which step fails and what the logs show!
