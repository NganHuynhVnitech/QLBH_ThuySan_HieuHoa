param(
    [string]$sourceDir = ".\publish_matbao",
    [string]$ftpServer = "ftp://112.78.2.68",
    [string]$ftpFolder = "/mbtest.vnitech.vn",
    [string]$ftpUser = "xang4739",
    [string]$ftpPass = "Vnitech113@"
)

$webclient = New-Object System.Net.WebClient
$webclient.Credentials = New-Object System.Net.NetworkCredential($ftpUser, $ftpPass)

# Make sure sourceDir is absolute
$sourceDir = (Resolve-Path $sourceDir).Path

# Function to create directory on FTP
function Create-FtpDirectory {
    param([string]$dirPath)
    
    try {
        $request = [System.Net.FtpWebRequest]::Create($dirPath)
        $request.Method = [System.Net.WebRequestMethods+Ftp]::MakeDirectory
        $request.Credentials = New-Object System.Net.NetworkCredential($ftpUser, $ftpPass)
        $response = $request.GetResponse()
        $response.Close()
        Write-Host "Created directory: $dirPath"
    } catch {
        # Ignore if directory already exists
        # Write-Host "Directory might exist: $dirPath"
    }
}

$items = Get-ChildItem -Path $sourceDir -Recurse

# First create all directories
foreach ($item in $items) {
    if ($item.PSIsContainer) {
        $relPath = $item.FullName.Substring($sourceDir.Length + 1).Replace('\', '/')
        $ftpPath = "$ftpServer$ftpFolder/$relPath"
        Create-FtpDirectory -dirPath $ftpPath
    }
}

# Then upload all files
foreach ($item in $items) {
    if (-not $item.PSIsContainer) {
        $relPath = $item.FullName.Substring($sourceDir.Length + 1).Replace('\', '/')
        $ftpPath = "$ftpServer$ftpFolder/$relPath"
        
        Write-Host "Uploading $relPath to $ftpPath"
        try {
            $webclient.UploadFile($ftpPath, $item.FullName)
        } catch {
            Write-Host "Error uploading $($item.FullName): $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

Write-Host "Upload Complete!"
