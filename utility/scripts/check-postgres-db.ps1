# Check and Create PostgreSQL Database and Tables for PIQI Tools
# This script verifies the PostgreSQL database and schema exist, creating them if not.

param(
    [string]$PgHost = $env:PGHOST,
    [int]$Port = 0,
    [string]$Database = $env:PGDATABASE,
    [string]$User = $env:PGUSER,
    [string]$Password = $env:PGPASSWORD,
    [string]$PsqlPath = $null
)

# Set defaults if not provided
if (-not $PgHost) { $PgHost = "localhost" }
if ($Port -eq 0) { 
    if ($env:PGPORT) { $Port = [int]$env:PGPORT } else { $Port = 5432 }
}
if (-not $Database) { $Database = "piqi" }
$userWasProvided = -not [string]::IsNullOrWhiteSpace($User)
if (-not $User) { $User = "postgres" }

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir
$schemaFile = Join-Path $projectRoot "src\db\schema.sql"

Write-Host "PostgreSQL Database Check and Setup" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Connection Details:"
Write-Host "  Host:     $PgHost`:$Port"
Write-Host "  Database: $Database"
Write-Host "  User:     $User"
Write-Host ""

# Resolve psql executable path
$psqlExe = $null

if ($PsqlPath) {
    if (Test-Path $PsqlPath) {
        $psqlExe = $PsqlPath
    } else {
        Write-Host "[X] Provided -PsqlPath does not exist: $PsqlPath" -ForegroundColor Red
        exit 1
    }
}

if (-not $psqlExe) {
    $psqlCommand = Get-Command psql -ErrorAction SilentlyContinue
    if ($psqlCommand) {
        $psqlExe = $psqlCommand.Source
    }
}

if (-not $psqlExe) {
    $candidatePsqlPaths = @(
        "C:\Program Files\PostgreSQL\18\pgAdmin 4\runtime\psql.exe",
        "C:\Program Files\PostgreSQL\17\pgAdmin 4\runtime\psql.exe",
        "C:\Program Files\PostgreSQL\16\pgAdmin 4\runtime\psql.exe",
        "C:\Program Files\PostgreSQL\15\pgAdmin 4\runtime\psql.exe",
        "C:\Program Files\PostgreSQL\14\pgAdmin 4\runtime\psql.exe",
        "C:\Program Files\PostgreSQL\13\pgAdmin 4\runtime\psql.exe"
    )

    foreach ($candidate in $candidatePsqlPaths) {
        if (Test-Path $candidate) {
            $psqlExe = $candidate
            break
        }
    }
}

if (-not $psqlExe) {
    Write-Host "[X] psql command not found. Provide -PsqlPath or add psql.exe to PATH." -ForegroundColor Red
    exit 1
}

Write-Host "Using psql: $psqlExe"

function Test-Login {
    param(
        [string]$CandidateUser,
        [string]$DbName = "postgres"
    )
    $result = & $psqlExe -h $PgHost -p $Port -U $CandidateUser -d $DbName -t -A -c "SELECT 1;" 2>&1
    if ($LASTEXITCODE -eq 0 -and $result -match "^\s*1\s*$") {
        return $true
    }
    return $false
}

if (-not $userWasProvided) {
    $candidateUsers = @("postgres", "admin")
    if ($env:USERNAME -and -not ($candidateUsers -contains $env:USERNAME)) {
        $candidateUsers += $env:USERNAME
    }

    foreach ($candidateUser in $candidateUsers) {
        if (Test-Login -CandidateUser $candidateUser) {
            $User = $candidateUser
            break
        }
    }
}

Write-Host "Resolved user: $User"

# Build connection string for psql
$env:PGPASSWORD = $Password

function Invoke-Psql {
    param(
        [string]$Query,
        [string]$DbName = $Database
    )
    $result = & $psqlExe -h $PgHost -p $Port -U $User -d $DbName -t -A -c $Query 2>&1
    return $result
}

function Test-DatabaseExists {
    param([string]$DbName)
    $query = "SELECT 1 FROM pg_database WHERE datname = '$DbName';"
    $result = & $psqlExe -h $PgHost -p $Port -U $User -d "postgres" -t -A -c $query 2>&1
    return $result -eq "1"
}

function Test-TableExists {
    param([string]$TableName)
    $query = "SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = '$TableName';"
    $result = Invoke-Psql -Query $query
    return $result -eq "1"
}

function Test-ColumnExists {
    param(
        [string]$TableName,
        [string]$ColumnName
    )
    $query = "SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = '$TableName' AND column_name = '$ColumnName';"
    $result = Invoke-Psql -Query $query
    return $result -eq "1"
}

function Test-IndexExists {
    param([string]$IndexName)
    $query = "SELECT 1 FROM pg_indexes WHERE schemaname = 'public' AND indexname = '$IndexName';"
    $result = Invoke-Psql -Query $query
    return $result -eq "1"
}

function Test-ResultsRunForeignKeyExists {
    $query = @"
SELECT 1
FROM pg_constraint c
JOIN pg_attribute a
  ON a.attrelid = c.conrelid
 AND a.attnum = ANY (c.conkey)
WHERE c.contype = 'f'
    AND c.conrelid = 'public.piqi_evaluation_results'::regclass
  AND c.confrelid = 'public.piqi_evaluation_run'::regclass
  AND a.attname = 'run_id'
LIMIT 1;
"@
    $result = Invoke-Psql -Query $query
    return $result -eq "1"
}

# Step 1: Test basic PostgreSQL connectivity
Write-Host "Step 1: Testing PostgreSQL connectivity..." -ForegroundColor Yellow
try {
    $testResult = & $psqlExe -h $PgHost -p $Port -U $User -d "postgres" -t -A -c "SELECT 1;" 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[X] Cannot connect to PostgreSQL server." -ForegroundColor Red
        Write-Host "    Error: $testResult" -ForegroundColor Red
        Write-Host ""
        Write-Host "Please ensure:" -ForegroundColor Yellow
        Write-Host "  - PostgreSQL server is running"
        Write-Host "  - Connection details are correct"
        Write-Host "  - Password is set via -Password parameter or PGPASSWORD environment variable"
        exit 1
    }
    Write-Host "[OK] Connected to PostgreSQL server" -ForegroundColor Green
} catch {
    Write-Host "[X] Failed to connect: $_" -ForegroundColor Red
    exit 1
}

# Step 2: Check if database exists
Write-Host ""
Write-Host "Step 2: Checking if database '$Database' exists..." -ForegroundColor Yellow
$dbExists = Test-DatabaseExists -DbName $Database

if ($dbExists) {
    Write-Host "[OK] Database '$Database' exists" -ForegroundColor Green
} else {
    Write-Host "[!] Database '$Database' does not exist. Creating..." -ForegroundColor Yellow
    $createDbResult = & $psqlExe -h $PgHost -p $Port -U $User -d "postgres" -c "CREATE DATABASE $Database;" 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[X] Failed to create database: $createDbResult" -ForegroundColor Red
        exit 1
    }
    Write-Host "[OK] Database '$Database' created successfully" -ForegroundColor Green
}

# Step 3: Verify required tables and schema components
Write-Host ""
Write-Host "Step 3: Checking required tables and columns..." -ForegroundColor Yellow

$requiredTables = @("piqi_evaluation_run", "piqi_evaluation_results")
$requiredColumns = @{
    "piqi_evaluation_run" = @("id", "run_name", "piqi_model_mnemonic", "evaluation_rubric_mnemonic", "status", "total_evaluations", "total_records", "total_completed", "total_failed", "created_at", "completed_at")
    "piqi_evaluation_results" = @("id", "run_id", "row_num", "message_id", "data_class", "attribute_name", "attribute_value", "assessment", "status", "reason", "effect", "created_at")
}
$requiredIndexes = @(
    "idx_piqi_evaluation_run_status",
    "idx_piqi_evaluation_results_run_id",
    "idx_piqi_evaluation_results_message_id",
    "idx_piqi_evaluation_results_run_message"
)

$missingTables = @()
$missingColumns = @()
$missingIndexes = @()
$missingConstraints = @()

foreach ($table in $requiredTables) {
    $tableExists = Test-TableExists -TableName $table
    if ($tableExists) {
        Write-Host "  [OK] Table '$table' exists" -ForegroundColor Green
    } else {
        Write-Host "  [!] Table '$table' is missing" -ForegroundColor Yellow
        $missingTables += $table
    }
}

foreach ($table in $requiredTables) {
    if ($missingTables -contains $table) {
        continue
    }

    foreach ($column in $requiredColumns[$table]) {
        $columnExists = Test-ColumnExists -TableName $table -ColumnName $column
        if ($columnExists) {
            Write-Host "  [OK] Column '$table.$column' exists" -ForegroundColor Green
        } else {
            Write-Host "  [!] Column '$table.$column' is missing" -ForegroundColor Yellow
            $missingColumns += "$table.$column"
        }
    }
}

if (-not ($missingTables -contains "piqi_evaluation_results") -and -not ($missingTables -contains "piqi_evaluation_run")) {
    $fkExists = Test-ResultsRunForeignKeyExists
    if ($fkExists) {
        Write-Host "  [OK] Foreign key piqi_evaluation_results.run_id -> piqi_evaluation_run.id exists" -ForegroundColor Green
    } else {
        Write-Host "  [!] Foreign key piqi_evaluation_results.run_id -> piqi_evaluation_run.id is missing" -ForegroundColor Yellow
        $missingConstraints += "fk_results_to_run"
    }
}

foreach ($idx in $requiredIndexes) {
    $indexExists = Test-IndexExists -IndexName $idx
    if ($indexExists) {
        Write-Host "  [OK] Index '$idx' exists" -ForegroundColor Green
    } else {
        Write-Host "  [!] Index '$idx' is missing" -ForegroundColor Yellow
        $missingIndexes += $idx
    }
}

# Step 4: Apply schema.sql only when required objects are missing
if ($missingTables.Count -gt 0 -or $missingColumns.Count -gt 0 -or $missingIndexes.Count -gt 0 -or $missingConstraints.Count -gt 0) {
    Write-Host ""
    Write-Host "Step 4: Applying additive schema updates from schema.sql..." -ForegroundColor Yellow
    
    if (-not (Test-Path $schemaFile)) {
        Write-Host "[X] Schema file not found: $schemaFile" -ForegroundColor Red
        exit 1
    }
    
    $schemaResult = & $psqlExe -h $PgHost -p $Port -U $User -d $Database -f $schemaFile 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[X] Failed to execute schema: $schemaResult" -ForegroundColor Red
        exit 1
    }
    Write-Host "[OK] Schema applied successfully" -ForegroundColor Green
    
    # Verify required objects after schema apply
    Write-Host ""
    Write-Host "Verifying required tables..." -ForegroundColor Yellow
    foreach ($table in $requiredTables) {
        $tableExists = Test-TableExists -TableName $table
        if ($tableExists) {
            Write-Host "  [OK] Table '$table' present" -ForegroundColor Green
        } else {
            Write-Host "  [X] Table '$table' is still missing" -ForegroundColor Red
        }
    }
} else {
    Write-Host ""
    Write-Host "Step 4: Required schema is already present, no schema changes needed" -ForegroundColor Green
}

# Step 5: Final index listing
Write-Host ""
Write-Host "Step 5: Listing PIQI indexes..." -ForegroundColor Yellow
$indexQuery = @"
SELECT indexname FROM pg_indexes 
WHERE schemaname = 'public' 
AND indexname LIKE 'idx_piqi_%'
ORDER BY indexname;
"@
$indexes = Invoke-Psql -Query $indexQuery
if ($indexes) {
    $indexList = $indexes -split "`n" | Where-Object { $_ -ne "" }
    foreach ($idx in $indexList) {
        Write-Host "  [OK] Index '$idx' exists" -ForegroundColor Green
    }
} else {
    Write-Host "  [!] No custom indexes found (they will be created with schema)" -ForegroundColor Yellow
}

# Summary
Write-Host ""
Write-Host "====================================" -ForegroundColor Cyan
Write-Host "PostgreSQL Setup Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Database: $Database"
Write-Host "Tables:   $($requiredTables -join ', ')"
Write-Host ""
