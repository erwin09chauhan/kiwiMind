@description('Name of the Azure OpenAI (Cognitive Services) account. Must be globally unique.')
param name string

@description('Azure region. Azure OpenAI is only available in a subset of regions - verify model availability before deploying.')
param location string

@description('Chat completion model deployment name.')
param chatDeploymentName string = 'gpt-5-mini'

@description('Embedding model deployment name.')
param embeddingDeploymentName string = 'text-embedding-3-small'

resource account 'Microsoft.CognitiveServices/accounts@2024-10-01' = {
  name: name
  location: location
  kind: 'OpenAI'
  sku: {
    name: 'S0'
  }
  properties: {
    customSubDomainName: name
    publicNetworkAccess: 'Enabled'
  }
}

resource chatDeployment 'Microsoft.CognitiveServices/accounts/deployments@2024-10-01' = {
  parent: account
  name: chatDeploymentName
  sku: {
    name: 'GlobalStandard'
    capacity: 1
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'gpt-5-mini'
      version: '2025-08-07'
    }
  }
}

resource embeddingDeployment 'Microsoft.CognitiveServices/accounts/deployments@2024-10-01' = {
  parent: account
  name: embeddingDeploymentName
  sku: {
    name: 'GlobalStandard'
    capacity: 1
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'text-embedding-3-small'
      version: '1'
    }
  }
  dependsOn: [
    chatDeployment
  ]
}

output endpoint string = account.properties.endpoint
output chatDeploymentName string = chatDeploymentName
output embeddingDeploymentName string = embeddingDeploymentName
#disable-next-line outputs-should-not-contain-secrets
output apiKey string = account.listKeys().key1
