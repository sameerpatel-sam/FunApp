# Spell the Word Game Feature

## Overview
A new standalone game mode that doesn't require SignalR or user check-ins. Perfect for quick team competitions where an admin can manually score teams.

## Features

### 1. Word Management
- **Add Words**: Admin can add words beforehand in the game interface
- **Delete Words**: Remove words that are no longer needed
- **Persistent Storage**: Words are stored in the database

### 2. Game Play
- **Ready State**: Shows "Ready to Spell the Word" initially
- **Reveal Word**: Button to reveal a random unrevealed word
- **Word Display**: Large, clear display of the revealed word
- **Score Table**: Shows all words with Team A and Team B scores

### 3. Scoring
- **Team A & Team B Columns**: Editable score inputs for each team
- **Real-time Updates**: Scores are saved to the database immediately
- **Only for Revealed Words**: Can only edit scores for words that have been revealed

### 4. Game Controls
- **Reset Game**: Marks all words as unrevealed (keeps words and scores)
- **Clear Scores**: Removes all score data (keeps the words)

## How to Use

### Setup (Before Game)
1. Navigate to the **Spell the Word** tab from the main page
2. In the "Manage Words" panel, add words one by one
3. Click "Add Word" after each entry

### During Game
1. Click **"Reveal Word"** to show a random unrevealed word
2. The word appears in large text for participants to spell
3. After teams spell the word, enter their scores in Team A and Team B columns
4. The word row in the score table becomes editable once revealed
5. Scores save automatically when you change them
6. Repeat: Click "Reveal Word" for the next round

### After Game
- **Reset Game**: Click to reset all words to unrevealed state (preserves scores for reference)
- **Clear Scores**: Click to remove all scores and start fresh (keeps the word list)

## Navigation

### From Main Page
Click the **"Spell the Word"** button in the top navigation bar

### From Admin Page
Click the **"?? Spell the Word"** button in the top right

### Back to Main Quiz
Click **"? Back to Quiz"** from the Spell the Word page

## Technical Details

### No SignalR Required
Unlike the Individual and Couple games, this game doesn't use SignalR for real-time updates. It's a simple admin-controlled game with manual scoring.

### Database Tables

#### SpellWords Table
- `Id` (Primary Key)
- `Word` (The word to spell)
- `IsRevealed` (Whether the word has been shown)
- `CreatedAt` (Timestamp)

#### SpellWordScores Table
- `Id` (Primary Key)
- `SpellWordId` (Foreign Key to SpellWords)
- `TeamAScore` (Score for Team A)
- `TeamBScore` (Score for Team B)
- `CreatedAt` (Timestamp)

### API Endpoints

All endpoints are under `/api/spellword/`:

- `GET /words` - Get all words with their scores
- `POST /words` - Add a new word
- `DELETE /words/{id}` - Delete a word
- `POST /reveal/{id}` - Mark a word as revealed
- `POST /score` - Update team scores for a word
- `POST /reset` - Reset all words to unrevealed
- `POST /clear-scores` - Clear all scores

## Use Cases

### Spelling Bee Competition
- Add spelling words
- Reveal one at a time
- Teams write their spelling
- Admin checks and awards points

### Vocabulary Game
- Add vocabulary words
- Reveal word
- Teams define or use in a sentence
- Admin scores responses

### Team Trivia
- Add trivia answers
- Reveal answer
- Teams write the question
- Score based on accuracy

## Design

The page uses a **blue-teal-green gradient** to distinguish it from the purple-pink-orange quiz pages.

### Layout
- **Left Column (2/3)**: Word display and score table
- **Right Column (1/3)**: Word management and game controls

### Color Scheme
- Background: Blue ? Teal ? Green gradient
- Buttons: 
  - Reveal Word: Yellow
  - Add Word: Green
  - Reset: Orange
  - Clear Scores: Red

## Future Enhancements (Optional)

1. **Timer**: Add countdown timer for each word
2. **Categories**: Group words by difficulty or category
3. **Export**: Export final scores to CSV/Excel
4. **Team Names**: Allow custom team names instead of Team A/B
5. **Multiple Teams**: Support more than 2 teams
6. **History**: Track game sessions and past results
7. **Sound Effects**: Play sounds when revealing words
8. **Projector Mode**: Full-screen view for word display

## Troubleshooting

### Words Not Showing Up
- Check browser console for API errors
- Ensure database connection is working
- Try refreshing the page

### Can't Edit Scores
- Make sure the word has been revealed first
- Unrevealed words have disabled score inputs
- Check that inputs are number type

### "No more words to reveal" Alert
- All words have been revealed
- Add more words OR
- Click "Reset Game" to mark all as unrevealed

### Scores Not Saving
- Check browser console for errors
- Verify the score update API endpoint is working
- Try entering the score again

## Credits
Created as part of the Fun App by Sameer quiz application.
