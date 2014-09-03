Param(
  [Parameter(Mandatory=$true)]
  [String]$Instance,

  [Parameter(Mandatory=$true)]
  [String]$Config,

  [Parameter(Mandatory=$true)]
  [String]$MinMem,
  
  [Parameter(Mandatory=$true)]
  [String]$MaxMem
)

$jvm = ""
if (Test-Path ENV:JRE_HOME) {
    $jvm = '%JRE_HOME%\bin\server\jvm.dll'
} elseif (Test-Path ENV:JAVA_HOME) {
    $jvm = '%JAVA_HOME%\bin\server\jvm.dll'
} else {
    Write-Error "Missing JRE_HOME/JAVA_HOME environment variable."
    Exit
}

$rootPath = Split-Path (Get-Variable MyInvocation -Scope 0).Value.MyCommand.Path
$dataPath = "$rootPath\Data"
$logPath  = "$rootPath\Logs"
$workPath = "$rootPath\Temp"

if (!(Test-Path $dataPath)) {
  md $dataPath | Out-Null
}

if (!(Test-Path $logPath)) {
  md $logPath | Out-Null
}

if (!(Test-Path $workPath)) {
  md $workPath | Out-Null
}

$servicePath = Resolve-Path "$rootPath\Service"
$dataPath    = Resolve-Path "$rootPath\Data"
$logPath     = Resolve-Path "$rootPath\Logs"
$workPath    = Resolve-Path "$rootPath\Temp"
$configPath  = Resolve-Path "$rootPath\Config"
$configFile  = Resolve-Path $Config

$JvmOptions = @(
    "-XX:+UseCondCardMark",
    "-XX:+UseCompressedOops",
    "-XX:+UseParNewGC",
    "-XX:+UseConcMarkSweepGC",
    "-XX:+CMSParallelRemarkEnabled",
    "-XX:SurvivorRatio=8",
    "-XX:MaxTenuringThreshold=1",
    "-XX:CMSInitiatingOccupancyFraction=75",
    "-XX:+UseCMSInitiatingOccupancyOnly",
    "-XX:+HeapDumpOnOutOfMemoryError",
    "-Djline.enabled=false",
    "-Delasticsearch",
    "-Des-foreground=yes",
    "-Des.path.home=$rootPath",
    "-Des.default.config=$configFile",
    "-Des.default.path.home=$servicePath",
    "-Des.default.path.logs=$logPath",
    "-Des.default.path.data=$dataPath",
    "-Des.default.path.work=$workPath",
    "-Des.default.path.conf=$configPath"
)

$exePath = "$servicePath\bin\elasticsearch-service-x64.exe"
$exeParams = @(
    "//IS//ELASTICSEARCH`$$($Instance.ToUpper())",
    "--Install=`"$exePath`"",
    
    "--DisplayName=`"ElasticSearch [Instance: $($Instance.ToUpper())]`"",
    "--Description=`"Distributed RESTful Full-Text Search Engine based on Lucene (http://www.elasticsearch.org/)`"",
    
    "--Classpath=`"$servicePath\lib\*;$servicePath\sigar\*`"",
    
    "--Jvm=$jvm",
    "--JvmMs=$MinMem",
    "--JvmMx=$MaxMem",
    "--JvmSs=128",
    "--JvmOptions=`"$($JvmOptions -join ';')`"",

    "--StartMode=jvm",
    "--StartClass=org.elasticsearch.bootstrap.Bootstrap",
    "--StartMethod=main",
    "--StartPath=`"$servicePath`"",
    "--StartParams=`"`"",

    "--StopMode=jvm",
    "--StopClass=org.elasticsearch.bootstrap.Bootstrap",
    "--StopMethod=close",
    "--StopPath=`"$servicePath`"",

    "--StdOutput=auto",
    "--StdError=auto",

    "--LogLevel=Warn",
    "--LogPath=`"$servicePath\logs`"",

    "--ServiceUser=`"NT AUTHORITY\NetworkService`"",
    "--Startup=auto" 
)

Write-Host "Registering ElasticSearch Windows Service"
Write-Host "$exePath $exeParams"
Start-Process -NoNewWindow -FilePath $exePath -ArgumentList $exeParams
