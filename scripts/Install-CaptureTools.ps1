param([switch]$CheckOnly,[switch]$SelfTest,[switch]$InstallMicrosoft,[string]$PackagePath)
$ErrorActionPreference='Stop'
$ProgressPreference='SilentlyContinue'
[Net.ServicePointManager]::SecurityProtocol=[Net.SecurityProtocolType]::Tls12
# GUI processes launched by PowerShell 7 can inherit incompatible module paths.
# Use Windows PowerShell's own, system-provided modules for signature checks.
$env:PSModulePath=(Join-Path $PSHOME 'Modules')+';'+(Join-Path $env:ProgramFiles 'WindowsPowerShell\Modules')
$stage='start'
function Step([string]$name){$script:stage=$name;Write-Output ('STEP '+$name)}
function CheckHash([string]$path,[string]$expected){if(!(Test-Path -LiteralPath $path) -or (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $expected){throw 'HashMismatch'}}
function MicrosoftSigned([string]$path){$s=Get-AuthenticodeSignature -LiteralPath $path;return $s.Status -eq 'Valid' -and $s.SignerCertificate.Subject -match 'O=Microsoft Corporation(?:,|$)'}
function PythonReady([string]$path){if(!$path -or !(Test-Path -LiteralPath $path)){return $false};try{& $path -I -c 'import sys,json,struct,ctypes; assert sys.version_info >= (3,9)' *> $null;return $LASTEXITCODE -eq 0}catch{return $false}}
function ParserReady([string]$path){return $path -and (Test-Path -LiteralPath $path) -and (MicrosoftSigned $path)}
function Download([string]$url,[string]$target,[string]$hash){
 if(Test-Path -LiteralPath $target){if((Get-FileHash -LiteralPath $target).Hash -eq $hash){Write-Output 'CACHE verified';return}}
 $partial=$target+'.partial';$curl=Join-Path $env:WINDIR 'System32\curl.exe'
 & $curl --silent --show-error --fail --location --proto '=https' --proto-redir '=https' --connect-timeout 20 --max-time 240 --output $partial $url 2>$null
 if($LASTEXITCODE){throw 'DownloadFailed'}
 Step 'verify-download'
 CheckHash $partial $hash
 Move-Item -LiteralPath $partial -Destination $target -Force
 Write-Output ('HASH sha256='+$hash+' verified')
}
try{
 Import-Module (Join-Path $PSHOME 'Modules\Microsoft.PowerShell.Security\Microsoft.PowerShell.Security.psd1') -ErrorAction Stop
 if($InstallMicrosoft){
  # Stage in a protected, system-readable directory: Windows Installer may not see a user's redirected LocalAppData.
  $hash='0DF5A3E3AEDE62770333FAB8FD2E044FC3BD6C891226F2087698291E7BAD69CA'
  CheckHash $PackagePath $hash;if(!(MicrosoftSigned $PackagePath)){throw 'MicrosoftSignatureInvalid'}
  $staging=Join-Path ([Environment]::GetFolderPath('CommonApplicationData')) ('Talaria-Setup-'+[Guid]::NewGuid().ToString('N'))
  $acl=New-Object Security.AccessControl.DirectorySecurity;$acl.SetAccessRuleProtection($true,$false)
  foreach($sid in @('S-1-5-18','S-1-5-32-544')){$identity=New-Object Security.Principal.SecurityIdentifier($sid);$rule=New-Object Security.AccessControl.FileSystemAccessRule($identity,'FullControl','ContainerInherit,ObjectInherit','None','Allow');$acl.AddAccessRule($rule)}
  [IO.Directory]::CreateDirectory($staging,$acl) | Out-Null
  $staged=Join-Path $staging 'BTP-1.14.0.msi';Copy-Item -LiteralPath $PackagePath -Destination $staged
  CheckHash $staged $hash;if(!(MicrosoftSigned $staged)){throw 'MicrosoftSignatureInvalid'}
  $process=Start-Process -FilePath (Join-Path $env:WINDIR 'System32\msiexec.exe') -ArgumentList ('/i "'+$staged+'" /passive /norestart') -WindowStyle Hidden -PassThru;$process.WaitForExit();$result=$process.ExitCode
  Remove-Item -LiteralPath $staged;Remove-Item -LiteralPath $staging
  exit $result
 }
 if($SelfTest){$testFile=Join-Path ([IO.Path]::GetTempPath()) ('Talaria-hash-'+[Guid]::NewGuid().ToString('N'));[IO.File]::WriteAllText($testFile,'test');$hash=(Get-FileHash $testFile).Hash;CheckHash $testFile $hash;$rejected=$false;try{CheckHash $testFile ('0'*64)}catch{$rejected=$true};if(!$rejected){throw 'Corrupt download accepted'};Remove-Item -LiteralPath $testFile;Write-Output 'PASS capture dependency hash validation';exit 0}
 Step 'detect'
 $local=[Environment]::GetFolderPath('LocalApplicationData');$config=Join-Path $local 'PadHop\capture-tools.json';$root=Join-Path $local 'Talaria\capture-tools';$python=$null;$parser=$null
 if(Test-Path -LiteralPath $config){try{$old=Get-Content -LiteralPath $config -Raw | ConvertFrom-Json;if(PythonReady $old.Python){$python=$old.Python};if(ParserReady $old.Parser){$parser=$old.Parser}}catch{Write-Output 'CONFIG invalid'}}
 $privatePython=Join-Path $root 'python-3.12.10\python.exe'
 if(!$python -and (PythonReady $privatePython)){$python=$privatePython}
 if(!$parser){$btpRoot=Join-Path $env:SystemDrive 'BTP';if(Test-Path $btpRoot){$parser=Get-ChildItem -LiteralPath $btpRoot -Filter BTETLParse.exe -Recurse -ErrorAction SilentlyContinue | Where-Object {$_.FullName -match '[\\/](x64|amd64)[\\/]' -and (MicrosoftSigned $_.FullName)} | Sort-Object FullName -Descending | Select-Object -First 1 -ExpandProperty FullName}}
 $wpr=Join-Path $env:WINDIR 'System32\wpr.exe';Write-Output ('DETECTED python='+[bool]$python+' parser='+[bool]$parser+' wpr='+(Test-Path $wpr))
 if(!(Test-Path $wpr)){throw 'WprMissing'}
 if($CheckOnly){if($python -and $parser){Write-Output 'READY';exit 0}else{Write-Output 'MISSING';exit 2}}
 New-Item -ItemType Directory -Force $root | Out-Null
 if(!$python){
  Step 'python-download';$zip=Join-Path $root 'python-3.12.10.zip';Download 'https://www.python.org/ftp/python/3.12.10/python-3.12.10-embed-amd64.zip' $zip '4ACBED6DD1C744B0376E3B1CF57CE906F9DC9E95E68824584C8099A63025A3C3'
  Step 'python-install';$unpack=Join-Path $root 'python-3.12.10';Expand-Archive -LiteralPath $zip -DestinationPath $unpack -Force;if(!(PythonReady $privatePython)){throw 'PythonValidationFailed'};$python=$privatePython;Write-Output 'PYTHON version=3.12.10 verified'
 }else{Write-Output 'PYTHON reused'}
 if(!$parser){
  Step 'microsoft-download';$msi=Join-Path $root 'BluetoothTestPlatformPack-1.14.0.msi';Download 'https://download.microsoft.com/download/e/e/e/eeed3cd5-bdbd-47db-9b8e-ca9d2df2cd29/BluetoothTestPlatformPack-1.14.0.msi' $msi '0DF5A3E3AEDE62770333FAB8FD2E044FC3BD6C891226F2087698291E7BAD69CA'
  if(!(MicrosoftSigned $msi)){throw 'MicrosoftSignatureInvalid'};Write-Output 'SIGNATURE Microsoft verified'
  Step 'microsoft-install';$process=Start-Process -FilePath (Join-Path $env:WINDIR 'System32\WindowsPowerShell\v1.0\powershell.exe') -ArgumentList ('-NoProfile -ExecutionPolicy Bypass -File "'+$PSCommandPath+'" -InstallMicrosoft -PackagePath "'+$msi+'"') -Verb RunAs -WindowStyle Hidden -PassThru;$process.WaitForExit();$code=$process.ExitCode;Write-Output ('MSI exit='+$code);if($code -eq 1602){Write-Output 'CANCELLED';exit 1223};if($code -ne 0 -and $code -ne 3010){throw 'MicrosoftInstallFailed'};if($code -eq 3010){Write-Output 'REBOOT required'}
  $btpRoot=Join-Path $env:SystemDrive 'BTP';$parser=Get-ChildItem -LiteralPath $btpRoot -Filter BTETLParse.exe -Recurse -ErrorAction SilentlyContinue | Where-Object {$_.FullName -match '[\\/](x64|amd64)[\\/]' -and (MicrosoftSigned $_.FullName)} | Sort-Object FullName -Descending | Select-Object -First 1 -ExpandProperty FullName
 }else{Write-Output 'PARSER reused'}
 Step 'verify';if(!(PythonReady $python) -or !(ParserReady $parser)){throw 'ComponentValidationFailed'}
 New-Item -ItemType Directory -Force (Split-Path $config) | Out-Null
 $temporary=$config+'.new';@{Python=$python;Parser=$parser} | ConvertTo-Json | Set-Content -LiteralPath $temporary -Encoding UTF8;Move-Item -LiteralPath $temporary -Destination $config -Force
 Write-Output 'CONFIG saved';Write-Output 'READY';exit 0
}catch{
 $native=$_.Exception.NativeErrorCode;if($native -eq 1223){Write-Output 'CANCELLED';exit 1223}
 # Never dump exception text: it can include usernames and personal paths.
 $known=@('HashMismatch','DownloadFailed','WprMissing','PythonValidationFailed','MicrosoftSignatureInvalid','MicrosoftInstallFailed','ComponentValidationFailed');$reason=if($known -contains $_.Exception.Message){$_.Exception.Message}else{$_.Exception.GetType().Name}
 Write-Output ('FAILED step='+$stage+' reason='+$reason+' hresult='+$_.Exception.HResult+' line='+$_.InvocationInfo.ScriptLineNumber);exit 1
}
