param(
    [int]$Port = 5000,
    [string]$NgrokExe = "$env:USERPROFILE\Downloads\ngrok.exe",
    [string]$ProjectPath = (Join-Path $PSScriptRoot 'FunApp'),
    [string]$NgrokAuthtoken = '',
    [switch]$StopExistingNgrok = $true,
    [switch]$EnablePooling = $false,
    [switch]$AutoChoosePort = $true,
    [switch]$StopExistingProcess = $false,
    [int]$NgrokApiPollSeconds = 1,
    [int]$NgrokApiTimeoutSeconds = 30,
    [int]$AppStartupTimeoutSeconds = 20
)

function Find-Ngrok {
    param([string]$Preferred)

    if ($Preferred -and (Test-Path $Preferred)) { return (Resolve-Path $Preferred).ProviderPath }

    $cmd = Get-Command ngrok -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    $candidate = "$env:USERPROFILE\Downloads\ngrok.exe"
    if (Test-Path $candidate) { return (Resolve-Path $candidate).ProviderPath }

    try {
        $found = Get-ChildItem -Path $env:USERPROFILE -Filter ngrok.exe -Recurse -ErrorAction SilentlyContinue -Force | Select-Object -First 1
        if ($found) { return $found.FullName }
    } catch {
        # ignore
    }

    return $null
}

function Test-PortAvailable {
    param([int]$p)
    try {
        $listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, $p)
        $listener.Start()
        $listener.Stop()
        return $true
    } catch {
        return $false
    }
}

function Get-FreePort {
    $listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, 0)
    $listener.Start()
    try {
        $endpoint = $listener.LocalEndpoint
        $port = $endpoint.Port
    } finally {
        $listener.Stop()
    }
    return $port
}

function Get-ProcessOwningPort {
    param([int]$p)
    try {
        $conn = Get-NetTCPConnection -LocalPort $p -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($conn) {
            $pid = $conn.OwningProcess
            return Get-Process -Id $pid -ErrorAction SilentlyContinue
        }
    } catch {
        # fallback to netstat
        $line = netstat -aon | Select-String ":$p" | Select-Object -First 1
        if ($line) {
            $pid = ($line -split '\s+')[-1]
            return Get-Process -Id $pid -ErrorAction SilentlyContinue
        }
    }
    return $null
}

# prerequisites
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error "dotnet not found on PATH. Install .NET SDK 8 and try again."
    exit 1
}

# Resolve ngrok path: prefer explicit path, then PATH, then common locations
$resolvedNgrok = $null
if (Test-Path $NgrokExe) {
    $resolvedNgrok = (Resolve-Path $NgrokExe).ProviderPath
} else {
    $resolvedNgrok = Find-Ngrok -Preferred $NgrokExe
}

if (-not $resolvedNgrok) {
    Write-Error "ngrok not found. Provide path with -NgrokExe or install ngrok and ensure it's on PATH."
    exit 1
}

if (-not (Test-Path $ProjectPath)) {
    Write-Error "Project folder not found: $ProjectPath`nAdjust ProjectPath parameter to point to the folder that contains FunApp.csproj." 
    exit 1
}

# port handling
if (-not (Test-PortAvailable -p $Port)) {
    $owner = Get-ProcessOwningPort -p $Port
    if ($owner) {
        Write-Warning "Port $Port is already in use by PID $($owner.Id) ($($owner.ProcessName))."
        if ($StopExistingProcess) {
            Write-Host "Stopping process $($owner.Id) ($($owner.ProcessName))..."
            try { Stop-Process -Id $owner.Id -Force; Start-Sleep -Seconds 1; Write-Host 'Stopped.' } catch { Write-Warning 'Failed to stop process.' }
            # Re-test
            if (-not (Test-PortAvailable -p $Port)) { Write-Warning "Port $Port still in use after attempting stop." }
        }
    } else {
        Write-Warning "Port $Port appears to be in use but owning process could not be determined."
    }

    if ($AutoChoosePort) {
        $free = Get-FreePort
        Write-Host "Auto-selecting free port $free instead of $Port."
        $Port = $free
    } else {
        Write-Error "Port $Port is not available. Re-run with -AutoChoosePort to pick a free port or free the port and retry."
        exit 1
    }
}

Write-Host "Starting FunApp in folder: $ProjectPath on port $Port"
Write-Host "Using ngrok: $resolvedNgrok"

# stop existing ngrok processes if requested
$existingNgrok = Get-Process -Name ngrok -ErrorAction SilentlyContinue
if ($existingNgrok -and $StopExistingNgrok) {
    Write-Host "Stopping existing ngrok process(es)..."
    try { $existingNgrok | Stop-Process -Force -ErrorAction Stop; Start-Sleep -Seconds 1; Write-Host "Stopped existing ngrok process(es)." } catch { Write-Warning "Failed to stop existing ngrok processes." }
}

# apply authtoken if provided
if ($NgrokAuthtoken -and $NgrokAuthtoken.Trim() -ne '') {
    Write-Host "Applying ngrok authtoken..."
    try {
        & $resolvedNgrok config add-authtoken $NgrokAuthtoken 2>$null
        if ($LASTEXITCODE -ne 0) { & $resolvedNgrok authtoken $NgrokAuthtoken 2>$null }
        Write-Host "ngrok authtoken applied (or command attempted)."
    } catch {
        Write-Warning "Failed to apply ngrok authtoken automatically. Run: $resolvedNgrok config add-authtoken <token>"
    }
} else {
    Write-Host "No ngrok authtoken provided. If required, supply -NgrokAuthtoken '<token>' when running the script."
}

# Start the ASP.NET app in a new PowerShell window
# Build command using concatenation to avoid nested-quote parsing issues
$dotnetCommand = 'cd "' + $ProjectPath + '"; dotnet restore; $env:ASPNETCORE_URLS="http://0.0.0.0:' + $Port + '"; Write-Host "ASPNETCORE_URLS=$env:ASPNETCORE_URLS"; dotnet run'
Start-Process -FilePath powershell -ArgumentList ('-NoExit','-Command', $dotnetCommand)

# wait for health endpoint
$healthUrl = "http://127.0.0.1:$Port/health"
$startupDeadline = [DateTime]::UtcNow.AddSeconds($AppStartupTimeoutSeconds)
$up = $false
Write-Host "Waiting up to $AppStartupTimeoutSeconds seconds for $healthUrl to respond..."
while (([DateTime]::UtcNow) -lt $startupDeadline) {
    try {
        $r = Invoke-RestMethod -Uri $healthUrl -Method Get -TimeoutSec 2 -ErrorAction Stop
        if ($r -ne $null) { $up = $true; break }
    } catch {
        Start-Sleep -Seconds 1
    }
}

if (-not $up) {
    Write-Warning "Local app did not respond at $healthUrl. Check the dotnet window for errors; ngrok will fail to forward until the app is listening."
    Write-Host "Starting ngrok anyway so you can inspect output..."
}

Start-Sleep -Seconds 1

# start ngrok pointing to 127.0.0.1 to avoid IPv6 issues
$poolingFlag = ''
if ($EnablePooling) { $poolingFlag = '--pooling-enabled' }
$ngrokDir = Split-Path -Parent $resolvedNgrok
$ngrokCommand = "cd '$ngrokDir'; & '$resolvedNgrok' http 127.0.0.1:$Port $poolingFlag"
Start-Process -FilePath powershell -ArgumentList ('-NoExit','-Command', $ngrokCommand)

Write-Host "Launched dotnet and ngrok. Polling ngrok local API for public URL (this may take a few seconds)..."

# poll ngrok api
$apiUrl = 'http://127.0.0.1:4040/api/tunnels'
$timeout = [DateTime]::UtcNow.AddSeconds($NgrokApiTimeoutSeconds)
$publicUrl = $null
while (([DateTime]::UtcNow) -lt $timeout) {
    try {
        $resp = Invoke-RestMethod -Uri $apiUrl -Method Get -ErrorAction Stop
        if ($resp.tunnels -and $resp.tunnels.Count -gt 0) {
            $tunnel = $resp.tunnels | Where-Object { $_.proto -eq 'https' } | Select-Object -First 1
            if (-not $tunnel) { $tunnel = $resp.tunnels[0] }
            $publicUrl = $tunnel.public_url
            break
        }
    } catch {
        Start-Sleep -Seconds $NgrokApiPollSeconds
    }
}

if ($publicUrl) {
    Write-Host "ngrok public URL: $publicUrl"
    Start-Process $publicUrl
} else {
    Write-Warning "Timed out waiting for ngrok API. You can check the ngrok window for forwarding URLs or run: $resolvedNgrok http 127.0.0.1:$Port"
}

Write-Host "Done."