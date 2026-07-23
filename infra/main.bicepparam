using 'main.bicep'

param environmentName = 'dev'
param location = 'australiaeast'

// Secrets: don't commit real values here. Pass them at deploy time instead, e.g.:
//   az deployment group create --resource-group rg-kiwimind-dev --template-file main.bicep \
//     --parameters main.bicepparam \
//     --parameters postgresAdminPassword=<secret> jwtSecret=<secret>
param postgresAdminPassword = ''
param jwtSecret = ''

param deployAzureOpenAi = true
