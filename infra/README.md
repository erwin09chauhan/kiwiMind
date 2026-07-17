# Infrastructure (Bicep)

Provisions the Azure resources for KiwiMind: Container Apps (API), PostgreSQL
Flexible Server (with pgvector allow-listed), Blob Storage, Container
Registry, Application Insights/Log Analytics, and optionally Azure OpenAI.

## Layout

```
infra/
├── main.bicep              # orchestrator - wires all modules together
├── main.bicepparam         # example parameters (no secrets committed)
└── modules/
    ├── log-analytics.bicep
    ├── app-insights.bicep
    ├── container-registry.bicep
    ├── postgres.bicep
    ├── storage.bicep
    ├── openai.bicep                    # optional, see below
    ├── container-apps-environment.bicep
    └── container-app.bicep
```

## Prerequisites

- Azure CLI with the Bicep tooling: `az bicep install`
- An Azure subscription and a resource group to deploy into
- **Validate before deploying** — these templates were written without a local
  Bicep compiler available; run `az bicep build --file main.bicep` first to
  catch any syntax issues before attempting a real deployment

## First deploy

The Container App needs an image reference at creation time, but the image
doesn't exist in the registry until CI/CD has pushed at least once. `main.bicep`
defaults `apiContainerImage` to a public Microsoft sample image so the first
`az deployment group create` succeeds; the CI/CD pipeline then updates the
Container App to the real image on every push to `main`.

```bash
az group create --name rg-kiwimind-dev --location eastus

az deployment group create \
  --resource-group rg-kiwimind-dev \
  --template-file main.bicep \
  --parameters main.bicepparam \
  --parameters postgresAdminPassword='<generate-a-strong-password>' jwtSecret='<generate-a-random-secret>'
```

After the deployment finishes, apply the EF Core migrations against the new
database (the app doesn't run migrations automatically):

```bash
dotnet ef database update \
  --project ../src/KiwiMind.Infrastructure \
  --startup-project ../src/KiwiMind.Api \
  --connection "Host=<postgresFullyQualifiedDomainName output>;Database=kiwimind;Username=kiwimind_admin;Password=<your-password>;Ssl Mode=Require"
```

## Azure OpenAI

`deployAzureOpenAi` defaults to `false`. Azure OpenAI requires the subscription
to have been granted access first (see `kiwiMind-build-sheet.md` section 9 -
"Request Azure OpenAI access early"). The app itself currently uses local
fake embedding/chat providers (`IEmbeddingService`/`IChatCompletionService`)
for cost-free demo purposes; this module provisions the resource and model
deployments for when those providers are swapped for the real Azure OpenAI
implementation, but nothing in the app reads its outputs yet.

## CI/CD (`.github/workflows/ci-cd.yml`)

Build and test always run on every push/PR - no Azure access needed for that.
The build/push-image, deploy-api, and deploy-frontend jobs are gated behind
repository secrets/variables so they no-op (rather than fail) until you've
provisioned real Azure resources:

**Repository variables** (Settings → Secrets and variables → Actions → Variables):

| Variable | Example | Used for |
|---|---|---|
| `ACR_NAME` | `kiwiminddevacrxxxxx` | `az acr login` |
| `ACR_LOGIN_SERVER` | `kiwiminddevacrxxxxx.azurecr.io` | image tag, gates the push/deploy jobs |
| `CONTAINER_APP_NAME` | `kiwiminddev-api-xxxxx` | `az containerapp update` |
| `AZURE_RESOURCE_GROUP` | `rg-kiwimind-dev` | `az containerapp update` |

(Values for the first three come from this Bicep deployment's outputs.)

**Repository secrets**:

| Secret | Used for |
|---|---|
| `AZURE_CREDENTIALS` | `azure/login@v2` - a service principal JSON (`az ad sp create-for-rbac --sdk-auth`), scoped to the resource group with `Contributor` + `AcrPush` |
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | Frontend deploy job, if hosting on Azure Static Web Apps - swap for a different deploy step if you'd rather use Cloudflare Pages or another target |

## Known simplifications (portfolio/demo scope, not production-hardened)

- **Postgres firewall** allows all Azure services (`0.0.0.0`-`0.0.0.0`) rather
  than a VNET/private endpoint, since Container Apps' outbound IPs aren't
  static without VNET integration. Tighten this for a real production
  deployment.
- **Storage account key** is used for the Blob connection string (matches
  `BlobStorageSettings.ConnectionString`, the same shape as local dev's
  Azurite connection string). A production setup would prefer a managed
  identity + RBAC instead of an account key.
- **No `az bicep build` validation** was possible in the environment these
  templates were authored in (no Bicep CLI installed) - validate locally
  before your first deployment.
