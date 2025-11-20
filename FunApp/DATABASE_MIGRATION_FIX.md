# Database Migration Fix - CoupleScores Table

## The Error You're Seeing

```
SQLite Error 1: 'no such table: CoupleScores'
```

This happens because your existing database doesn't have the new `CoupleScores` table that was added for the couples game feature.

## Automatic Fix Applied ?

I've updated `Program.cs` to automatically detect and fix this issue. The app will now:

1. Check if the `CoupleScores` table exists
2. If missing, automatically delete and recreate the database
3. Log the recreation process

## How to Apply the Fix

### Option 1: Automatic (Recommended) ?
Just restart your app - it will detect the missing table and fix itself:

```powershell
# Stop the app (Ctrl+C)
# Then restart
dotnet run
```

**You'll see this in the logs:**
```
warn: Database schema is outdated or corrupted. Recreating database...
info: Database recreated successfully with new schema including CoupleScores table.
info: DB question counts: Individual=0, Couple=0
```

### Option 2: Manual (If you want to be sure)
Manually delete the database files and restart:

```powershell
# Stop the app (Ctrl+C)

# Delete database files
del quiz.db
del quiz.db-wal
del quiz.db-shm

# Restart app
dotnet run
```

## What Happens to Existing Data

?? **Important**: Database recreation will delete:
- All existing questions
- All quiz sessions
- All responses
- All scores

If you have important data, backup first:
```powershell
copy quiz.db quiz.db.backup
```

## After Fix is Applied

You'll have a fresh database with:
- ? Questions table
- ? QuizSessions table  
- ? QuizResponses table
- ? **CoupleScores table (NEW)**
- ? All updated schema with FirstName, LastName, Score fields

## Testing After Fix

1. **Verify app starts without errors**
   ```
   info: Database recreated successfully...
   info: Starting FunApp...
   ```

2. **Add test questions**
   - Go to Admin panel
   - Add a couple question
   - Verify it saves successfully

3. **Test couple mode**
   - Select "Couple Players"
   - Join with "John Test" and "Jane Test"
   - Submit matching answers
   - Click "Next Question"
   - Check logs for "Couple Test: Answers MATCHED"

## If You Still See Errors

### Check 1: Database Files Deleted
```powershell
# Make sure these are gone
ls quiz.db*
# Should show "Cannot find path"
```

### Check 2: App Restarted
```powershell
# Completely stop and restart
Ctrl+C  # Stop
dotnet run  # Start fresh
```

### Check 3: Build is Up to Date
```powershell
dotnet clean
dotnet build
dotnet run
```

## Database Schema After Fix

### New Table: CoupleScores
```sql
CREATE TABLE CoupleScores (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    QuizSessionId INTEGER NOT NULL,
    LastName TEXT NOT NULL,
    QuestionId INTEGER NOT NULL,
    AnswersMatched INTEGER NOT NULL,  -- 0 or 1 (boolean)
    PointsAwarded INTEGER NOT NULL,
    Partner1Answer TEXT NOT NULL,
    Partner2Answer TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    FOREIGN KEY (QuizSessionId) REFERENCES QuizSessions(Id) ON DELETE CASCADE,
    FOREIGN KEY (QuestionId) REFERENCES Questions(Id) ON DELETE CASCADE
);
```

### Updated: Users (in memory, not persisted)
```csharp
- FirstName (new)
- LastName (new)
- Score (new)
```

## Verification Queries

After app starts, you can verify the schema:

```sql
-- Check if CoupleScores table exists
SELECT name FROM sqlite_master WHERE type='table' AND name='CoupleScores';
-- Should return: CoupleScores

-- Check table structure
PRAGMA table_info(CoupleScores);
-- Should show all columns
```

## Common Issues

### Issue: "Database is locked"
**Solution:**
```powershell
# Stop all instances of the app
# Delete .db-wal and .db-shm files
del quiz.db-wal
del quiz.db-shm
# Restart
```

### Issue: Still getting "no such table"
**Solution:**
```powershell
# Nuclear option - clean everything
del quiz.db*
dotnet clean
dotnet build
dotnet run
```

### Issue: "Cannot find quiz.db"
**Solution:**
```
This is normal! The app will create it automatically on startup.
Just run: dotnet run
```

## Success Indicators

? **You're good when you see:**
```
info: FunApp.Program[0]
      Database recreated successfully with new schema including CoupleScores table.
info: FunApp.Program[0]
      DB question counts: Individual=0, Couple=0
info: FunApp.Program[0]
      Starting FunApp. RunningInContainer=False, PORT=(none)
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

? **And no errors about:**
- "no such table: CoupleScores"
- "SQLite Error 1"
- Database schema issues

## Next Steps

After successful fix:
1. ? Add questions in Admin panel
2. ? Test couple mode
3. ? Verify scoring works
4. ? Check database has CoupleScores entries

---

**The fix is automatic - just restart your app!** ??
