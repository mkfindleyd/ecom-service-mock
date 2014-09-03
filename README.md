# Ecommerce Service Mock

This service mock uses ElasticSearch for persisting test orders. Use the following powershell 
to manage an ElasticSearch instance on your local dev machine:

```PowerShell
# install windows service
.\Install-Service.ps1 -Instance local -Config .\Config\localhost.yml -MinMem 256 -MaxMem 1024
Start-Service 'ELASTICSEARCH$LOCAL'

# create ecom poc index
Import-Module .\ElasticSearch.psm1
New-ESIndex -Server localhost:9200 -Index ecom-poc

# uninstall windows service
Stop-Service 'ELASTICSEARCH$LOCAL'
.\Uninstall-Service.ps1 -Instance local

```