using 'main.bicep'

param environmentName = 'dev'
param location = 'eastus'

// Secrets: don't commit real values here. Pass them at deploy time instead, e.g.:
//   az deployment group create --resource-group rg-kiwimind-dev --template-file main.bicep \
//     --parameters main.bicepparam \
//     --parameters postgresAdminPassword=<secret> jwtSecret=<secret>
param postgresAdminPassword = ''
param jwtSecret = ''

// Azure OpenAI requires access to be approved for the subscription first -
// see kiwiMind-build-sheet.md section 9. Leave false until approved.
param deployAzureOpenAi = false
