targetScope = 'resourceGroup'

@description('Short environment name, e.g. dev, staging, prod. Used as a naming suffix.')
param environmentName string = 'dev'

@description('Azure region for most resources.')
param location string = resourceGroup().location

@description('Azure region for Azure OpenAI - only available in a subset of regions.')
param openAiLocation string = location

@description('Azure region for the Static Web App - only available in a small subset of regions (East US 2, Central US, West US 2, West Europe, East Asia).')
param staticWebAppLocation string = 'eastasia'

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

module staticWebApp 'modules/static-web-app.bicep' = {
  name: 'static-web-app'
  params: {
    name: '${namePrefix}-web-${resourceToken}'
    location: staticWebAppLocation
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
var frontendOrigin = 'https://${staticWebApp.outputs.defaultHostname}'

// User-assigned managed identity the Container App uses to authenticate to the
// ACR. Created up front so its AcrPull role assignment can exist *before* the
// Container App tries to pull an image. A system-assigned identity can't work
// here: its principalId only exists after the app is created, so the AcrPull
// grant would land too late and the first revision 401s on the ACR token
// exchange ("Operation expired").
module apiIdentity 'modules/user-assigned-identity.bicep' = {
  name: 'api-identity'
  params: {
    name: '${namePrefix}-api-id-${resourceToken}'
    location: location
  }
}

// Grant that identity permission to pull images from this specific registry
// (scoped to the ACR, not the whole resource group), avoiding the need to store
// registry admin credentials as a secret. Wrapped in its own module because a
// role assignment's name/scope must be computable "at the start of deployment" -
// a module output (principalId) referenced directly in a raw resource doesn't
// qualify, but the same value passed as a parameter into a nested module does.
module acrPullRoleAssignment 'modules/acr-pull-role-assignment.bicep' = {
  name: 'acr-pull-role-assignment'
  params: {
    containerRegistryName: containerRegistry.outputs.name
    principalId: apiIdentity.outputs.principalId
  }
}

module containerApp 'modules/container-app.bicep' = {
  name: 'container-app'
  params: {
    name: '${namePrefix}-api-${resourceToken}'
    location: location
    environmentId: containerAppsEnvironment.outputs.id
    containerRegistryLoginServer: containerRegistry.outputs.loginServer
    containerImage: apiContainerImage
    userAssignedIdentityId: apiIdentity.outputs.id
    postgresConnectionString: postgresConnectionString
    blobStorageConnectionString: storage.outputs.connectionString
    jwtSecret: jwtSecret
    appInsightsConnectionString: appInsights.outputs.connectionString
    corsAllowedOrigin: frontendOrigin
    azureOpenAiEnabled: deployAzureOpenAi
    #disable-next-line BCP318
    azureOpenAiEndpoint: deployAzureOpenAi ? openAi.outputs.endpoint : ''
    #disable-next-line BCP318
    azureOpenAiApiKey: deployAzureOpenAi ? openAi.outputs.apiKey : ''
    #disable-next-line BCP318
    azureOpenAiChatDeploymentName: deployAzureOpenAi ? openAi.outputs.chatDeploymentName : ''
    #disable-next-line BCP318
    azureOpenAiEmbeddingDeploymentName: deployAzureOpenAi ? openAi.outputs.embeddingDeploymentName : ''
  }
  // Ensure the AcrPull grant is in place before the first revision pulls.
  dependsOn: [
    acrPullRoleAssignment
  ]
}

output apiUrl string = 'https://${containerApp.outputs.fqdn}'
output containerRegistryLoginServer string = containerRegistry.outputs.loginServer
output postgresFullyQualifiedDomainName string = postgres.outputs.fullyQualifiedDomainName
output appInsightsConnectionString string = appInsights.outputs.connectionString
#disable-next-line BCP318
output openAiEndpoint string = deployAzureOpenAi ? openAi.outputs.endpoint : ''
