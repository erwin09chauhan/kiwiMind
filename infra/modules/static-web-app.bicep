@description('Name of the Static Web App.')
param name string

@description('Azure region. Static Web Apps are only available in a subset of regions.')
param location string

resource staticWebApp 'Microsoft.Web/staticSites@2024-04-01' = {
  name: name
  location: location
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {
    // Deployed via GitHub Actions ourselves (ci-cd.yml), not SWA's built-in
    // repo integration, so no repositoryUrl/branch/buildProperties here.
    provider: 'Custom'
  }
}

output name string = staticWebApp.name
output defaultHostname string = staticWebApp.properties.defaultHostname
#disable-next-line outputs-should-not-contain-secrets
output apiKey string = staticWebApp.listSecrets().properties.apiKey
