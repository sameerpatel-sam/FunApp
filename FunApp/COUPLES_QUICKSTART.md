# Quick Start - Couples Game Feature

## ?? Important: Database Schema Update Required

The app has new database tables for the couples feature. Follow these steps:

### Step 1: Stop Your Running App
```powershell
# Press Ctrl+C in the terminal where the app is running
```

### Step 2: Backup Existing Data (Optional)
```powershell
# If you have important quiz data, backup the database
copy quiz.db quiz.db.backup
```

### Step 3: Delete Old Database (Recommended for Clean Start)
```powershell
# Remove old database to force recreation with new schema
del quiz.db
del quiz.db-wal
del quiz.db-shm
```

### Step 4: Restart the App
```powershell
cd FunApp
dotnet run
```

The app will automatically create the new database schema with:
- ? CoupleScores table
- ? Updated User model (FirstName, LastName, Score fields)
- ? All existing tables

---

## How to Use the Couples Feature

### Testing with Two Players (Same Last Name)

1. **Start the App**
   ```powershell
   dotnet run
   ```

2. **Select Couple Mode**
   - Open your app URL
   - Click "Couple Players" button

3. **Add Couple Questions**
   - Click "Manage" or go to `/Admin`
   - Add questions under "Couple Game Questions"
   - Example: "What is your favorite vacation spot?"

4. **Join as a Couple**
   - Open `/join` in two browser tabs/devices
   - Player 1: Enter "John Smith"
   - Player 2: Enter "Jane Smith"
   - ? System will log: "Couple formed: Mr & Mrs Smith"

5. **Play the Quiz**
   - Host clicks "Next Question"
   - Both partners answer independently
   - Try matching answers: e.g., both answer "Hawaii"
   - Host clicks "Next Question" again to evaluate

6. **View Results**
   - Click "Show Results"
   - See: "Mr & Mrs Smith - 1" (if they matched)
   - Answers shown with ? for matches

---

## Key Features

### ? Automatic Couple Detection
```
"John Smith" + "Jane Smith" = Mr & Mrs Smith (couple)
"Alice Jones" + "Bob Jones" = Mr & Mrs Jones (couple)
"Charlie Brown" alone = No couple
```

### ? Answer Matching
```
Question: "Favorite color?"
John answers: "Blue"
Jane answers: "Blue"
? Match! +1 point ?

Question: "Favorite food?"
John answers: "Pizza"
Jane answers: "Pasta"
? No match, 0 points ?
```

### ? Results Format
```
Mr & Mrs Smith - 3
  Q1: Partner 1: Blue | Partner 2: Blue ?
  Q2: Partner 1: Pizza | Partner 2: Pasta
  Q3: Partner 1: Hawaii | Partner 2: Hawaii ?
```

### ? Database Persistence
All scores saved to `CoupleScores` table:
```sql
SELECT LastName, SUM(PointsAwarded) as Score 
FROM CoupleScores 
GROUP BY LastName;
```

---

## Testing Checklist

- [ ] App starts without errors
- [ ] Can select "Couple Players" mode
- [ ] Two users with same last name join successfully
- [ ] Console shows "Couple formed" message
- [ ] Both partners can submit answers
- [ ] Matching answers award 1 point
- [ ] Non-matching answers award 0 points
- [ ] Results show "Mr & Mrs {LastName} - Score"
- [ ] Database has entries in CoupleScores table

---

## Troubleshooting

### Build Errors
**If you see compilation errors:**
1. Make sure app is stopped (Ctrl+C)
2. Run `dotnet clean`
3. Run `dotnet build`
4. Run `dotnet run`

### Database Errors
**If you see database errors:**
```
Error: no such table: CoupleScores
```
**Solution:**
```powershell
# Delete and recreate database
del quiz.db*
dotnet run
```

### Couple Not Pairing
**If users with same last name don't pair:**
- Check names have same last name exactly: "John Smith" & "Jane Smith" ?
- Last name must be LAST word: "Mary Smith Jones" & "Bob Smith Jones" ?
- Single names don't work: "Prince" & "Prince" ? (no last name)

---

## What Changed

### Code Files Modified:
1. `Models/QuizModels.cs` - Added CoupleScore, CoupleResult models
2. `Data/AppDbContext.cs` - Added CoupleScores DbSet
3. `Services/QuizService.cs` - Added couple matching and scoring logic
4. `Services/PersistentQuizService.cs` - Added couple score persistence
5. `Hubs/QuizHub.cs` - Updated to handle couple game flow
6. `Pages/Index.cshtml` - Updated results display for couples
7. `Program.cs` - Updated schema validation

### Database Changes:
- **New Table**: `CoupleScores`
- **Updated Table**: `Users` (added FirstName, LastName, Score columns)

---

## Next Steps

After successful testing:
1. Add more couple questions
2. Test with real participants
3. Check database for score persistence
4. Export results to CSV

---

## Need Help?

- Check `COUPLES_GAME_FEATURE.md` for detailed documentation
- Review application logs for error messages
- Check browser console (F12) for JavaScript errors

**Happy Quizzing! ??**
