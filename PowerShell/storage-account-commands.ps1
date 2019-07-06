$SA_RG_NAME = "$env:SA_RG_NAME"
Write-Verbose "SA_RG_NAME: $SA_RG_NAME" -Verbose

$SA_NAME = "$env:SA_NAME"
Write-Verbose "SA_NAME: $SA_NAME" -Verbose

$SA_URL = "https://$env:SA_NAME.blob.core.windows.net"
Write-Verbose "SA_URL: $SA_URL" -Verbose

$MOCK_DATA_PATH = "$env:MOCK_DATA_PATH"
Write-Verbose "MOCK_DATA_PATH: $MOCK_DATA_PATH" -Verbose

# Get Storage Account Key
$SA_KEY = $(az storage account keys list `
--account-name "$SA_NAME" `
--resource-group "$SA_RG_NAME" `
--query "[?contains(keyName, 'key1')].value" `
--output tsv)
Write-Verbose "SA_KEY: $SA_KEY" -Verbose

# Create a Storage Account container for CSV files
az storage container create `
--name "csv-files" `
--account-name "$SA_NAME" `
--auth-mode "key" `
--account-key "$SA_KEY"

# Create a Storage Account container for JSON files
az storage container create `
--name "json-files" `
--account-name "$SA_NAME" `
--auth-mode "key" `
--account-key "$SA_KEY"

# Create a Storage Account container for SQL files
az storage container create `
--name "sql-files" `
--account-name "$SA_NAME" `
--auth-mode "key" `
--account-key "$SA_KEY"

# Upload a CSV file to the Storage Account
az storage blob upload `
--account-name "$SA_NAME" `
--auth-mode "key" `
--account-key "$SA_KEY" `
--container-name "csv-files" `
--name "MOCK_DATA.csv" `
--file "../MockData/MOCK_DATA.csv"

# Upload a JSON file to the Storage Account
az storage blob upload `
--account-name "$SA_NAME" `
--auth-mode "key" `
--account-key "$SA_KEY" `
--container-name "json-files" `
--name "MOCK_DATA.json" `
--file "../MockData/MOCK_DATA.json"

# Upload a SQL file to the Storage Account
az storage blob upload `
--account-name "$SA_NAME" `
--auth-mode "key" `
--account-key "$SA_KEY" `
--container-name "sql-files" `
--name "MOCK_DATA.sql" `
--file "../MockData/MOCK_DATA.sql"

$SAS_END_DATE = (Get-Date).AddMinutes(30).ToString("yyyy-MM-dTH:mZ")
Write-Verbose "SAS_END_DATE: $SAS_END_DATE" -Verbose

$SAS = az storage container generate-sas `
--name "csv-files" `
--account-name "$SA_NAME" `
--auth-mode "key" `
--account-key "$SA_KEY" `
--https-only `
--permissions dlrw `
--expiry $SAS_END_DATE `
-o tsv`
Write-Verbose "SAS: $SAS" -Verbose

azcopy login --tenant-id="bfebaa8a-81ff-46e4-9a1f-0a76c3dd6c68"

# Upload a CSV file to the Storage Account
azcopy `
/Source:"$MOCK_DATA_PATH\MOCK_DATA.csv" `
/Dest:"$SA_URL/csv-files/MOCK_DATA.csv" `
/DestKey:"$SA_KEY" `
/NC:10 `
/V:"C:\srcServerless\DataFileProcessor\PowerShell\csv-files.log" `
/Y

# Upload a JSON file to the Storage Account
azcopy `
/Source:"$MOCK_DATA_PATH\MOCK_DATA.json" `
/Dest:"$SA_URL/json-files/MOCK_DATA.json" `
/DestKey:"$SA_KEY" `
/NC:10 `
/V:"C:\srcServerless\DataFileProcessor\PowerShell\json-files.log" `
/Y

# Upload a SQL file to the Storage Account
azcopy `
/Source:"$MOCK_DATA_PATH\MOCK_DATA.sql" `
/Dest:"$SA_URL/sql-files/MOCK_DATA.sql" `
/DestKey:"$SA_KEY" `
/NC:10 `
/V:"C:\srcServerless\DataFileProcessor\PowerShell\sql-files.log" `
/Y
