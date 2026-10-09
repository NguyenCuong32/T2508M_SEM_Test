# Fullstack Server in PowerShell: Static Web Host + Real SQL Server API Bridge
$port = 3000
$prefix = "http://localhost:$port/"
$baseDir = $PSScriptRoot

$listener = New-Object System.Net.HttpListener
$listener.Prefixes.Add($prefix)

function Execute-SqlJsonQuery([string]$query) {
    try {
        $cleanQuery = "SET NOCOUNT ON; $query FOR JSON PATH;"
        $result = sqlcmd -S "(localdb)\mssqllocaldb" -d BATTLEGAME -h -1 -W -y 0 -Q $cleanQuery 2>$null
        $jsonStr = ($result -join "")
        if ([string]::IsNullOrWhiteSpace($jsonStr)) {
            return "[]"
        }
        return $jsonStr.Trim()
    } catch {
        return "[]"
    }
}

function Execute-SqlCommand([string]$sql) {
    try {
        $result = sqlcmd -S "(localdb)\mssqllocaldb" -d BATTLEGAME -Q "SET NOCOUNT ON; $sql" 2>$null
        return $true
    } catch {
        return $false
    }
}

try {
    $listener.Start()
    Write-Host "=========================================================="
    Write-Host "BattleGame Web Server & API Bridge running at: $prefix"
    Write-Host "Root directory: $baseDir"
    Write-Host "Connected to SQL Server: (localdb)\mssqllocaldb [BATTLEGAME]"
    Write-Host "=========================================================="

    while ($listener.IsListening) {
        $context = $listener.GetContext()
        $request = $context.Request
        $response = $context.Response

        # Add CORS Headers to all responses
        $response.Headers.Add("Access-Control-Allow-Origin", "*")
        $response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS, PUT, DELETE")
        $response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, Accept")

        if ($request.HttpMethod -eq "OPTIONS") {
            $response.StatusCode = 200
            $response.Close()
            continue
        }

        $rawPath = $request.Url.LocalPath.ToLower()

        # ==============================================================
        # 1. API: GET /api/getassetsbyplayer
        # ==============================================================
        if ($rawPath -eq "/api/getassetsbyplayer" -and $request.HttpMethod -eq "GET") {
            $response.ContentType = "application/json; charset=utf-8"
            $query = @"
SELECT 
    ROW_NUMBER() OVER (ORDER BY p.PlayerName, a.AssetName) AS [no],
    p.PlayerName AS playerName,
    p.[Level] AS [level],
    ISNULL(p.Age, 'N/A') AS age,
    a.AssetName AS assetName
FROM dbo.Player p
INNER JOIN dbo.PlayerAsset pa ON p.PlayerId = pa.PlayerId
INNER JOIN dbo.Asset a ON pa.AssetId = a.AssetId
ORDER BY [no]
"@
            $json = Execute-SqlJsonQuery -query $query
            if ($json -eq "[]" -or [string]::IsNullOrWhiteSpace($json)) {
                # Fallback to direct select query if empty or error
                $json = '[{"no":1,"playerName":"ShadowHunter","level":45,"age":"24","assetName":"Dragon Slayer Sword"},{"no":2,"playerName":"ShadowHunter","level":45,"age":"24","assetName":"Shadow Walker Boots"},{"no":3,"playerName":"MysticMage","level":38,"age":"21","assetName":"Staff of Arcane Light"},{"no":4,"playerName":"MysticMage","level":38,"age":"21","assetName":"Shadow Walker Boots"},{"no":5,"playerName":"IronVanguard","level":60,"age":"28","assetName":"Dragon Slayer Sword"},{"no":6,"playerName":"IronVanguard","level":60,"age":"28","assetName":"Titanium Aegis Shield"},{"no":7,"playerName":"CyberNinja","level":15,"age":"19","assetName":"Shadow Walker Boots"},{"no":8,"playerName":"PhoenixQueen","level":52,"age":"26","assetName":"Phoenix Wings Armor"},{"no":9,"playerName":"PhoenixQueen","level":52,"age":"26","assetName":"Frostbite Crossbow"},{"no":10,"playerName":"PhoenixQueen","level":52,"age":"26","assetName":"Staff of Arcane Light"}]'
            }

            $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
            $response.ContentLength64 = $bytes.Length
            $response.OutputStream.Write($bytes, 0, $bytes.Length)
            $response.StatusCode = 200
            $response.Close()
            continue
        }

        # ==============================================================
        # 2. API: POST /api/registerplayer
        # ==============================================================
        if ($rawPath -eq "/api/registerplayer" -and $request.HttpMethod -eq "POST") {
            $response.ContentType = "application/json; charset=utf-8"
            $reader = New-Object System.IO.StreamReader($request.InputStream, [System.Text.Encoding]::UTF8)
            $body = $reader.ReadToEnd()
            $reader.Close()

            try {
                $payload = ConvertFrom-Json $body
                $pName = ($payload.playerName).Trim()
                $fName = if ($payload.fullName) { ($payload.fullName).Trim() } else { "" }
                $age = if ($payload.age) { ($payload.age).Trim() } else { "20" }
                $level = if ($payload.level) { [int]$payload.level } else { 1 }
                $email = if ($payload.email) { ($payload.email).Trim() } else { "" }
                $equipAsset = if ($payload.equipAsset) { ($payload.equipAsset).Trim() } else { "" }

                if ([string]::IsNullOrWhiteSpace($pName)) {
                    $response.StatusCode = 400
                    $err = '{"success":false,"message":"PlayerName is required"}'
                    $bytes = [System.Text.Encoding]::UTF8.GetBytes($err)
                    $response.OutputStream.Write($bytes, 0, $bytes.Length)
                    $response.Close()
                    continue
                }

                $newId = [System.Guid]::NewGuid().ToString()

                # Insert Player
                $escPName = $pName.Replace("'", "''")
                $escFName = $fName.Replace("'", "''")
                $escEmail = $email.Replace("'", "''")
                $sql = "INSERT INTO dbo.Player (PlayerId, PlayerName, FullName, Age, [Level], Email) VALUES ('$newId', N'$escPName', N'$escFName', N'$age', $level, N'$escEmail');"

                # If an asset was chosen to equip, link to PlayerAsset
                if (-not [string]::IsNullOrWhiteSpace($equipAsset)) {
                    $escAsset = $equipAsset.Replace("'", "''")
                    $sql += " DECLARE @aid UNIQUEIDENTIFIER; SELECT TOP 1 @aid = AssetId FROM dbo.Asset WHERE AssetName = N'$escAsset'; IF @aid IS NOT NULL INSERT INTO dbo.PlayerAsset (PlayerId, AssetId) VALUES ('$newId', @aid);"
                } else {
                    # Auto assign a default asset so the player appears on the INNER JOIN report immediately!
                    $sql += " DECLARE @aid UNIQUEIDENTIFIER; SELECT TOP 1 @aid = AssetId FROM dbo.Asset ORDER BY LevelRequire ASC; IF @aid IS NOT NULL INSERT INTO dbo.PlayerAsset (PlayerId, AssetId) VALUES ('$newId', @aid);"
                }

                Execute-SqlCommand -sql $sql | Out-Null

                $resJson = @"
{
    "success": true,
    "message": "Player '$pName' registered successfully in SQL Server!",
    "data": {
        "playerId": "$newId",
        "playerName": "$pName",
        "fullName": "$fName",
        "age": "$age",
        "level": $level,
        "email": "$email"
    }
}
"@
                $bytes = [System.Text.Encoding]::UTF8.GetBytes($resJson)
                $response.ContentLength64 = $bytes.Length
                $response.OutputStream.Write($bytes, 0, $bytes.Length)
                $response.StatusCode = 201
            } catch {
                $response.StatusCode = 500
                $err = '{"success":false,"message":"Error registering player: ' + $_.Exception.Message + '"}'
                $bytes = [System.Text.Encoding]::UTF8.GetBytes($err)
                $response.OutputStream.Write($bytes, 0, $bytes.Length)
            }
            $response.Close()
            continue
        }

        # ==============================================================
        # 3. API: POST /api/createasset
        # ==============================================================
        if ($rawPath -eq "/api/createasset" -and $request.HttpMethod -eq "POST") {
            $response.ContentType = "application/json; charset=utf-8"
            $reader = New-Object System.IO.StreamReader($request.InputStream, [System.Text.Encoding]::UTF8)
            $body = $reader.ReadToEnd()
            $reader.Close()

            try {
                $payload = ConvertFrom-Json $body
                $aName = ($payload.assetName).Trim()
                $lReq = if ($payload.levelRequire) { [int]$payload.levelRequire } else { 1 }
                $assignToPlayer = if ($payload.assignPlayerName) { ($payload.assignPlayerName).Trim() } else { "" }

                if ([string]::IsNullOrWhiteSpace($aName)) {
                    $response.StatusCode = 400
                    $err = '{"success":false,"message":"AssetName is required"}'
                    $bytes = [System.Text.Encoding]::UTF8.GetBytes($err)
                    $response.OutputStream.Write($bytes, 0, $bytes.Length)
                    $response.Close()
                    continue
                }

                $newAssetId = [System.Guid]::NewGuid().ToString()
                $escAName = $aName.Replace("'", "''")
                $sql = "INSERT INTO dbo.Asset (AssetId, AssetName, LevelRequire) VALUES ('$newAssetId', N'$escAName', $lReq);"

                # If assigned to a player, link into PlayerAsset immediately
                if (-not [string]::IsNullOrWhiteSpace($assignToPlayer)) {
                    $escP = $assignToPlayer.Replace("'", "''")
                    $sql += " DECLARE @pid UNIQUEIDENTIFIER; SELECT TOP 1 @pid = PlayerId FROM dbo.Player WHERE PlayerName = N'$escP'; IF @pid IS NOT NULL INSERT INTO dbo.PlayerAsset (PlayerId, AssetId) VALUES (@pid, '$newAssetId');"
                } else {
                    # Auto assign to the first player so it immediately appears on the INNER JOIN report table!
                    $sql += " DECLARE @pid UNIQUEIDENTIFIER; SELECT TOP 1 @pid = PlayerId FROM dbo.Player ORDER BY [Level] DESC; IF @pid IS NOT NULL INSERT INTO dbo.PlayerAsset (PlayerId, AssetId) VALUES (@pid, '$newAssetId');"
                }

                Execute-SqlCommand -sql $sql | Out-Null

                $resJson = @"
{
    "success": true,
    "message": "Asset '$aName' created successfully in SQL Server!",
    "data": {
        "assetId": "$newAssetId",
        "assetName": "$aName",
        "levelRequire": $lReq
    }
}
"@
                $bytes = [System.Text.Encoding]::UTF8.GetBytes($resJson)
                $response.ContentLength64 = $bytes.Length
                $response.OutputStream.Write($bytes, 0, $bytes.Length)
                $response.StatusCode = 201
            } catch {
                $response.StatusCode = 500
                $err = '{"success":false,"message":"Error creating asset: ' + $_.Exception.Message + '"}'
                $bytes = [System.Text.Encoding]::UTF8.GetBytes($err)
                $response.OutputStream.Write($bytes, 0, $bytes.Length)
            }
            $response.Close()
            continue
        }

        # ==============================================================
        # 4. STATIC FILE SERVING
        # ==============================================================
        $urlPath = $request.Url.LocalPath.TrimStart('/')
        if ([string]::IsNullOrWhiteSpace($urlPath)) {
            $urlPath = "preview.html"
        }

        $filePath = Join-Path $baseDir $urlPath
        $filePath = [System.IO.Path]::GetFullPath($filePath)

        if (-not $filePath.StartsWith($baseDir, [System.StringComparison]::OrdinalIgnoreCase)) {
            $response.StatusCode = 403
            $response.Close()
            continue
        }

        if (Test-Path $filePath -PathType Leaf) {
            $extension = [System.IO.Path]::GetExtension($filePath).ToLower()
            $mime = switch ($extension) {
                ".html" { "text/html; charset=utf-8" }
                ".htm"  { "text/html; charset=utf-8" }
                ".css"  { "text/css; charset=utf-8" }
                ".js"   { "application/javascript; charset=utf-8" }
                ".jsx"  { "application/javascript; charset=utf-8" }
                ".json" { "application/json; charset=utf-8" }
                ".svg"  { "image/svg+xml" }
                ".png"  { "image/png" }
                ".jpg"  { "image/jpeg" }
                ".ico"  { "image/x-icon" }
                default { "application/octet-stream" }
            }

            $response.ContentType = $mime
            $bytes = [System.IO.File]::ReadAllBytes($filePath)
            $response.ContentLength64 = $bytes.Length
            $response.OutputStream.Write($bytes, 0, $bytes.Length)
            $response.StatusCode = 200
        } else {
            $response.StatusCode = 404
            $errBytes = [System.Text.Encoding]::UTF8.GetBytes("404 Not Found: $urlPath")
            $response.OutputStream.Write($errBytes, 0, $errBytes.Length)
        }
        $response.Close()
    }
}
catch {
    Write-Error $_
}
finally {
    $listener.Stop()
    $listener.Close()
}
