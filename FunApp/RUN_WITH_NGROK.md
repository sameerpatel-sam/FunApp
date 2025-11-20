# ?? Running FunApp with ngrok - Complete Guide

## Quick Start (Easiest Method)

### Option 1: Using the Automated Script ?

```powershell
# Navigate to your project root
cd C:\Users\samee\source\repos\FunApp

# Run the script (it will auto-detect ngrok)
.\run-ngrok.ps1
```

**That's it!** The script will:
1. ? Start your ASP.NET app on port 5000
2. ? Start ngrok and create a public tunnel
3. ? Open the ngrok URL in your browser automatically

---

## Before You Start - Prerequisites

### 1. Install ngrok (if not already installed)

**Download ngrok:**
1. Go to https://ngrok.com/download
2. Download the Windows version
3. Extract `ngrok.exe` to `C:\Users\samee\Downloads\` (or anywhere you like)

**Sign up for ngrok account (FREE):**
1. Go to https://dashboard.ngrok.com/signup
2. Sign up for a free account
3. Get your authtoken from https://dashboard.ngrok.com/get-started/your-authtoken

### 2. Configure ngrok authtoken (IMPORTANT!)

```powershell
# Replace YOUR_TOKEN_HERE with your actual token from ngrok dashboard
.\run-ngrok.ps1 -NgrokAuthtoken "YOUR_TOKEN_HERE"
```

**Or configure it manually:**
```powershell
# If ngrok.exe is in Downloads folder:
~\Downloads\ngrok.exe config add-authtoken YOUR_TOKEN_HERE

# Or if ngrok is in your PATH:
ngrok config add-authtoken YOUR_TOKEN_HERE
```

---

## Running the App

### Method 1: Simple Run (Recommended)

```powershell
cd C:\Users\samee\source\repos\FunApp
.\run-ngrok.ps1
```

### Method 2: Custom Port

```powershell
.\run-ngrok.ps1 -Port 8080
```

### Method 3: Specify ngrok Location

```powershell
.\run-ngrok.ps1 -NgrokExe "C:\Tools\ngrok.exe"
```

### Method 4: Full Control

```powershell
.\run-ngrok.ps1 `
    -Port 5000 `
    -NgrokExe "C:\Users\samee\Downloads\ngrok.exe" `
    -NgrokAuthtoken "YOUR_TOKEN_HERE" `
    -StopExistingNgrok $true `
    -AutoChoosePort $true
```

---

## What Happens When You Run It

1. **Two PowerShell windows will open:**
   - Window 1: Your ASP.NET app running (`dotnet run`)
   - Window 2: ngrok tunnel forwarding traffic

2. **Your browser will open automatically** with the ngrok URL (e.g., `https://abc123.ngrok.io`)

3. **You'll see output like this:**
   ```
   Starting FunApp in folder: C:\Users\samee\source\repos\FunApp\FunApp on port 5000
   Using ngrok: C:\Users\samee\Downloads\ngrok.exe
   Launched dotnet and ngrok. Polling ngrok local API for public URL...
   ngrok public URL: https://abc123-def456.ngrok-free.app
   ```

---

## Testing Your Setup

### 1. Check if the app is running locally
Open: http://localhost:5000

You should see your FunApp homepage.

### 2. Check the ngrok public URL
The script will automatically open your ngrok URL in the browser.

You should see the same FunApp homepage, but accessible from anywhere!

### 3. Share with participants
Share the ngrok URL with quiz participants. They can join from:
- ? Their phones
- ? Other computers
- ? Anywhere with internet access

---

## Troubleshooting

### Issue 1: "ngrok not found"

**Solution:**
```powershell
# Download ngrok and specify its location
.\run-ngrok.ps1 -NgrokExe "C:\Users\samee\Downloads\ngrok.exe"
```

### Issue 2: "Port 5000 is already in use"

**Solution A - Let script auto-choose a port:**
```powershell
.\run-ngrok.ps1 -AutoChoosePort $true
```

**Solution B - Stop the existing process:**
```powershell
.\run-ngrok.ps1 -StopExistingProcess $true
```

**Solution C - Use a different port:**
```powershell
.\run-ngrok.ps1 -Port 8080
```

### Issue 3: "ngrok tunnel authentication required"

**This means you need to add your authtoken!**

```powershell
# Get your token from: https://dashboard.ngrok.com/get-started/your-authtoken
.\run-ngrok.ps1 -NgrokAuthtoken "YOUR_TOKEN_HERE"
```

### Issue 4: "ERR_NGROK_108" or connection refused

**This means your app isn't running on the expected port.**

Check the dotnet window for errors. Common causes:
- Database migration needed
- Compilation errors
- Port conflict

**Fix:**
```powershell
# First, manually test your app:
cd FunApp
dotnet build
dotnet run

# Then once it's working, run ngrok:
.\run-ngrok.ps1
```

### Issue 5: Can't see the ngrok URL

**Manually check ngrok dashboard:**
1. Open http://localhost:4040 in your browser
2. You'll see the ngrok web interface
3. Copy the "Forwarding" URL (https://xxxxx.ngrok-free.app)

### Issue 6: Score still showing 0

**This is unrelated to ngrok!** You need to restart the app:
1. Close the dotnet PowerShell window
2. Run the script again: `.\run-ngrok.ps1`

---

## Manual Setup (If Script Doesn't Work)

### Step 1: Start the App

```powershell
cd FunApp
dotnet run
```

Wait for: `Now listening on: http://localhost:5000`

### Step 2: Start ngrok (in a NEW PowerShell window)

```powershell
# If ngrok is in Downloads:
cd ~\Downloads
.\ngrok.exe http 5000

# Or if ngrok is in your PATH:
ngrok http 5000
```

### Step 3: Get the ngrok URL

In the ngrok window, you'll see:
```
Forwarding   https://abc123-def456.ngrok-free.app -> http://localhost:5000
```

Copy that URL and share it with participants!

---

## ngrok Free Plan Limits

The free plan includes:
- ? 1 online ngrok process
- ? 4 tunnels per ngrok process
- ? 40 connections/minute
- ? HTTPS forwarding
- ?? **Random URL each time** (changes when you restart)

**Good for:** Testing, small quizzes (up to ~15 participants)

**Upgrade needed if:**
- More than 40 requests/minute
- Want a custom/permanent domain
- Need more concurrent tunnels

---

## Production Tips

### For Small Quizzes (1-20 people)
? ngrok free plan works great!

### For Medium Quizzes (20-50 people)
Consider ngrok paid plan ($8/month) for:
- Custom subdomain (e.g., `yourquiz.ngrok.io`)
- Higher connection limits
- Reserved domain (doesn't change)

### For Large Quizzes (50+ people)
Deploy to a proper hosting service:
- Azure App Service
- AWS Elastic Beanstalk
- Digital Ocean
- Heroku

---

## Common Use Cases

### Use Case 1: Testing on Your Phone

1. Run: `.\run-ngrok.ps1`
2. Copy the ngrok URL (e.g., https://abc123.ngrok-free.app)
3. Open it on your phone's browser
4. Join the quiz as a participant!

### Use Case 2: Remote Participants

1. Run: `.\run-ngrok.ps1`
2. Share the ngrok URL with participants via:
   - WhatsApp
   - Email
   - Text message
   - Slack/Teams
3. They can join from anywhere!

### Use Case 3: Demo/Presentation

1. Run: `.\run-ngrok.ps1`
2. Share your screen showing the host view
3. Give participants the ngrok URL
4. They join on their devices
5. Run the quiz live!

---

## Quick Reference

### Start everything:
```powershell
.\run-ngrok.ps1
```

### Start with your ngrok token:
```powershell
.\run-ngrok.ps1 -NgrokAuthtoken "YOUR_TOKEN"
```

### Stop everything:
Close both PowerShell windows (or press Ctrl+C in each)

### Check ngrok status:
Open http://localhost:4040

### Check app status:
Open http://localhost:5000

---

## Security Notes

?? **Important Security Considerations:**

1. **The ngrok URL is PUBLIC** - Anyone with the URL can access your quiz
2. **Use unique/difficult usernames** - No password protection by default
3. **Don't share sensitive data** - This is HTTP-level security only
4. **Monitor the ngrok dashboard** - Check who's connecting at http://localhost:4040
5. **Close ngrok when done** - Don't leave it running unnecessarily

---

## Need Help?

### Check the logs:
- **App logs:** Check the dotnet PowerShell window
- **ngrok logs:** Check the ngrok PowerShell window
- **ngrok dashboard:** http://localhost:4040

### Common Commands:

```powershell
# Test if app is running:
curl http://localhost:5000/health

# Test ngrok tunnel:
curl https://your-ngrok-url.ngrok-free.app/health

# Stop a process on port 5000:
Get-Process | Where-Object {$_.Id -eq (Get-NetTCPConnection -LocalPort 5000).OwningProcess} | Stop-Process

# Find ngrok processes:
Get-Process ngrok
```

---

## Success Checklist ?

Before sharing with participants:

- [ ] App runs locally (http://localhost:5000 works)
- [ ] ngrok tunnel is active (http://localhost:4040 shows status)
- [ ] Public URL is accessible (open ngrok URL in browser)
- [ ] Game mode is selected (Individual or Couple)
- [ ] Questions are added in Admin panel
- [ ] You tested joining as a participant
- [ ] Score calculation works (if testing couples mode)

---

**You're now ready to run your quiz with remote participants! ??**

Run `.\run-ngrok.ps1` and share the ngrok URL with your participants!
