Param(
  [Parameter(Mandatory=$true)]
  [String]$Instance
)

$rootPath = Split-Path (Get-Variable MyInvocation -Scope 0).Value.MyCommand.Path
$servicePath = Resolve-Path "$rootPath\Service"

$exePath = "$servicePath\bin\elasticsearch-service-x64.exe"
$exeParams = @("//DS//ELASTICSEARCH`$$($Instance.ToUpper())")

Start-Process -NoNewWindow -FilePath $exePath -ArgumentList $exeParams
