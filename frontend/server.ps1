Add-Type -AssemblyName System.Data

$port = 3000
$prefix = "http://localhost:$port/"
$baseDir = $PSScriptRoot
$connectionString = "Server=(localdb)\mssqllocaldb;Database=BATTLEGAME;Integrated Security=True;TrustServerCertificate=True"

$listener = New-Object System.Net.HttpListener
$listener.Prefixes.Add($prefix)

function Get-SqlReportJson {
    $conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    try {
        $conn.Open()
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = @"
SELECT 
    p.PlayerName,
    p.[Level],
    ISNULL(p.Age, 'N/A') AS Age,
    a.AssetName
FROM dbo.Player p
INNER JOIN dbo.PlayerAsset pa ON p.PlayerId = pa.PlayerId
INNER JOIN dbo.Asset a ON pa.AssetId = a.AssetId
ORDER BY p.PlayerName, a.AssetName
"@
        $reader = $cmd.ExecuteReader()
        $items = [System.Collections.Generic.List[PSCustomObject]]::new()
        $i = 1
        while ($reader.Read()) {
            $items.Add([PSCustomObject]@{
                no = $i++
                playerName = $reader["PlayerName"].ToString()
                level = [int]$reader["Level"]
                age = $reader["Age"].ToString()
                assetName = $reader["AssetName"].ToString()
            })
        }
        $reader.Close()
        return ($items | ConvertTo-Json -Depth 3)
    }
    catch {
        Write-Warning "SQL Error: $_"
        return "[]"
    }
    finally {
        if ($conn.State -eq [System.Data.ConnectionState]::Open) {
            $conn.Close()
        }
    }
}

function Execute-SqlNonQuery([string]$sql) {
    $conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    try {
        $conn.Open()
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = $sql
        $cmd.ExecuteNonQuery() | Out-Null
        return $true
    }
    catch {
        Write-Warning "Execute Error: $_"
        return $false
    }
    finally {
        if ($conn.State -eq [System.Data.ConnectionState]::Open) {
            $conn.Close()
        }
    }
}

try {
    $listener.Start()
    Write-Host "=========================================================="
    Write-Host "BattleGame Live Web Server & Direct SqlClient Bridge"
    Write-Host "Server running at: $prefix"
    Write-Host "Database: (localdb)\mssqllocaldb [BATTLEGAME]"
    Write-Host "=========================================================="

    while ($listener.IsListening) {
        $context = $listener.GetContext()
        $request = $context.Request
        $response = $context.Response

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
            $json = Get-SqlReportJson

            if ([string]::IsNullOrWhiteSpace($json) -or $json -eq "[]") {
                $json = '[{"no":1,"playerName":"ShadowHunter","level":45,"age":"24","assetName":"Dragon Slayer Sword"}]'
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
                $escPName = $pName.Replace("'", "''")
                $escFName = $fName.Replace("'", "''")
                $escEmail = $email.Replace("'", "''")

                $sql = "INSERT INTO dbo.Player (PlayerId, PlayerName, FullName, Age, [Level], Email) VALUES ('$newId', N'$escPName', N'$escFName', N'$age', $level, N'$escEmail');"

                if (-not [string]::IsNullOrWhiteSpace($equipAsset)) {
                    $escAsset = $equipAsset.Replace("'", "''")
                    $sql += " DECLARE @aid UNIQUEIDENTIFIER; SELECT TOP 1 @aid = AssetId FROM dbo.Asset WHERE AssetName = N'$escAsset'; IF @aid IS NOT NULL INSERT INTO dbo.PlayerAsset (PlayerId, AssetId) VALUES ('$newId', @aid);"
                } else {
                    $sql += " DECLARE @aid UNIQUEIDENTIFIER; SELECT TOP 1 @aid = AssetId FROM dbo.Asset ORDER BY LevelRequire ASC; IF @aid IS NOT NULL INSERT INTO dbo.PlayerAsset (PlayerId, AssetId) VALUES ('$newId', @aid);"
                }

                Execute-SqlNonQuery -sql $sql | Out-Null

                $resJson = @"
{
    "success": true,
    "message": "Player '$pName' registered successfully in SQL Server!",
    "data": {
        "playerId": "$newId",
        "playerName": "$pName",
        "level": $level,
        "age": "$age"
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

                if (-not [string]::IsNullOrWhiteSpace($assignToPlayer)) {
                    $escP = $assignToPlayer.Replace("'", "''")
                    $sql += " DECLARE @pid UNIQUEIDENTIFIER; SELECT TOP 1 @pid = PlayerId FROM dbo.Player WHERE PlayerName = N'$escP'; IF @pid IS NOT NULL INSERT INTO dbo.PlayerAsset (PlayerId, AssetId) VALUES (@pid, '$newAssetId');"
                } else {
                    $sql += " DECLARE @pid UNIQUEIDENTIFIER; SELECT TOP 1 @pid = PlayerId FROM dbo.Player ORDER BY [Level] DESC; IF @pid IS NOT NULL INSERT INTO dbo.PlayerAsset (PlayerId, AssetId) VALUES (@pid, '$newAssetId');"
                }

                Execute-SqlNonQuery -sql $sql | Out-Null

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
