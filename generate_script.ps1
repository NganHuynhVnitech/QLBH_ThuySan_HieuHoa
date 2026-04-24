[CmdletBinding()]
param()

try {
    # Load SMO assembly
    [Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.Smo") | Out-Null
    [Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.SqlEnum") | Out-Null
    [Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.Management.Sdk.Sfc") | Out-Null
    
    $serverName = "HG-PC\SQLEXPRESS"
    $databaseName = "HieuHoaDB"
    $userName = "AccAntigravity"
    $password = "Anti@123"
    $outputFile = "c:\Users\Admin\Documents\CodeQLBH_HieuHoa\QLBH_ThuySan_HieuHoa\Generate_HieuHoaDB.sql"
    
    if (Test-Path $outputFile) { Remove-Item $outputFile }

    Write-Host "Connecting to server..."
    $srv = New-Object Microsoft.SqlServer.Management.Smo.Server($serverName)
    $srv.ConnectionContext.LoginSecure = $false
    $srv.ConnectionContext.Login = $userName
    $srv.ConnectionContext.Password = $password
    $srv.ConnectionContext.Connect()
    
    $db = $srv.Databases[$databaseName]
    if ($null -eq $db) {
        Write-Error "Database $databaseName not found."
        exit 1
    }

    Write-Host "Configuring Scripter..."
    $scripter = New-Object Microsoft.SqlServer.Management.Smo.Scripter($srv)
    $scripter.Options.ScriptSchema = $true
    $scripter.Options.ScriptData = $true
    $scripter.Options.ScriptDrops = $false
    $scripter.Options.WithDependencies = $true
    $scripter.Options.Indexes = $true
    $scripter.Options.Triggers = $true
    $scripter.Options.DriAll = $true
    $scripter.Options.IncludeHeaders = $false
    $scripter.Options.ToFileOnly = $true
    $scripter.Options.FileName = $outputFile
    $scripter.Options.AppendToFile = $true

    Write-Host "Scripting tables and data..."
    $tables = $db.Tables | Where-Object { $_.IsSystemObject -eq $false }
    if ($tables) {
        $scripter.EnumScript($tables)
    }

    Write-Host "Scripting stored procedures..."
    $scripter.Options.ScriptData = $false # SPs dont have data
    $scripter.Options.WithDependencies = $false
    $procedures = $db.StoredProcedures | Where-Object { $_.IsSystemObject -eq $false }
    if ($procedures) {
        $scripter.EnumScript($procedures)
    }

    Write-Host "Done! Script saved to $outputFile"
} catch {
    Write-Error $_.Exception.ToString()
}
