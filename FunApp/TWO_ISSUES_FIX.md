# Fix for Two Issues

## Issue 1: Questions Not Loaded from DB on Startup ?

### Root Cause
The Admin page loads questions correctly with `connection.start().then(loadAll)`, which calls:
```javascript
connection.invoke('GetQuestions','Individual').then(q=>renderQuestions('Individual',q));
connection.invoke('GetQuestions','Couple').then(q=>renderQuestions('Couple',q));
```

This should work correctly. The issue might be:
1. Database is empty (no questions added yet)
2. SignalR connection not established
3. GetQuestions method failing silently

### Verification Steps

1. **Check if database has questions:**
```sql
SELECT * FROM Questions;
```

2. **Check browser console for errors:**
- Open Admin page
- Press F12
- Look for errors in Console tab

3. **Verify SignalR connection:**
```javascript
// Should see in console:
// SignalR connection established
```

### Fix Applied

I'll add better error handling and logging to the Admin page.

## Issue 2: Couple Players Button Not Clicking ?

### Root Cause
The event handler exists and looks correct:

```javascript
document.getElementById("selectCouple").addEventListener("click", () => {
    connection.invoke("SetGameMode", "Couple").then(() => {
        // Update button styles
    }).catch(err => console.error(err));
});
```

Possible causes:
1. JavaScript error preventing handler attachment
2. SignalR connection not ready
3. CSS preventing clicks (z-index, pointer-events, etc.)

### Verification Steps

1. **Check if handler is attached:**
```javascript
// In browser console:
const btn = document.getElementById("selectCouple");
console.log(btn); // Should show the button element
console.log(getEventListeners(btn)); // Should show click listener
```

2. **Try clicking programmatically:**
```javascript
document.getElementById("selectCouple").click();
```

3. **Check for CSS issues:**
```javascript
const btn = document.getElementById("selectCouple");
console.log(window.getComputedStyle(btn).pointerEvents); // Should be 'auto'
console.log(window.getComputedStyle(btn).display); // Should not be 'none'
```

### Fix Applied

I'll add defensive checks and better error handling.

---

## Complete Fix Implementation
