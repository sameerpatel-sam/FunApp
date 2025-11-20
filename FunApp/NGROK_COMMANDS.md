# Copy-Paste Commands for ngrok Setup

## Option 1: First Time Setup (With Token)

```powershell
# 1. Navigate to project
cd C:\Users\samee\source\repos\FunApp

# 2. Run with your ngrok token (get it from https://dashboard.ngrok.com/get-started/your-authtoken)
.\run-ngrok.ps1 -NgrokAuthtoken "YOUR_NGROK_TOKEN_HERE"
```

## Option 2: Already Configured ngrok

```powershell
# Just run the script
cd C:\Users\samee\source\repos\FunApp
.\run-ngrok.ps1
```

## Option 3: If ngrok is in Downloads Folder

```powershell
cd C:\Users\samee\source\repos\FunApp
.\run-ngrok.ps1 -NgrokExe "C:\Users\samee\Downloads\ngrok.exe"
```

## Option 4: Custom Port

```powershell
cd C:\Users\samee\source\repos\FunApp
.\run-ngrok.ps1 -Port 8080
```

## Option 5: Auto-Choose Port if 5000 is Busy

```powershell
cd C:\Users\samee\source\repos\FunApp
.\run-ngrok.ps1 -AutoChoosePort $true
```

## Option 6: Stop Any Existing Processes First

```powershell
cd C:\Users\samee\source\repos\FunApp
.\run-ngrok.ps1 -StopExistingProcess $true -StopExistingNgrok $true
```

---

## If You Don't Have ngrok Installed Yet

### Download ngrok:
1. Go to: https://ngrok.com/download
2. Download Windows version
3. Extract ngrok.exe to `C:\Users\samee\Downloads\`

### Get your authtoken:
1. Sign up (free): https://dashboard.ngrok.com/signup
2. Get token: https://dashboard.ngrok.com/get-started/your-authtoken
3. Copy the token

### Then run:
```powershell
cd C:\Users\samee\source\repos\FunApp
.\run-ngrok.ps1 -NgrokAuthtoken "PASTE_YOUR_TOKEN_HERE"
```

---

## What You'll See

### Terminal Output:
```
Starting FunApp in folder: C:\Users\samee\source\repos\FunApp\FunApp on port 5000
Using ngrok: C:\Users\samee\Downloads\ngrok.exe
Launched dotnet and ngrok. Polling ngrok local API for public URL...
ngrok public URL: https://abc123-def456.ngrok-free.app
```

### Two Windows Will Open:
1. **dotnet window** - Your ASP.NET app running
2. **ngrok window** - The tunnel showing connection status

### Your Browser Will Open:
- Automatically opens the ngrok URL
- You'll see your FunApp homepage
- Share this URL with participants!

---

## Quick Checks

### Is the app running?
```powershell
curl http://localhost:5000/health
```

### Is ngrok running?
Open: http://localhost:4040 (ngrok web dashboard)

### Get the ngrok URL again:
Check the ngrok PowerShell window or go to http://localhost:4040

---

## Stop Everything

Just close both PowerShell windows that opened!

Or press `Ctrl+C` in each window.

---

## Example Complete Flow

```powershell
# 1. Navigate to project
cd C:\Users\samee\source\repos\FunApp

# 2. Run the script
.\run-ngrok.ps1

# 3. Wait for the URL to appear
# Output: ngrok public URL: https://abc123.ngrok-free.app

# 4. Share the URL with participants
# They can access: https://abc123.ngrok-free.app

# 5. You open the admin/host view:
# https://abc123.ngrok-free.app
# (Click "Couple Players", add questions, etc.)

# 6. Participants join:
# https://abc123.ngrok-free.app/join
```

---

## Troubleshooting Commands

### Port 5000 is busy - find what's using it:
```powershell
Get-NetTCPConnection -LocalPort 5000 | Select-Object OwningProcess
Get-Process -Id (Get-NetTCPConnection -LocalPort 5000).OwningProcess
```

### Kill process on port 5000:
```powershell
Stop-Process -Id (Get-NetTCPConnection -LocalPort 5000).OwningProcess -Force
```

### Find all ngrok processes:
```powershell
Get-Process ngrok
```

### Stop all ngrok processes:
```powershell
Get-Process ngrok | Stop-Process -Force
```

### Check if dotnet is running:
```powershell
Get-Process dotnet
```

---

## For Your Zero-Score Issue

**IMPORTANT:** The fixes are already in your code. To apply them:

```powershell
# 1. Stop any running app (close the dotnet window if open)

# 2. Navigate to project
cd C:\Users\samee\source\repos\FunApp

# 3. Run fresh with ngrok
.\run-ngrok.ps1

# 4. Test couples mode
# - Join as "John Test" and "Jane Test"
# - Answer 3 questions (matching answers)
# - Click "Show Results"
# - Score should now show 3 instead of 0!

# 5. Check terminal logs for diagnostic output
# Look for lines with [GetCoupleResults] to see score calculation
```

---

**Ready to go? Copy the command that matches your setup and run it!** ??
