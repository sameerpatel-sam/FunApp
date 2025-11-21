# Quick Start: Spell the Word Game

## ?? Launch the Game

1. **Start your app** (if not running):
   ```powershell
   cd FunApp
   dotnet run
   ```

2. **Open your browser**:
   - Go to: http://localhost:5000

3. **Navigate to Spell the Word**:
   - Click the **"Spell the Word"** button in the top navigation

---

## ?? Setup Words (Before Game)

1. In the **"Manage Words"** panel on the right:
   - Type a word in the input box
   - Click **"? Add Word"**
   - Repeat for all your words

Example words for a spelling bee:
- Accommodate
- Necessary
- Entrepreneur
- Occurrence
- Rhythm

---

## ?? Play the Game

### Step 1: Reveal a Word
- Click **"?? Reveal Word"** button
- A random unrevealed word appears in large text

### Step 2: Teams Spell the Word
- Show the word to participants (via screen share or projector)
- Teams write their spelling on paper/device
- Collect and check the answers

### Step 3: Enter Scores
- In the score table, find the revealed word row
- Enter Team A's score in the Team A column
- Enter Team B's score in the Team B column
- Scores save automatically!

### Step 4: Next Round
- Click **"?? Reveal Word"** again
- Repeat steps 2-3

---

## ?? End of Game

### View Final Scores
- Check the score table for totals
- The table shows all revealed words with their scores

### Reset for Next Game
- **Option 1**: Click **"?? Reset Game"** to play again with same words
- **Option 2**: Click **"??? Clear All Scores"** to start completely fresh

---

## ?? Quick Tips

### ? Best Practices
- Add all words BEFORE starting the game
- Reveal words one at a time
- Enter scores immediately after checking

### ?? Things to Note
- This game doesn't need participants to join online
- No SignalR or user check-ins required
- Admin controls everything manually
- Perfect for in-person events

### ?? Scoring Tips
- Award points based on:
  - Correct spelling: 10 points
  - Partially correct: 5 points
  - Incorrect: 0 points
- Or use your own scoring system!

---

## ?? Controls Reference

### Word Management
| Button | Action |
|--------|--------|
| ? Add Word | Adds a new word to the list |
| Delete | Removes a word from the list |

### Game Controls
| Button | Action |
|--------|--------|
| ?? Reveal Word | Shows a random unrevealed word |
| ?? Reset Game | Marks all words as unrevealed (keeps scores) |
| ??? Clear All Scores | Removes all scores (keeps words) |

### Navigation
| Link | Destination |
|------|-------------|
| ? Back to Quiz | Returns to main quiz page |
| Manage | Goes to quiz question management |

---

## ?? Example Game Flow

```
1. Setup Phase
   ?? Add word "Dictionary"
   ?? Add word "Library"
   ?? Add word "Computer"
   ?? Add word "Necessary"

2. Round 1
   ?? Click "Reveal Word" ? Shows "Library"
   ?? Team A spells: "Libary" (wrong)
   ?? Team B spells: "Library" (correct)
   ?? Enter Team A: 0 points
   ?? Enter Team B: 10 points

3. Round 2
   ?? Click "Reveal Word" ? Shows "Computer"
   ?? Team A spells: "Computer" (correct)
   ?? Team B spells: "Computer" (correct)
   ?? Enter Team A: 10 points
   ?? Enter Team B: 10 points

4. Continue...
   ?? Reveal remaining words

5. End Game
   ?? Final Score: Team A = 10, Team B = 20
```

---

## ?? Troubleshooting

### "No more words to reveal!"
**Problem**: All words have been revealed  
**Solution**: 
- Add more words, OR
- Click "Reset Game" to reuse the same words

### Can't edit Team A/B scores
**Problem**: Word hasn't been revealed yet  
**Solution**: Click "Reveal Word" first - only revealed words have editable scores

### Scores not saving
**Problem**: API error or connection issue  
**Solution**: 
- Check browser console (F12) for errors
- Refresh the page and try again
- Make sure the app is running

---

## ?? Game Variations

### Spelling Bee
- Standard spelling competition
- 10 points for correct spelling
- 0 points for incorrect

### Definition Game
- Reveal the word
- Teams write the definition
- Score based on accuracy (0-10 points)

### Use in Sentence
- Reveal the word
- Teams write a sentence using the word
- Score based on proper usage (0-10 points)

### Speed Round
- Reveal 3 words at once
- First team to spell all correctly wins
- Award 30 points to winner

---

## ? You're Ready!

Start adding words and enjoy your **Spell the Word** game! ??

**Need help?** Check `SPELL_THE_WORD_FEATURE.md` for detailed documentation.

---

**Have fun! ??**
