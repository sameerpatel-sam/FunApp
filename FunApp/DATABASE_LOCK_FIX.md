# Database File Lock Issue - FIXED

## The Error You Saw

```
System.IO.IOException: The process cannot access the file 'quiz.db' 
because it is being used by another process.
```

## Root Cause

The database context was still holding a connection to the database file when the code tried to delete it. SQLite locks the database file while connections are open.

**The Problem Flow:**
1. Code opens database context to check tables ?
2. Check finds CoupleScores missing ??
3. Code tries to delete database ?
4. **But context is still open!** ? File locked ? Error

## The Fix Applied

I've updated `Program.cs` to properly manage database contexts:

### Key Changes:

1. **Dispose contexts immediately after use:**
```csharp
// OLD (Wrong):
using var db = dbFactory.CreateDbContext();
// db stays open until end of using block

// NEW (Correct):
using (var db = dbFactory.CreateDbContext())
{
    // Check tables
} // db disposed HERE, before deletion
```

2. **Added small delay for file release:**
```csharp
System.Threading.Thread.Sleep(100);
// Give OS time to release file locks
```

3. **Better error handling:**
```csharp
catch (IOException ioEx) when (ioEx.Message.Contains("being used by another process"))
{
    // Specific guidance for file lock issues
}
```

## How to Fix Your Current Situation

### Option 1: Manual Cleanup (Recommended)

**Stop the app completely:**
```powershell
# Press Ctrl+C in terminal
# Wait for app to fully stop
```

**Delete database files manually:**
```powershell
del quiz.db
del quiz.db-wal
del quiz.db-shm
```

**Start app:**
```powershell
dotnet run
```

### Option 2: Find and Close Process

**Find what's locking the file:**
```powershell
# Using PowerShell
Get-Process | Where-Object {$_.Path -like "*FunApp*"}

# Or using Resource Monitor
# 1. Open Resource Monitor (resmon.exe)
# 2. Go to CPU tab
# 3. Search for "quiz.db"
# 4. Close the process
```

**Then restart:**
```powershell
dotnet run
```

### Option 3: Restart Computer
If all else fails (nuclear option):
```powershell
# Close everything
# Restart computer
# Delete quiz.db files
# Start app
```

## What You'll See After Fix

### Successful Migration:
```
warn: CoupleScores table not found. Database needs migration.
warn: Database schema is outdated or incomplete. Recreating database...
info: ? Database recreated successfully with CoupleScores table.
info: DB question counts: Individual=0, Couple=0
info: Starting FunApp...
```

### If File Still Locked (Helpful Error):
```
error: Database file is locked. Please close any applications accessing quiz.db and restart the app.
error: Alternative: Delete quiz.db manually and restart.
```

## Why This Happened

### Common Causes:

1. **Multiple app instances running**
   - Check Task Manager for multiple FunApp.exe processes
   - Kill all instances before restarting

2. **Database browser open**
   - DB Browser for SQLite
   - Visual Studio's SQL Server Object Explorer
   - Any SQLite viewer

3. **Previous crash didn't release lock**
   - WAL (Write-Ahead Log) file still locked
   - Solution: Delete all quiz.db* files

4. **Antivirus scanning the file**
   - Temporarily disable antivirus
   - Or add exception for quiz.db

## Prevention

### The fix prevents this by:

1. ? **Disposing contexts properly** - Release locks immediately
2. ? **Adding delay** - Give OS time to clean up
3. ? **Better error messages** - Clear guidance when it happens
4. ? **Scoped contexts** - Each check gets its own short-lived context

## Testing the Fix

### Test 1: Clean Start
```powershell
# Delete database
del quiz.db*

# Start app
dotnet run

# Expected: Database created successfully
```

### Test 2: Migration Scenario
```powershell
# Create old database (without CoupleScores)
# Start app

# Expected: 
# - Warning about migration
# - Database recreated
# - No file lock errors
```

### Test 3: Normal Operation
```powershell
# Start app with existing correct database

# Expected:
# - No warnings
# - Question counts shown
# - App starts normally
```

## Verification

After restart, verify:

? **No file lock errors**
```
# Should NOT see:
error: The process cannot access the file...
```

? **Database created successfully**
```
info: ? Database recreated successfully with CoupleScores table.
```

? **App starts normally**
```
info: Now listening on: http://localhost:5000
```

## Additional Tips

### Check for Lingering Processes

**Before starting app:**
```powershell
# PowerShell
Get-Process dotnet | Stop-Process -Force

# Or check Task Manager:
# Ctrl+Shift+Esc ? Details tab ? Look for "FunApp.exe" or "dotnet.exe"
```

### Clean SQLite WAL Files

SQLite's WAL mode creates these files:
- `quiz.db` - Main database
- `quiz.db-wal` - Write-Ahead Log (can lock)
- `quiz.db-shm` - Shared Memory (can lock)

**Always delete all three:**
```powershell
del quiz.db*
```

### Disable WAL Mode (Optional)

If you keep having lock issues, disable WAL mode:

```csharp
// In Program.cs, update connection string:
var connectionString = builder.Configuration.GetConnectionString("Default") 
    ?? "Data Source=quiz.db;Mode=ReadWrite;Journal Mode=Delete";
```

But WAL mode is better for performance, so try the fixes above first.

## Summary

### What Was Wrong:
- Database context stayed open while trying to delete database
- File remained locked
- Deletion failed

### What's Fixed:
- Contexts properly disposed before deletion
- Added delay for file system to catch up
- Better error messages with solutions
- Each operation uses its own scoped context

### What to Do:
1. **Stop app completely**
2. **Delete quiz.db files manually**
3. **Restart app**
4. **Verify database recreates successfully**

---

## Quick Fix Command

```powershell
# Stop app (Ctrl+C)
# Then run:
del quiz.db*; dotnet run
```

That's it! The database will be recreated with the correct schema. ??
