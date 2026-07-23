@description('Name of the Container App.')
param name string

@description('Azure region.')
param location string

@description('Resource ID of the Container Apps managed environment.')
param environmentId string

@description('Login server of the Azure Container Registry (e.g. myregistry.azurecr.io).')
param containerRegistryLoginServer string

@description('Full container image reference, e.g. myregistry.azurecr.io/kiwimind-api:latest.')
param containerImage string

@description('Resource ID of the user-assigned managed identity used to pull images from the ACR. Must already hold AcrPull on the registry before this app is created.')
param userAssignedIdentityId string

@description('Target port the container listens on.')
param targetPort int = 8080

@secure()
param postgresConnectionString string

@secure()
param blobStorageConnectionString string

@secure()
param jwtSecret string

param jwtIssuer string = 'KiwiMind'
param jwtAudience string = 'KiwiMind'

param appInsightsConnectionString string = ''

@description('Origin allowed to call this API via CORS (the deployed frontend URL).')
param corsAllowedOrigin string = ''

@description('Enable real Azure OpenAI providers instead of the Fake ones. Requires the other azureOpenAi* params to be set.')
param azureOpenAiEnabled bool = false

param azureOpenAiEndpoint string = ''

@secure()
param azureOpenAiApiKey string = ''

param azureOpenAiChatDeploymentName string = ''
param azureOpenAiEmbeddingDeploymentName string = ''

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: name
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentityId}': {}
    }
  }
  properties: {
    environmentId: environmentId
    configuration: {
      ingress: {
        external: true
        targetPort: targetPort
        transport: 'auto'
        allowInsecure: false
      }
      registries: [
        {
          server: containerRegistryLoginServer
          identity: userAssignedIdentityId
        }
      ]
      secrets: [
        { name: 'connectionstrings--default', value: postgresConnectionString }
        { name: 'blobstorage--connectionstring', value: blobStorageConnectionString }
        { name: 'jwt--secret', value: jwtSecret }
        { name: 'azureopenai--apikey', value: azureOpenAiApiKey }
      ]
    }
    template: {
      containers: [
        {
          name: 'api'
          image: containerImage
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            { name: 'ASPNETCORE_URLS', value: 'http://+:${targetPort}' }
            { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
            { name: 'ConnectionStrings__Default', secretRef: 'connectionstrings--default' }
            { name: 'BlobStorage__ConnectionString', secretRef: 'blobstorage--connectionstring' }
            { name: 'BlobStorage__ContainerName', value: 'documents' }
            { name: 'Jwt__Secret', secretRef: 'jwt--secret' }
            { name: 'Jwt__Issuer', value: jwtIssuer }
            { name: 'Jwt__Audience', value: jwtAudience }
            { name: 'Cors__AllowedOrigins__0', value: corsAllowedOrigin }
            { name: 'ApplicationInsights__ConnectionString', value: appInsightsConnectionString }
            { name: 'AzureOpenAI__Enabled', value: string(azureOpenAiEnabled) }
            { name: 'AzureOpenAI__Endpoint', value: azureOpenAiEndpoint }
            { name: 'AzureOpenAI__ApiKey', secretRef: 'azureopenai--apikey' }
            { name: 'AzureOpenAI__ChatDeploymentName', value: azureOpenAiChatDeploymentName }
            { name: 'AzureOpenAI__EmbeddingDeploymentName', value: azureOpenAiEmbeddingDeploymentName }
          ]
          // No custom readiness probe: this app-specific /health path only
          // exists on the real kiwimind-api image, not the bootstrap
          // placeholder used for the very first deploy (before CI/CD has
          // pushed anything). Container Apps' default TCP-level readiness
          // check works for both.
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 3
      }
    }
  }
}

output id string = containerApp.id
output fqdn string = containerApp.properties.configuration.ingress.fqdn
