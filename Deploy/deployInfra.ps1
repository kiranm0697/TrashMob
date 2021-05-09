param ($environment, $region, $subscriptionId, $sqlAdminPassword)

$rgName = "rg-trashmob-$environment-$region"
$keyVaultName = "kv-tm-$environment-$region"
$azureMapsName = "map-tm-$environment-$region"

az group create --location $region --name $rgName

az deployment group create --template-file ".\azureMaps.bicep" -g $rgName --parameters environment=$environment region=$region
az deployment group create --template-file ".\sqlServer.bicep" -g $rgName --parameters environment=$environment region=$region subscriptionId=$subscriptionId sqlAdminPassword=$sqlAdminPassword
az deployment group create --template-file ".\sqlDatabase.bicep" -g $rgName --parameters environment=$environment region=$region subscriptionId=$subscriptionId rgName=$rgName
az deployment group create --template-file ".\keyVault.bicep" -g $rgName --parameters environment=$environment region=$region

# Add secrets to KeyVault
$mapKey = az maps account keys list --name $azureMapsName --resource-group $rgName --query primaryKey
$sqlKey = "Server=tcp:sql-tm-$environment-$region.database.windows.net,1433;Database=db-tm-$environment-$region;User ID=dbadmin;Password=$sqlAdminPassword;Encrypt=true;Connection Timeout=30;"

az keyvault secret set --vault-name $keyVaultName --name AzureMapsKey --value $mapKey
az keyvault secret set --vault-name $keyVaultName --name TMDBServerConnectionString --value $sqlKey
