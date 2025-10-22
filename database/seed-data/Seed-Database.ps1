param(
    [string]$Server = "db29009.public.databaseasp.net",
    [string]$Database = "db29009",
    [string]$User = "db29009",
    [string]$Password = "Ap6-=2PtcE!7"
)

$ErrorActionPreference = 'Stop'

function Invoke-SqlFile {
    param(
        [Parameter(Mandatory=$true)][string]$File
    )
    Write-Host "Running: $File" -ForegroundColor Cyan
    $query = Get-Content -Raw -Path $File
    sqlcmd -S $Server -d $Database -U $User -P $Password -b -I -C -i $File
}

Write-Host "Seeding database $Database on $Server" -ForegroundColor Green

# 1) Cleanup all data
Invoke-SqlFile -File "$PSScriptRoot\Cleanup.sql"

# 2) Base taxonomy
Invoke-SqlFile -File "$PSScriptRoot\..\seed-data\ServiceCategories.sql"

# 3) Users & Roles (login hashes are placeholders; adjust if needed)
Invoke-SqlFile -File "$PSScriptRoot\UsersAndRoles.sql"

# 4) Merchants for categories (depends on Users and ServiceCategories)
Invoke-SqlFile -File "$PSScriptRoot\MerchantsForCategories.sql"

# 5) Core products for fixed GUIDs
Invoke-SqlFile -File "$PSScriptRoot\CoreProducts.sql"

# 6) Review system migration (SP + trigger)
Invoke-SqlFile -File "$PSScriptRoot\..\migrations\AddProductReviewSystem.sql"

# 7) Popular products with orders and reviews (depends on previous)
Invoke-SqlFile -File "$PSScriptRoot\PopularProductsWithReviews.sql"

# 8) Analytics stored procedures (optional but recommended)
Invoke-SqlFile -File "$PSScriptRoot\..\optimizations\AnalyticsStoredProcedures.sql"

Write-Host "✅ Seeding completed successfully." -ForegroundColor Green


