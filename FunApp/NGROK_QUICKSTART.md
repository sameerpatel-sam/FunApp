# ?? QUICK START - Run FunApp with ngrok NOW!

## Step 1: Get Your ngrok Token (One-Time Setup)

1. Go to: https://dashboard.ngrok.com/signup
2. Sign up (FREE account)
3. Copy your authtoken from: https://dashboard.ngrok.com/get-started/your-authtoken

## Step 2: Run the App

Open PowerShell in your project folder and run:

```powershell
cd C:\Users\samee\source\repos\FunApp

# Replace YOUR_TOKEN_HERE with your actual token
.\run-ngrok.ps1 -NgrokAuthtoken "YOUR_TOKEN_HERE"
```

**Or if you've already configured ngrok before:**

```powershell
.\run-ngrok.ps1
```

## Step 3: Share the URL

1. Two windows will open (dotnet app + ngrok)
2. Your browser will open with the ngrok URL (something like `https://abc123.ngrok-free.app`)
3. **Share this URL** with your quiz participants!

---

## That's It! ??

Participants can now:
- Open the ngrok URL on their phones/computers
- Join the quiz from anywhere
- Participate in real-time

---

## Troubleshooting

### "ngrok not found"
Download ngrok from https://ngrok.com/download and extract to `C:\Users\samee\Downloads\`

Then run:
```powershell
.\run-ngrok.ps1 -NgrokExe "C:\Users\samee\Downloads\ngrok.exe"
```

### "Port 5000 already in use"
```powershell
.\run-ngrok.ps1 -AutoChoosePort $true
```

### Score showing 0 (Couples mode)
**Stop the app** (close the dotnet window) and restart:
```powershell
.\run-ngrok.ps1
```

---

## Quick Commands

**Start everything:**
```powershell
.\run-ngrok.ps1
```

**Stop everything:**
Close both PowerShell windows

**Check ngrok dashboard:**
Open http://localhost:4040 in your browser

**Check if app is running:**
Open http://localhost:5000

---

## For Your Specific Setup

Since you're working on the **couples mode with score issues**, make sure to:

1. ? **Close any running instances** of your app first
2. ? **Run the script** to start fresh: `.\run-ngrok.ps1`
3. ? **Test the couples mode** with the scoring fixes
4. ? **Share the ngrok URL** with test participants

The latest fixes for the zero-score issue are already in your code - they'll take effect when you restart via this script!

---

**Need more details?** Check `RUN_WITH_NGROK.md` for the complete guide!
