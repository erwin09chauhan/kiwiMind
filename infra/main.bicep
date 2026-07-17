targetScope = 'resourceGroup'

@description('Short environment name, e.g. dev, staging, prod. Used as a naming suffix.')
param environmentName string = 'dev'

@description('Azure region for most resources.')
param location string = resourceGroup().location

@description('Azure region for Azure OpenAI - only available in a subset of regions.')
param openAiLocation string = location

@description('PostgreSQL administrator login name.')
param postgresAdminLogin string = 'kiwimind_admin'

@secure()
@description('PostgreSQL administrator password.')
param postgresAdminPassword string

@secure()
@description('Symmetric signing key for JWT access tokens.')
param jwtSecret string

@description('Full container image reference for the API, e.g. myregistry.azurecr.io/kiwimind-api:latest. Leave as the placeholder for the first deployment - CI/CD updates this on every push.')
param apiContainerImage string = 'mcr.microsoft.com/dotnet/samples:aspnetapp'

@description('Deployed frontend origin, allowed via CORS. Leave empty until the frontend is deployed.')
param corsAllowedOrigin string = ''

@description('Deploy an Azure OpenAI account and model deployments. Requires OpenAI access to be approved for the subscription - see build sheet section 9.')
param deployAzureOpenAi bool = false

var resourceToken = uniqueString(resourceGroup().id, environmentName)
var namePrefix = 'kiwimind${environmentName}'

module logAnalytics 'modules/log-analytics.bicep' = {
  name: 'log-analytics'
  params: {
    name: '${namePrefix}-logs-${resourceToken}'
    location: location
  }
}

module appInsights 'modules/app-insights.bicep' = {
  name: 'app-insights'
  params: {
    name: '${namePrefix}-appi-${resourceToken}'
    location: location
    logAnalyticsWorkspaceId: logAnalytics.outputs.id
  }
}

module containerRegistry 'modules/container-registry.bicep' = {
  name: 'container-registry'
  params: {
    // ACR names must be alphanumeric only, no hyphens.
    name: '${namePrefix}acr${resourceToken}'
    location: location
  }
}

module postgres 'modules/postgres.bicep' = {
  name: 'postgres'
  params: {
    name: '${namePrefix}-pg-${resourceToken}'
    location: location
    administratorLogin: postgresAdminLogin
    administratorPassword: postgresAdminPassword
  }
}

module storage 'modules/storage.bicep' = {
  name: 'storage'
  params: {
    // Storage account names must be lowercase alphanumeric, <=24 chars.
    name: take('${namePrefix}st${resourceToken}', 24)
    location: location
  }
}

module openAi 'modules/openai.bicep' = if (deployAzureOpenAi) {
  name: 'openai'
  params: {
    name: '${namePrefix}-openai-${resourceToken}'
    location: openAiLocation
  }
}

module containerAppsEnvironment 'modules/container-apps-environment.bicep' = {
  name: 'container-apps-environment'
  params: {
    name: '${namePrefix}-env-${resourceToken}'
    location: location
    logAnalyticsCustomerId: logAnalytics.outputs.customerId
    logAnalyticsSharedKey: logAnalytics.outputs.sharedKey
  }
}

var postgresConnectionString = 'Host=${postgres.outputs.fullyQualifiedDomainName};Database=${postgres.outputs.databaseName};Username=${postgresAdminLogin};Password=${postgresAdminPassword};Ssl Mode=Require'

module containerApp 'modules/container-app.bicep' = {
  name: 'container-app'
  params: {
    name: '${namePrefix}-api-${resourceToken}'
    location: location
    environmentId: containerAppsEnvironment.outputs.id
    containerRegistryLoginServer: containerRegistry.outputs.loginServer
    containerImage: apiContainerImage
    postgresConnectionString: postgresConnectionString
    blobStorageConnectionString: storage.outputs.connectionString
    jwtSecret: jwtSecret
    appInsightsConnectionString: appInsights.outputs.connectionString
    corsAllowedOrigin: corsAllowedOrigin
  }
}

resource acr 'Microsoft.ContainerRegistry/registries@2023-11-01-preview' existing = {
  name: containerRegistry.outputs.name
}

// Grant the Container App's managed identity permission to pull images from
// this specific registry (scoped to the ACR, not the whole resource group),
// avoiding the need to store registry admin credentials as a secret.
resource acrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(acr.id, containerApp.outputs.principalId, 'AcrPull')
  scope: acr
  properties: {
    // AcrPull built-in role definition ID.
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
    principalId: containerApp.outputs.principalId
    principalType: 'ServicePrincipal'
  }
}

output apiUrl string = 'https://${containerApp.outputs.fqdn}'
output containerRegistryLoginServer string = containerRegistry.outputs.loginServer
output postgresFullyQualifiedDomainName string = postgres.outputs.fullyQualifiedDomainName
output appInsightsConnectionString string = appInsights.outputs.connectionString
output openAiEndpoint string = deployAzureOpenAi ? openAi.outputs.endpoint : ''
