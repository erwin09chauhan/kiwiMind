@description('Name of the PostgreSQL Flexible Server. Must be globally unique.')
param name string

@description('Azure region.')
param location string

@description('Administrator login name.')
param administratorLogin string

@secure()
@description('Administrator password.')
param administratorPassword string

@description('Name of the application database to create.')
param databaseName string = 'kiwimind'

resource server 'Microsoft.DBforPostgreSQL/flexibleServers@2024-08-01' = {
  name: name
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '16'
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorPassword
    storage: {
      storageSizeGB: 32
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
  }
}

// Allow-list the pgvector extension so "CREATE EXTENSION vector" (run by the
// app's EF Core migration) succeeds. Server-level allow-listing is required
// before an extension can be created inside any database on the server.
resource pgvectorAllowList 'Microsoft.DBforPostgreSQL/flexibleServers/configurations@2024-08-01' = {
  parent: server
  name: 'azure.extensions'
  properties: {
    value: 'VECTOR'
    source: 'user-override'
  }
}

resource database 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-08-01' = {
  parent: server
  name: databaseName
  dependsOn: [
    pgvectorAllowList
  ]
}

// Demo-scoped simplification: Container Apps' outbound IPs aren't static
// without VNET integration, so we allow Azure services rather than pinning
// individual firewall rules. Tighten this with VNET/private endpoint for a
// production deployment.
resource allowAzureServices 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2024-08-01' = {
  parent: server
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

output fullyQualifiedDomainName string = server.properties.fullyQualifiedDomainName
output databaseName string = databaseName
