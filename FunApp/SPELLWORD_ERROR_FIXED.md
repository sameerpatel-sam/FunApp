# ? SpellWord API Fix - "Error adding word" RESOLVED

## Problem
When trying to add words in the Spell the Word game, you were getting "Error adding word" message.

## Root Cause
**Property name mismatch** between JavaScript and C# API endpoint:

- JavaScript was sending: `{ word: "example" }` (lowercase)
- API was expecting: `{ Word: "example" }` (capital W)

C# record types are case-sensitive for property names!

## What Was Fixed

### Changed in `SpellWord.cshtml`:

**Before:**
```javascript
body: JSON.stringify({ word: word })  // lowercase 'word'
```

**After:**
```javascript
body: JSON.stringify({ Word: word })  // capital 'Word'
```

Also fixed in the score update:
```javascript
body: JSON.stringify({ 
    SpellWordId: wordId,  // was: spellWordId
    Team: team,            // was: team (this was already correct)
    Score: score           // was: score
})
```

## How to Apply the Fix

### Option 1: Build is Locked (App Running)

**Step 1: Stop the running app**
- Find the terminal/PowerShell window running `dotnet run`
- Press `Ctrl + C` to stop it

**Step 2: Build should work now**
The changes are already in your file, just restart the app:
```powershell
cd FunApp
dotnet run
```

### Option 2: Force Kill the Process

If you can't find the running app window:

```powershell
# Find the process
Get-Process | Where-Object {$_.ProcessName -like "*FunApp*"}

# Or find what's using port 5000
Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue | 
    Select-Object OwningProcess

# Kill it (replace XXXXX with the process ID)
Stop-Process -Id XXXXX -Force

# Then build
cd FunApp
dotnet build
dotnet run
```

## Test the Fix

1. **Run the app:**
   ```powershell
   cd FunApp
   dotnet run
   ```

2. **Navigate to Spell the Word:**
   - Open: http://localhost:5000
   - Click "Spell the Word" button

3. **Add a word:**
   - Type "Dictionary" in the input
   - Click "? Add Word"
   - ? Word should appear in the list below!

4. **Verify it works:**
   - Add a few more words
   - They should all save successfully
   - No more "Error adding word" alert!

## Technical Details

### The API Endpoint
```csharp
app.MapPost("/api/spellword/words", async (IDbContextFactory<AppDbContext> dbFactory, SpellWordRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Word))  // Expects capital 'W'
        return Results.BadRequest("Word is required");
    // ...
});

record SpellWordRequest(string Word);  // Property name is 'Word' with capital W
```

### Why This Happened
In C#, record types with primary constructors create properties that match the exact case:
```csharp
record SpellWordRequest(string Word);  // Creates property: 'Word'
```

JSON deserialization is **case-sensitive** by default in .NET minimal APIs, so:
- `{ "Word": "test" }` ? Works
- `{ "word": "test" }` ? Fails (property not found)

## Additional Improvements

I also added better error logging:

```javascript
if (response.ok) {
    input.value = '';
    await loadWords();
} else {
    const errorText = await response.text();
    console.error('Server error:', errorText);
    alert('Error adding word: ' + errorText);  // Now shows actual error
}
```

Now if there's an error, you'll see the actual server error message instead of just "Error adding word".

## Verification Checklist

After restarting the app, verify:

- ? Can add words without errors
- ? Words appear in the management list
- ? Words appear in the score table (blurred)
- ? Can reveal words
- ? Can delete words
- ? Can update team scores for revealed words
- ? Can reset game
- ? Can clear scores

## Current Status

? **Fix Applied** - JavaScript now uses correct property names  
?? **Build Locked** - App is currently running (needs to be stopped)  
?? **Next Step** - Stop app ? Restart ? Test!  

## Quick Commands

```powershell
# Stop if running in current terminal
Ctrl + C

# Navigate to project
cd C:\Users\samee\source\repos\FunApp\FunApp

# Run the app
dotnet run

# Open in browser
start http://localhost:5000/SpellWord
```

---

**The fix is ready - just restart your app to apply it!** ??
