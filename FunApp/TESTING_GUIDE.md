# Testing Guide for Fixed Issues

## Issue 1: Questions Not Loaded from DB on Startup

### What Was Fixed
- ? Added error handling for GetQuestions calls
- ? Added console logging to track loading process
- ? Added connection status indicators
- ? Added automatic reload on reconnection

### How to Test

#### Step 1: Check Database
```powershell
# Open SQLite database
sqlite3 quiz.db

# Check if questions exist
SELECT * FROM Questions;

# If no questions, add some:
INSERT INTO Questions (Text, GameMode, CreatedAt) VALUES 
('Test Individual Question 1', 0, datetime('now')),
('Test Individual Question 2', 0, datetime('now')),
('Test Couple Question 1', 1, datetime('now')),
('Test Couple Question 2', 1, datetime('now'));
```

#### Step 2: Test Admin Page

1. **Start the app:**
   ```powershell
   dotnet run
   ```

2. **Open Admin page:**
   ```
   http://localhost:5000/Admin
   ```

3. **Open browser console (F12)**

4. **Look for these messages:**
   ```
   ? SignalR connected successfully
   Loading questions...
   Loaded Individual questions: [...]
   Loaded Couple questions: [...]
   ```

5. **Verify questions appear in the UI:**
   - Individual questions section should show questions
   - Couple questions section should show questions

#### Step 3: What If Questions Don't Load?

**If you see:**
```
Error loading Individual questions: ...
```

**Check:**
1. Is the database file accessible?
2. Does the Questions table exist?
3. Are there any build errors?

**Manual test:**
```javascript
// In browser console:
connection.invoke('GetQuestions', 'Individual').then(console.log);
// Should return array of questions
```

---

## Issue 2: Couple Players Button Not Clicking

### What Was Fixed
- ? Added connection state check before invoking
- ? Added console logging on button click
- ? Added error alerts if connection fails
- ? Added activity log messages

### How to Test

#### Step 1: Test Individual Button

1. **Open main page:**
   ```
   http://localhost:5000
   ```

2. **Open browser console (F12)**

3. **Click "Individual Players" button**

4. **Look for console messages:**
   ```
   Individual button clicked
   Game mode set to Individual
   ```

5. **Check activity log in side menu:**
   - Should show: "?? Switched to Individual mode"

6. **Check button styling:**
   - Individual button should have white background
   - Couple button should have semi-transparent background

#### Step 2: Test Couple Button

1. **Click "Couple Players" button**

2. **Look for console messages:**
   ```
   Couple button clicked
   Game mode set to Couple
   ```

3. **Check activity log:**
   - Should show: "?? Switched to Couple mode"

4. **Check button styling:**
   - Couple button should have white background
   - Individual button should have semi-transparent background

#### Step 3: What If Button Doesn't Work?

**If you see:**
```
SignalR not connected
```

**Then:**
- Connection didn't establish
- Refresh the page
- Check server logs for errors

**If nothing happens:**

**Test 1: Check if button exists**
```javascript
// In browser console:
document.getElementById("selectCouple")
// Should return button element, not null
```

**Test 2: Try clicking manually**
```javascript
document.getElementById("selectCouple").click();
// Should see console logs and mode change
```

**Test 3: Check button styles**
```javascript
const btn = document.getElementById("selectCouple");
console.log(window.getComputedStyle(btn).pointerEvents);
// Should be 'auto', not 'none'
```

**Test 4: Invoke method directly**
```javascript
connection.invoke("SetGameMode", "Couple")
    .then(() => console.log("Success"))
    .catch(err => console.error(err));
```

---

## Complete Test Checklist

### Admin Page (Questions Loading)
- [ ] App starts without errors
- [ ] Admin page opens
- [ ] Browser console shows "? SignalR connected"
- [ ] Console shows "Loading questions..."
- [ ] Console shows "Loaded Individual questions: [...]"
- [ ] Console shows "Loaded Couple questions: [...]"
- [ ] Individual questions appear in UI
- [ ] Couple questions appear in UI
- [ ] Can add new questions
- [ ] Questions persist after page refresh

### Main Page (Couple Button)
- [ ] Main page opens
- [ ] Browser console shows "? SignalR connected"
- [ ] Click "Individual Players" ? Console shows "Individual button clicked"
- [ ] Individual button changes to white background
- [ ] Activity log shows mode change
- [ ] Click "Couple Players" ? Console shows "Couple button clicked"
- [ ] Couple button changes to white background
- [ ] Activity log shows mode change
- [ ] Can switch between modes multiple times
- [ ] Mode persists during quiz

---

## Quick Debug Commands

### Check Database
```powershell
sqlite3 quiz.db "SELECT * FROM Questions;"
sqlite3 quiz.db "SELECT * FROM QuizSessions;"
```

### Check SignalR Connection
```javascript
// In browser console:
connection.state
// Should be: 1 (Connected)
```

### Test Couple Button Programmatically
```javascript
// Force click
document.getElementById("selectCouple").click();

// Or invoke directly
connection.invoke("SetGameMode", "Couple").then(() => alert("Success!"));
```

### Check for JavaScript Errors
```javascript
// Open console (F12)
// Look for red error messages
// Most common: "connection is not defined" or "Cannot read property '...' of null"
```

---

## Expected Behavior

### After All Fixes

**Admin Page:**
```
1. Page loads
2. Console: "? SignalR connected successfully"
3. Console: "Loading questions..."
4. Console: "Loaded Individual questions: [array]"
5. Console: "Loaded Couple questions: [array]"
6. UI shows all questions
```

**Main Page:**
```
1. Page loads
2. Console: "? SignalR connected successfully"
3. Activity log: "? Connected to server"
4. Click "Couple Players"
5. Console: "Couple button clicked"
6. Console: "Game mode set to Couple"
7. Activity log: "?? Switched to Couple mode"
8. Button styling changes
```

---

## If Still Not Working

### For Questions Not Loading:

1. **Check database file exists:**
   ```powershell
   ls quiz.db
   ```

2. **Verify database schema:**
   ```powershell
   sqlite3 quiz.db ".schema Questions"
   ```

3. **Add test questions:**
   ```powershell
   sqlite3 quiz.db "INSERT INTO Questions (Text, GameMode, CreatedAt) VALUES ('Test Q1', 0, datetime('now'));"
   ```

4. **Check server logs** for GetQuestions errors

### For Couple Button Not Clicking:

1. **Verify button HTML** - Check Elements tab in DevTools

2. **Check for CSS z-index issues** - Something might be covering the button

3. **Try different browser** - Test in Chrome, Edge, Firefox

4. **Clear browser cache** - Ctrl+F5 to hard refresh

5. **Check JavaScript console** - Look for any errors that prevent script from running

---

## Files Modified

- ? `FunApp/Pages/Index.cshtml` - Added connection status handling and better error logging
- ? `FunApp/Pages/Admin/Index.cshtml` - Added question loading error handling
- ? `FunApp/TWO_ISSUES_FIX.md` - Documentation
- ? `FunApp/TESTING_GUIDE.md` - This file

## Summary

Both issues should now be fixed with:
1. **Better error handling** - See what's failing
2. **Console logging** - Track execution flow
3. **Connection checks** - Prevent calls when disconnected
4. **User feedback** - Alerts and activity log messages

**Restart your app and test!** ??
