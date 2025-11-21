# Adding Spell the Word to Your Manage Module

## Changes Made

We've added a new game called **"Spell the Word"** with management capabilities directly in the game interface.

---

## Where to Find It

### 1. From Main Quiz Page (Index)
Look for the **teal "Spell the Word"** button in the top navigation bar:

```
Individual Players | Couple Players | [Spell the Word] | Manage
```

### 2. From Admin/Manage Page
Look for the **teal "?? Spell the Word"** button in the top right:

```
Quiz Management                    [?? Spell the Word] [? Close]
```

---

## What's Different About This Game

### No Separate Admin Panel Needed
Unlike Individual and Couple games which have their word management in the separate Admin page, **Spell the Word manages its words directly in the game interface**.

### Why?
- **Simpler workflow**: Add words and play in one place
- **Real-time scoring**: See scores while managing the game
- **Self-contained**: Everything you need is on one page

---

## The Manage Section (On Game Page)

Located on the **right side** of the Spell the Word page:

```
???????????????????????????????????
?      Manage Words               ?
???????????????????????????????????
? [Input: Enter a word]           ?
? [? Add Word]                    ?
?                                 ?
? Word List:                      ?
? • Accommodate      [Delete]     ?
? • Necessary        [Delete]     ?
? • Entrepreneur     [Delete]     ?
???????????????????????????????????
```

---

## How It Works

### Adding Words
1. Type a word in the "Enter a word" input
2. Click **"? Add Word"**
3. Word appears in the list below
4. Word is saved to database immediately

### Deleting Words
1. Find the word in the word list
2. Click the **Delete** button next to it
3. Word is removed from database immediately

### Playing with Words
1. Words you add appear in the score table
2. Click **"Reveal Word"** to randomly show one
3. Enter scores for Team A and Team B
4. Revealed words are tracked in the database

---

## Integration Points

### Navigation Added

#### In `Index.cshtml` (Main Quiz Page)
```html
<a href="/SpellWord" class="px-3 py-1 bg-teal-500 text-white font-semibold rounded text-sm">
    Spell the Word
</a>
```

#### In `Admin/Index.cshtml` (Manage Page)
```html
<a asp-page="/SpellWord" class="px-4 py-2 bg-teal-500 hover:bg-teal-600 rounded-lg text-sm font-semibold">
    ?? Spell the Word
</a>
```

---

## Database Tables

Two new tables were added:

### SpellWords
Stores the words for the game:
```
Id | Word          | IsRevealed | CreatedAt
1  | Accommodate   | false      | 2024-01-15
2  | Necessary     | true       | 2024-01-15
```

### SpellWordScores
Stores team scores for each word:
```
Id | SpellWordId | TeamAScore | TeamBScore | CreatedAt
1  | 2           | 10         | 5          | 2024-01-15
```

---

## Comparison with Other Games

### Individual/Couple Games
- **Question Management**: Separate Admin page
- **Playing**: Main Index page
- **Participants**: Join via SignalR
- **Scoring**: Automated based on answers

### Spell the Word Game
- **Word Management**: On the game page itself
- **Playing**: Same page as management
- **Participants**: In-person (no online join)
- **Scoring**: Manual by admin

---

## User Experience Flow

```
1. Admin Opens App
   ?? http://localhost:5000

2. Admin Navigates to Spell the Word
   ?? Clicks "Spell the Word" button

3. Admin Adds Words (Setup)
   ?? Types "Dictionary"
   ?? Clicks "Add Word"
   ?? Types "Library"
   ?? Clicks "Add Word"

4. Admin Starts Game
   ?? Clicks "Reveal Word"

5. Admin Scores Responses
   ?? Checks Team A's spelling
   ?? Enters score in Team A column
   ?? Checks Team B's spelling
   ?? Enters score in Team B column

6. Admin Continues
   ?? Clicks "Reveal Word" for next round

7. Admin Ends Game
   ?? Reviews final scores in table
```

---

## API Endpoints (For Reference)

All word and score management goes through these endpoints:

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/spellword/words` | GET | Get all words with scores |
| `/api/spellword/words` | POST | Add a new word |
| `/api/spellword/words/{id}` | DELETE | Delete a word |
| `/api/spellword/reveal/{id}` | POST | Mark word as revealed |
| `/api/spellword/score` | POST | Update team scores |
| `/api/spellword/reset` | POST | Reset all to unrevealed |
| `/api/spellword/clear-scores` | POST | Clear all scores |

These are called automatically by the JavaScript on the page.

---

## Styling

### Color Scheme
To distinguish from the main quiz (purple/pink/orange), we used:

- **Background**: Blue ? Teal ? Green gradient
- **Accent Color**: Teal (for buttons and navigation)
- **Button Colors**:
  - Reveal Word: Yellow
  - Add Word: Green  
  - Reset: Orange
  - Clear Scores: Red

---

## Future Customization Ideas

If you want to extend this feature:

1. **Category Tags**: Add categories to words (Easy, Medium, Hard)
2. **Point Values**: Assign different point values to different words
3. **Timer**: Add countdown for each word
4. **Team Names**: Let admin customize team names
5. **Export**: Add "Download Results" button
6. **Images**: Show images with words for visual learners
7. **Audio**: Play pronunciation audio for each word

---

## That's It! ??

The Spell the Word game is fully integrated into your app with its own self-contained management interface.

**Quick Access:**
- Main page ? **"Spell the Word"** button (teal)
- Admin page ? **"?? Spell the Word"** button (teal)

Enjoy your new game! ????
