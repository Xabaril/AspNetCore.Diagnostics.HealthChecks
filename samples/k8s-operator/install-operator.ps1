$operatorinstallerurl = "https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/raw/master/deploy/operator/installer/releases/"
Write-Host "Downloading Windows Operator Installer"
$file = "operator-installer-win.exe"
Invoke-WebRequest -Uri $($operatorinstallerurl + $file)  -OutFile $file
& "./$file"
