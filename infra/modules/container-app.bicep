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

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: name
  location: location
  identity: {
    type: 'SystemAssigned'
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
          identity: 'system'
        }
      ]
      secrets: [
        { name: 'connectionstrings--default', value: postgresConnectionString }
        { name: 'blobstorage--connectionstring', value: blobStorageConnectionString }
        { name: 'jwt--secret', value: jwtSecret }
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
          ]
          probes: [
            {
              type: 'Readiness'
              httpGet: {
                path: '/health'
                port: targetPort
              }
              initialDelaySeconds: 5
              periodSeconds: 10
            }
          ]
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
output principalId string = containerApp.identity.principalId
output fqdn string = containerApp.properties.configuration.ingress.fqdn
