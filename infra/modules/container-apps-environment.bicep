@description('Name of the Container Apps managed environment.')
param name string

@description('Azure region.')
param location string

@description('Log Analytics workspace customer ID (GUID).')
param logAnalyticsCustomerId string

@secure()
@description('Log Analytics shared key.')
param logAnalyticsSharedKey string

resource managedEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: name
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsCustomerId
        sharedKey: logAnalyticsSharedKey
      }
    }
  }
}

output id string = managedEnvironment.id
