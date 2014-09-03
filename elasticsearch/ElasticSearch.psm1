function Get-ESServiceName {
  param(
    [Parameter(Mandatory = $true)]
    [String] $Instance
  )

  return "ELASTICSEARCH`$$($Instance.ToUpper())"
}

function Start-ESService {
  param(
    [Parameter(Mandatory = $true)]
    [String] $Server,
    [Parameter(Mandatory = $true)]
    [String] $Instance
  )

  $ServiceName = Get-ESServiceName $Instance
  $Service = Get-Service -ComputerName $Server $ServiceName -ErrorAction SilentlyContinue
  if ($Service -ne $null) {
    Write-Host "Starting ElasticSearch Service $ServiceName on $Server" -ForegroundColor Cyan
    $Service.Start()
    $Service.WaitForStatus('Started', '00:00:30')
  } else {
    throw "ElasticSearch service $ServiceName was not found on server $Server"
  }
}

function Stop-ESService {
  param(
    [Parameter(Mandatory = $true)]
    [String] $Server,
    [Parameter(Mandatory = $true)]
    [String] $Instance
  )

  $ServiceName = Get-ESServiceName $Instance
  $Service = Get-Service -ComputerName $Server $ServiceName -ErrorAction SilentlyContinue
  if ($Service -ne $null) {
    Write-Host "Stopping ElasticSearch Service $ServiceName on $Server" -ForegroundColor Cyan
    $Service.Stop()
    $Service.WaitForStatus('Stopped', '00:00:30')
  }
}

function Get-ESVersion {
  param(
    [Parameter(Mandatory = $true)]
    [String] $Server
  )
  $url = New-Object UriBuilder(@{$true=$Server;$false="http://$Server"}[$Server.Contains("://")])
  $info = Invoke-RestMethod $url.Uri
  return $info.version.number
}

function Test-ESIndex {
  param(
    [Parameter(Mandatory = $true)]
    [String] $Server,
    [Parameter(Mandatory = $true)]
    [String] $Index
  )
  
  $url = New-Object UriBuilder(@{$true=$Server;$false="http://$Server"}[$Server.Contains("://")])
  $url.Path = $Index
  
  try {
    Invoke-RestMethod $url.Uri -Method Head
    return $true
  }
  catch [System.Net.WebException] {
    if (($_.Exception.Status -ne [System.Net.WebExceptionStatus]::ProtocolError) -or ($_.Exception.Response.StatusCode -ne [System.Net.HttpStatusCode]::NotFound)) {
      throw
    }
  }
  
  return $false
}

function New-ESIndex {
  param(
    [Parameter(Mandatory = $true)]
    [String] $Server,
    [Parameter(Mandatory = $true)]
    [String] $Index,
    [String] $Mappings
  )

  $url = New-Object UriBuilder(@{$true=$Server;$false="http://$Server"}[$Server.Contains("://")])
  $url.Path = $Index
  
  if ($Mappings) {
    $Mappings = ([IO.File]::ReadAllText(((Resolve-Path $Mappings).ProviderPath)))
    return Invoke-RestMethod $url.Uri -Method Put -Body $Mappings
  } else {
    return Invoke-RestMethod $url.Uri -Method Put
  }
}

function Get-ESIndex {
  param(
    [Parameter(Mandatory = $true)]
    [String] $Server,
    [Parameter(Mandatory = $true)]
    [String] $Index
  )
  
  $url = New-Object UriBuilder(@{$true=$Server;$false="http://$Server"}[$Server.Contains("://")])
  $url.Path = [System.IO.Path]::Combine($Index, "_settings")
  
  return Invoke-RestMethod $url.Uri
}

function Remove-ESIndex {
  param(
    [Parameter(Mandatory = $true)]
    [String] $Server,
    [Parameter(Mandatory = $true)]
    [String] $Index
  )

  $url = New-Object UriBuilder(@{$true=$Server;$false="http://$Server"}[$Server.Contains("://")])
  $url.Path = $Index

  return Invoke-RestMethod $url.Uri -Method Delete
}

function Test-ESAlias {
  param(
    [Parameter(Mandatory = $true)]
    [String] $Server,
    [Parameter(Mandatory = $true)]
    [String] $Index,
    [Parameter(Mandatory = $true)]
    [String] $Alias
  )
  
  $aliases = Get-ESAliases $Server $Index

  if ($aliases -ne $null) {
      return ($aliases | select -ExpandProperty $Index -EA Ignore | select -ExpandProperty "aliases" -EA Ignore | select -ExpandProperty $Alias -EA Ignore) -ne $null
  }
  
  return $false
}

function Get-ESAliases {
  param(
    [Parameter(Mandatory = $true)]
    [String] $Server,
    [String] $Index = $null
  )
  
  $url = New-Object UriBuilder(@{$true=$Server;$false="http://$Server"}[$Server.Contains("://")])
  
  if ($IndexName -ne $null) {
    $url.Path = [System.IO.Path]::Combine($Index, "_aliases")
  } else {
    $url.Path = "_aliases"
  }
  
  return Invoke-RestMethod $url.Uri
}

function New-ESAlias {
  param(
    [Parameter(Mandatory = $true)]
    [String] $Server,
    [Parameter(Mandatory = $true)]
    [String] $Index,
    [Parameter(Mandatory = $true)]
    [String] $Alias
  )  
  
  $url = New-Object UriBuilder(@{$true=$Server;$false="http://$Server"}[$Server.Contains("://")])
  $url.Path = "_aliases"
  $json = (ConvertTo-Json @{ actions = @( @{ add = @{ index = $Index; alias = $Alias } } ) } -Depth 3)
  
  return Invoke-RestMethod $url.Uri -Method Post -Body $json
}

function Remove-ESAlias {
  param(
    [Parameter(Mandatory = $true)]
    [String] $Server,
    [Parameter(Mandatory = $true)]
    [String] $Index,
    [Parameter(Mandatory = $true)]
    [String] $Alias
  )  
  
  $url = New-Object UriBuilder(@{$true=$Server;$false="http://$Server"}[$Server.Contains("://")])
  $url.Path = "_aliases"
  $json = (ConvertTo-Json @{ actions = @( @{ remove = @{ index = $Index; alias = $Alias } } ) } -Depth 3)
  
  return Invoke-RestMethod $url.Uri -Method Post -Body $json
}

function Move-ESAlias {
  param(
    [Parameter(Mandatory = $true)]
    [String] $Server,
    [Parameter(Mandatory = $true)]
    [String] $SourceIndex,
    [Parameter(Mandatory = $true)]
    [String] $TargetIndex,
    [Parameter(Mandatory = $true)]
    [String] $Alias
  )  
  
  $url = New-Object UriBuilder(@{$true=$Server;$false="http://$Server"}[$Server.Contains("://")])
  $url.Path = "_aliases"
  $json = (ConvertTo-Json @{ actions = @( @{ remove = @{ index = $SourceIndex; alias = $Alias }; add = @{ index = $TargetIndex; alias = $Alias } } ) } -Depth 3)
  
  return Invoke-RestMethod $url.Uri -Method Post -Body $json
}

function Set-ESRefreshInterval {
  param(
    [Parameter(Mandatory = $true)]
    [String] $Server,
    [Parameter(Mandatory = $true)]
    [String] $Index,
    [Parameter(Mandatory = $true)]
    [Int] $RefreshInterval
  )

  $url = New-Object UriBuilder(@{$true=$Server;$false="http://$Server"}[$Server.Contains("://")])
  $url.Path = [System.IO.Path]::Combine($Index, "_settings")
  $json = (ConvertTo-Json @{ index = @{ refresh_interval = $RefreshInterval } } -Depth 3)
  
  return Invoke-RestMethod $url.Uri -Method Put -Body $json
}

function Set-ESNumberOfReplicas {
  param(
    [Parameter(Mandatory = $true)]
    [String] $Server,
    [Parameter(Mandatory = $true)]
    [String] $Index,
    [Parameter(Mandatory = $true)]
    [Int] $NumberOfReplicas
  )

  $url = New-Object UriBuilder(@{$true=$Server;$false="http://$Server"}[$Server.Contains("://")])
  $url.Path = [System.IO.Path]::Combine($Index, "_settings")
  $json = (ConvertTo-Json @{ index = @{ number_of_replicas = $NumberOfReplicas } } -Depth 3)
  
  return Invoke-RestMethod $url.Uri -Method Put -Body $json
}

Export-ModuleMember -Function * -Alias *