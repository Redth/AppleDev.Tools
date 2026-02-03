<#
.SYNOPSIS
    Fetch and analyze Apple's App Store Connect OpenAPI specification.

.DESCRIPTION
    Downloads the latest spec from Apple and provides analysis of:
    - Available endpoints and their operations
    - Data schemas and their relationships
    - Resource categories (tags)
    - Request/response patterns

.PARAMETER Refresh
    Force re-download of the spec even if cached.

.PARAMETER Resource
    Show endpoints for a specific resource (e.g., BundleIdCapabilities).

.PARAMETER Schema
    Show schema details for a specific schema name.

.PARAMETER Search
    Search endpoints and schemas for a query string.

.PARAMETER ListResources
    List all resource categories.

.EXAMPLE
    .\fetch_openapi_spec.ps1
    # Download spec and show summary

.EXAMPLE
    .\fetch_openapi_spec.ps1 -ListResources
    # List all 192 resource categories

.EXAMPLE
    .\fetch_openapi_spec.ps1 -Resource BundleIdCapabilities
    # Show endpoints for a resource

.EXAMPLE
    .\fetch_openapi_spec.ps1 -Schema BundleIdCreateRequest
    # Show schema details

.EXAMPLE
    .\fetch_openapi_spec.ps1 -Search testflight
    # Search endpoints and schemas
#>

param(
    [switch]$Refresh,
    [string]$Resource,
    [string]$Schema,
    [string]$Search,
    [switch]$ListResources
)

$SpecUrl = "https://developer.apple.com/sample-code/app-store-connect/app-store-connect-openapi-specification.zip"
$CacheDir = Join-Path $HOME ".cache" "asc-openapi"
$CacheFile = Join-Path $CacheDir "openapi.oas.json"

function Download-Spec {
    param([switch]$Force)
    
    if ((Test-Path $CacheFile) -and -not $Force) {
        return Get-Content $CacheFile -Raw | ConvertFrom-Json -AsHashtable
    }
    
    Write-Host "Downloading OpenAPI spec from Apple..." -ForegroundColor Cyan
    
    $tempZip = Join-Path $env:TEMP "asc-openapi.zip"
    $tempDir = Join-Path $env:TEMP "asc-openapi-extract"
    
    try {
        Invoke-WebRequest -Uri $SpecUrl -OutFile $tempZip
        
        if (Test-Path $tempDir) { Remove-Item $tempDir -Recurse -Force }
        Expand-Archive -Path $tempZip -DestinationPath $tempDir -Force
        
        $jsonFile = Get-ChildItem -Path $tempDir -Filter "*.json" -Recurse | Select-Object -First 1
        if (-not $jsonFile) {
            throw "No JSON file found in zip"
        }
        
        if (-not (Test-Path $CacheDir)) {
            New-Item -ItemType Directory -Path $CacheDir -Force | Out-Null
        }
        
        Copy-Item $jsonFile.FullName $CacheFile -Force
        Write-Host "Cached spec to $CacheFile" -ForegroundColor Green
        
        return Get-Content $CacheFile -Raw | ConvertFrom-Json -AsHashtable
    }
    finally {
        if (Test-Path $tempZip) { Remove-Item $tempZip -Force }
        if (Test-Path $tempDir) { Remove-Item $tempDir -Recurse -Force }
    }
}

function Get-AllTags {
    param($Spec)
    
    $tags = @{}
    foreach ($path in $Spec.paths.Keys) {
        foreach ($method in $Spec.paths[$path].Keys) {
            $details = $Spec.paths[$path][$method]
            if ($details -is [hashtable] -and $details.ContainsKey('tags')) {
                foreach ($tag in $details.tags) {
                    $tags[$tag] = $true
                }
            }
        }
    }
    return $tags.Keys | Sort-Object
}

function Show-Summary {
    param($Spec)
    
    Write-Host ("=" * 60) -ForegroundColor Cyan
    Write-Host "APP STORE CONNECT API - OpenAPI Specification Summary" -ForegroundColor Cyan
    Write-Host ("=" * 60) -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Version: $($Spec.info.version)"
    Write-Host "Base URL: $($Spec.servers[0].url)"
    Write-Host "Total Endpoints: $($Spec.paths.Count)"
    Write-Host "Total Schemas: $($Spec.components.schemas.Count)"
    
    $tags = Get-AllTags -Spec $Spec
    Write-Host "Resource Categories: $($tags.Count)"
    
    $common = @('Apps', 'BundleIds', 'Certificates', 'Devices', 'Profiles', 
                'Users', 'BundleIdCapabilities', 'AppStoreVersions')
    
    Write-Host ""
    Write-Host "Core Resources:" -ForegroundColor Yellow
    foreach ($tag in $common) {
        if ($tags -contains $tag) {
            Write-Host "  - $tag"
        }
    }
    
    Write-Host ""
    Write-Host "Use -ListResources to see all $($tags.Count) resource categories"
    Write-Host "Use -Resource <name> to see endpoints for a resource"
    Write-Host "Use -Schema <name> to see schema details"
}

function Show-Resources {
    param($Spec)
    
    $tags = Get-AllTags -Spec $Spec
    Write-Host "Resource Categories ($($tags.Count)):" -ForegroundColor Cyan
    Write-Host ""
    foreach ($tag in $tags) {
        Write-Host "  $tag"
    }
}

function Show-Resource {
    param($Spec, [string]$ResourceName)
    
    Write-Host ""
    Write-Host "=== $ResourceName Endpoints ===" -ForegroundColor Cyan
    Write-Host ""
    
    $found = $false
    foreach ($path in $Spec.paths.Keys | Sort-Object) {
        foreach ($method in $Spec.paths[$path].Keys) {
            $details = $Spec.paths[$path][$method]
            if ($details -is [hashtable] -and $details.ContainsKey('tags')) {
                if ($details.tags -contains $ResourceName) {
                    $found = $true
                    $opId = if ($details.operationId) { $details.operationId } else { "N/A" }
                    
                    Write-Host ("{0,-7} {1}" -f $method.ToUpper(), $path)
                    Write-Host "        Operation: $opId"
                    
                    # Request body
                    if ($details.requestBody -and $details.requestBody.content) {
                        foreach ($contentType in $details.requestBody.content.Keys) {
                            $schemaInfo = $details.requestBody.content[$contentType]
                            if ($schemaInfo.schema -and $schemaInfo.schema.'$ref') {
                                $ref = $schemaInfo.schema.'$ref'.Split('/')[-1]
                                Write-Host "        Request: $ref"
                            }
                        }
                    }
                    
                    # Response schemas
                    if ($details.responses) {
                        foreach ($code in $details.responses.Keys) {
                            if ($code -match '^2') {
                                $resp = $details.responses[$code]
                                if ($resp.content) {
                                    foreach ($contentType in $resp.content.Keys) {
                                        $schemaInfo = $resp.content[$contentType]
                                        if ($schemaInfo.schema -and $schemaInfo.schema.'$ref') {
                                            $ref = $schemaInfo.schema.'$ref'.Split('/')[-1]
                                            Write-Host "        Response ($code): $ref"
                                        }
                                    }
                                }
                            }
                        }
                    }
                    Write-Host ""
                }
            }
        }
    }
    
    if (-not $found) {
        Write-Host "No endpoints found for resource: $ResourceName" -ForegroundColor Red
        Write-Host "Use -ListResources to see available resources"
    }
}

function Show-Schema {
    param($Spec, [string]$SchemaName)
    
    $schemas = $Spec.components.schemas
    $matches = $schemas.Keys | Where-Object { $_ -like "*$SchemaName*" } | Sort-Object
    
    if (-not $matches) {
        Write-Host "No schemas found matching: $SchemaName" -ForegroundColor Red
        return
    }
    
    # Check for exact match
    $exact = $matches | Where-Object { $_.ToLower() -eq $SchemaName.ToLower() }
    if ($exact) { $matches = @($exact) }
    
    if ($matches.Count -gt 10) {
        Write-Host "Found $($matches.Count) matching schemas:" -ForegroundColor Yellow
        $matches | Select-Object -First 20 | ForEach-Object { Write-Host "  $_" }
        if ($matches.Count -gt 20) {
            Write-Host "  ... and $($matches.Count - 20) more"
        }
        return
    }
    
    foreach ($name in $matches) {
        $schema = $schemas[$name]
        Write-Host ""
        Write-Host "=== $name ===" -ForegroundColor Cyan
        Write-Host "Type: $(if ($schema.type) { $schema.type } else { 'object' })"
        
        if ($schema.properties) {
            Write-Host "Properties:"
            foreach ($prop in $schema.properties.Keys) {
                $details = $schema.properties[$prop]
                $propType = ""
                if ($details.'$ref') {
                    $propType = $details.'$ref'.Split('/')[-1]
                }
                elseif ($details.items -and $details.items.'$ref') {
                    $propType = "array of $($details.items.'$ref'.Split('/')[-1])"
                }
                elseif ($details.type -eq 'array') {
                    $itemType = if ($details.items.type) { $details.items.type } else { 'any' }
                    $propType = "array of $itemType"
                }
                else {
                    $propType = $details.type
                }
                Write-Host "  - ${prop}: $propType"
            }
        }
        
        if ($schema.required) {
            Write-Host "Required: $($schema.required -join ', ')"
        }
        
        if ($schema.enum) {
            Write-Host "Enum values: $($schema.enum -join ', ')"
        }
    }
}

function Search-Spec {
    param($Spec, [string]$Query)
    
    $query = $Query.ToLower()
    
    Write-Host ""
    Write-Host "=== Endpoints matching '$Query' ===" -ForegroundColor Cyan
    Write-Host ""
    foreach ($path in $Spec.paths.Keys | Sort-Object) {
        if ($path.ToLower().Contains($query)) {
            foreach ($method in $Spec.paths[$path].Keys) {
                if ($Spec.paths[$path][$method] -is [hashtable]) {
                    Write-Host ("  {0,-7} {1}" -f $method.ToUpper(), $path)
                }
            }
        }
    }
    
    Write-Host ""
    Write-Host "=== Schemas matching '$Query' ===" -ForegroundColor Cyan
    Write-Host ""
    foreach ($name in $Spec.components.schemas.Keys | Sort-Object) {
        if ($name.ToLower().Contains($query)) {
            Write-Host "  $name"
        }
    }
}

# Main execution
$spec = Download-Spec -Force:$Refresh

if ($ListResources) {
    Show-Resources -Spec $spec
}
elseif ($Resource) {
    Show-Resource -Spec $spec -ResourceName $Resource
}
elseif ($Schema) {
    Show-Schema -Spec $spec -SchemaName $Schema
}
elseif ($Search) {
    Search-Spec -Spec $spec -Query $Search
}
else {
    Show-Summary -Spec $spec
}
