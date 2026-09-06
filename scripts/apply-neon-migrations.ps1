[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

function Get-ConnectionParts {
    param(
        [Parameter(Mandatory)]
        [string]$ConnectionString
    )

    $parts = @{}
    $segments = [System.Collections.Generic.List[string]]::new()
    $segmentStart = 0
    $quote = $null
    for ($index = 0; $index -lt $ConnectionString.Length; $index++) {
        $character = $ConnectionString[$index]
        if ($null -ne $quote) {
            if ($character -eq $quote) {
                if ($index + 1 -lt $ConnectionString.Length -and $ConnectionString[$index + 1] -eq $quote) {
                    $index++
                }
                else {
                    $quote = $null
                }
            }
        }
        elseif ($character -eq '"' -or $character -eq "'") {
            $quote = $character
        }
        elseif ($character -eq ';') {
            $segments.Add($ConnectionString.Substring($segmentStart, $index - $segmentStart))
            $segmentStart = $index + 1
        }
    }
    $segments.Add($ConnectionString.Substring($segmentStart))

    foreach ($segment in $segments) {
        if ([string]::IsNullOrWhiteSpace($segment)) {
            continue
        }

        $separator = $segment.IndexOf('=')
        if ($separator -lt 1) {
            throw 'La cadena contiene un segmento sin formato clave=valor.'
        }

        $key = $segment.Substring(0, $separator).Trim()
        $value = $segment.Substring($separator + 1).Trim()

        if ($value.Length -ge 2) {
            $first = $value[0]
            $last = $value[$value.Length - 1]
            if (($first -eq '"' -and $last -eq '"') -or ($first -eq "'" -and $last -eq "'")) {
                $value = $value.Substring(1, $value.Length - 2)
            }
        }

        if (-not [string]::IsNullOrWhiteSpace($key)) {
            $parts[$key.ToLowerInvariant()] = $value.Trim()
        }
    }

    return $parts
}

function Redact-Output {
    param(
        [Parameter(Mandatory)]
        [object]$Output,
        [Parameter(Mandatory)]
        [string]$ConnectionString
    )

    foreach ($line in @($Output)) {
        $text = [string]$line
        $text = $text.Replace($ConnectionString, '<cadena de conexión redactada>')
        $text = [regex]::Replace($text, '(?i)(password\s*=\s*)[^;\s]*', '$1<redactada>')
        Write-Host $text
    }
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$apiProject = Join-Path $repoRoot 'LiveNow.CRM.API\LiveNow.CRM.API.csproj'
$migrationProject = Join-Path $repoRoot 'LiveNow.CRM.Infrastructure.PostgreSql\LiveNow.CRM.Infrastructure.PostgreSql.csproj'

if (-not (Test-Path -LiteralPath $apiProject) -or -not (Test-Path -LiteralPath $migrationProject)) {
    throw 'No se encontraron los proyectos de API y migraciones PostgreSQL desde la raíz del repositorio.'
}

$secureConnectionString = Read-Host 'Pega la cadena .NET/Npgsql de Neon' -AsSecureString
$rawConnectionString = $null
$connectionPointer = [IntPtr]::Zero
try {
    $connectionPointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureConnectionString)
    $rawConnectionString = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($connectionPointer)
}
finally {
    if ($connectionPointer -ne [IntPtr]::Zero) {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($connectionPointer)
    }
    $secureConnectionString.Dispose()
}

if ([string]::IsNullOrWhiteSpace($rawConnectionString)) {
    throw 'La cadena de conexión no puede estar vacía.'
}

$connectionString = $rawConnectionString.Trim()
if ($connectionString.Length -ge 2) {
    $first = $connectionString[0]
    $last = $connectionString[$connectionString.Length - 1]
    if (($first -eq '"' -and $last -eq '"') -or ($first -eq "'" -and $last -eq "'")) {
        $connectionString = $connectionString.Substring(1, $connectionString.Length - 2).Trim()
    }
}

if (-not $connectionString.StartsWith('Host=', [StringComparison]::OrdinalIgnoreCase)) {
    throw 'La cadena debe comenzar con Host=.'
}

$parts = Get-ConnectionParts -ConnectionString $connectionString
foreach ($requiredKey in @('host', 'database', 'username', 'password')) {
    if (-not $parts.ContainsKey($requiredKey) -or [string]::IsNullOrWhiteSpace($parts[$requiredKey])) {
        throw "La cadena debe contener un valor no vacío para $requiredKey."
    }
}

$hostName = $parts['host']
$databaseName = $parts['database']
Write-Host "Destino confirmado: Host=$hostName; Database=$databaseName"

$previousProvider = [Environment]::GetEnvironmentVariable('DatabaseProvider', 'Process')
$previousConnection = [Environment]::GetEnvironmentVariable('ConnectionStrings__DefaultConnection', 'Process')

try {
    [Environment]::SetEnvironmentVariable('DatabaseProvider', 'postgresql', 'Process')
    [Environment]::SetEnvironmentVariable('ConnectionStrings__DefaultConnection', $connectionString, 'Process')

    Push-Location $repoRoot
    try {
        $efOutput = & dotnet ef database update `
            --project $migrationProject `
            --startup-project $apiProject `
            --context 'LiveNow.CRM.Infrastructure.Data.LiveNowDbContext' `
            -- --environment Production 2>&1
        $exitCode = $LASTEXITCODE
    }
    finally {
        Pop-Location
    }

    Redact-Output -Output $efOutput -ConnectionString $connectionString
    if ($exitCode -ne 0) {
        throw "EF Core terminó con código de salida $exitCode."
    }
}
finally {
    [Environment]::SetEnvironmentVariable('DatabaseProvider', $previousProvider, 'Process')
    [Environment]::SetEnvironmentVariable('ConnectionStrings__DefaultConnection', $previousConnection, 'Process')
}
