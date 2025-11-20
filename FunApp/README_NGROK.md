# ?? FunApp with ngrok - Complete Setup Guide

Welcome! This guide will help you run your FunApp quiz with remote participants using ngrok.

---

## ?? Quick Start (3 Steps)

### 1?? Get ngrok (One-Time)
- Download: https://ngrok.com/download
- Extract `ngrok.exe` to your Downloads folder
- Get authtoken: https://dashboard.ngrok.com/get-started/your-authtoken

### 2?? Run the App
**Option A - Double-click:**
```
Double-click: start-ngrok.bat
```

**Option B - PowerShell:**
```powershell
cd C:\Users\samee\source\repos\FunApp
.\run-ngrok.ps1
```

### 3?? Share the URL
- Look for the ngrok URL in the output (e.g., `https://abc123.ngrok-free.app`)
- Share it with quiz participants
- They can join from anywhere!

---

## ?? Documentation Files

I've created several guides for different needs:

### Quick References
- **NGROK_QUICKSTART.md** - Fastest way to get started
- **NGROK_COMMANDS.md** - Copy-paste commands for all scenarios
- **start-ngrok.bat** - Just double-click to start everything

### Detailed Guides
- **RUN_WITH_NGROK.md** - Complete documentation
- **NGROK_VISUAL_GUIDE.md** - Visual diagrams and flowcharts

### For Your Specific Issue
- **ZERO_SCORE_DIAGNOSTIC.md** - About the score bug
- **TEST_ZERO_SCORE_FIX.md** - Testing the fix
- **CHECK_TERMINAL_NOW.md** - What to look for in logs

---

## ?? Usage Scenarios

### Scenario 1: Testing on Your Phone
```powershell
.\run-ngrok.ps1
# Open ngrok URL on your phone
# Join as a participant
# Test the quiz!
```

### Scenario 2: Remote Quiz with Friends
```powershell
.\run-ngrok.ps1
# Share ngrok URL via WhatsApp/text
# Friends join from their phones
# Run the quiz together!
```

### Scenario 3: Presentation/Demo
```powershell
.\run-ngrok.ps1
# Share your screen (host view)
# Give participants the ngrok URL
# They join on their devices
# Run live demo!
```

---

## ?? Common Commands

### Start Everything
```powershell
# Method 1: Batch file
.\start-ngrok.bat

# Method 2: PowerShell script
.\run-ngrok.ps1

# Method 3: With custom token
.\run-ngrok.ps1 -NgrokAuthtoken "YOUR_TOKEN"
```

### Check Status
```powershell
# Is app running?
curl http://localhost:5000/health

# Check ngrok dashboard
# Open: http://localhost:4040
```

### Stop Everything
```
Close both PowerShell windows
Or press Ctrl+C in each window
```

---

## ?? Troubleshooting

### Problem: "ngrok not found"
**Solution:**
1. Download ngrok: https://ngrok.com/download
2. Extract to Downloads folder
3. Run: `.\run-ngrok.ps1 -NgrokExe "C:\Users\samee\Downloads\ngrok.exe"`

### Problem: "Port 5000 already in use"
**Solution:**
```powershell
.\run-ngrok.ps1 -AutoChoosePort $true
```

### Problem: "ngrok authentication required"
**Solution:**
```powershell
# Get token from: https://dashboard.ngrok.com/get-started/your-authtoken
.\run-ngrok.ps1 -NgrokAuthtoken "YOUR_TOKEN"
```

### Problem: Score showing 0 (Couples mode)
**Solution:**
1. Close the dotnet window
2. Run: `.\run-ngrok.ps1` (fresh start)
3. Test again

### Problem: Can't find ngrok URL
**Solution:**
Open http://localhost:4040 - you'll see the URL there

---

## ?? What You'll See

### Two Windows Open:
1. **dotnet window** - Your ASP.NET app logs
2. **ngrok window** - Tunnel status and connections

### Browser Opens Automatically:
- Shows your ngrok URL (e.g., `https://abc123.ngrok-free.app`)
- This is what you share with participants!

### Example Output:
```
Starting FunApp in folder: C:\Users\samee\source\repos\FunApp\FunApp on port 5000
Using ngrok: C:\Users\samee\Downloads\ngrok.exe
Launched dotnet and ngrok. Polling ngrok local API for public URL...
ngrok public URL: https://abc123-def456.ngrok-free.app
Done.
```

---

## ?? For Your Couples Mode Issue

The score bug fixes are already in your code. To apply them:

1. **Stop any running app** (close dotnet window if open)
2. **Run fresh:**
   ```powershell
   .\run-ngrok.ps1
   ```
3. **Test:**
   - Select "Couple Players"
   - Join as "John Test" and "Jane Test"
   - Answer 3 questions (matching answers)
   - Click "Show Results"
   - **Score should show 3 instead of 0!**

4. **Check logs:**
   Look in the dotnet window for lines containing:
   - `[GetCoupleResults]` - Score calculation
   - `Processing couple result` - Score processing
   - `Created result object for UI` - Final score

---

## ?? Sharing with Participants

### Option 1: WhatsApp/Text
```
"Join our quiz at:
https://abc123.ngrok-free.app"
```

### Option 2: QR Code
1. Go to https://qr-code-generator.com/
2. Enter your ngrok URL
3. Generate QR code
4. Show on screen - participants scan!

### Option 3: Email
```
Subject: Join Our Quiz!

Click here to join:
https://abc123.ngrok-free.app

Instructions:
1. Open the link
2. Enter your name
3. Wait for questions
4. Have fun!
```

---

## ?? Important Notes

### Security
- ?? ngrok URL is PUBLIC - anyone with it can join
- ?? Don't share sensitive data
- ?? Close ngrok when done
- ? Monitor connections at http://localhost:4040

### URL Changes
- ?? Free ngrok URL changes every time you restart
- ?? Need to share new URL each session
- ? Upgrade to ngrok Pro for permanent URL ($8/month)

### Free Plan Limits
- ? 40 connections/minute
- ? 1 online ngrok process
- ? HTTPS forwarding
- ?? Good for up to ~15-20 participants

---

## ?? Complete Workflow Example

```
1. YOU: Run .\run-ngrok.ps1
   ? Two windows open
   ? Browser opens with ngrok URL

2. YOU: Copy the ngrok URL
   ? Example: https://abc123.ngrok-free.app

3. YOU: Go to the URL, click "Couple Players"
   ? Select game mode

4. YOU: Go to /admin, add 3 questions
   ? Prepare the quiz

5. YOU: Share ngrok URL with participants
   ? Via WhatsApp, text, etc.

6. PARTICIPANTS: Open ngrok URL on phones
   ? Join as couples (same last name)

7. PARTICIPANTS: Wait on join page
   ? See "Waiting for questions..."

8. YOU: Click "Next Question"
   ? Question appears for everyone

9. PARTICIPANTS: Submit answers
   ? Type their answers

10. YOU: Click "Next Question" again
    ? Repeat for all questions

11. YOU: Click "Show Results"
    ? Scores appear!
    ? Share screen to show results
```

---

## ?? Need Help?

### Check These First:
1. **Is the app running?** ? Check dotnet window for errors
2. **Is ngrok running?** ? Check ngrok window
3. **Can you access locally?** ? Try http://localhost:5000
4. **Is the URL correct?** ? Check http://localhost:4040

### Common Issues & Fixes:
- **App crashes** ? Check database, run migrations
- **ngrok fails** ? Add authtoken
- **Port busy** ? Use `-AutoChoosePort $true`
- **Score is 0** ? Restart app (close dotnet window and run script again)

### Get More Help:
- Check **RUN_WITH_NGROK.md** for detailed troubleshooting
- Check **NGROK_VISUAL_GUIDE.md** for diagrams
- Check terminal logs for error messages

---

## ? Pre-Flight Checklist

Before sharing with participants:

- [ ] ngrok authtoken configured
- [ ] App runs locally (http://localhost:5000 works)
- [ ] ngrok tunnel active (http://localhost:4040 shows status)
- [ ] Public URL accessible (open ngrok URL in browser)
- [ ] Game mode selected (Individual or Couple)
- [ ] Questions added in Admin panel
- [ ] Tested joining as a participant
- [ ] Score calculation works (for couples mode)

---

## ?? You're Ready!

Run this command to start everything:

```powershell
cd C:\Users\samee\source\repos\FunApp
.\run-ngrok.ps1
```

Then share the ngrok URL with your participants!

**Good luck with your quiz! ??**

---

## ?? Quick Links

- ngrok Download: https://ngrok.com/download
- ngrok Dashboard: https://dashboard.ngrok.com/
- Get Authtoken: https://dashboard.ngrok.com/get-started/your-authtoken
- ngrok Documentation: https://ngrok.com/docs

---

**Questions? Check the other markdown files for more details!**
