# Script Azure CLI completo para criar toda a infraestrutura do SkillBridge (PostgreSQL PaaS)
# Uso: .\scripts\script-infra-completo.ps1

param(
    [string]$ResourceGroupName = "rg-skillbridge-devops",
    [string]$Location = "brazilsouth"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  SkillBridge - Provisionamento Azure  " -ForegroundColor Cyan
Write-Host "  (PostgreSQL PaaS)                    " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar se está logado no Azure
Write-Host "Verificando login no Azure..." -ForegroundColor Yellow
$account = az account show 2>$null | ConvertFrom-Json
if (-not $account) {
    Write-Host "Erro: Não está logado no Azure. Execute 'az login' primeiro." -ForegroundColor Red
    exit 1
}
Write-Host "Logado como: $($account.user.name)" -ForegroundColor Green
Write-Host "Subscription: $($account.name)" -ForegroundColor Green
Write-Host ""

# ============================================
# 1. CRIAR RESOURCE GROUP
# ============================================
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "1. Criando Resource Group..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Nome: $ResourceGroupName" -ForegroundColor White
Write-Host "Região: $Location" -ForegroundColor White
Write-Host ""

az group create `
  --name $ResourceGroupName `
  --location $Location

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao criar Resource Group" -ForegroundColor Red
    exit 1
}

Write-Host "Resource Group criado com sucesso!" -ForegroundColor Green
Write-Host ""

# ============================================
# 2. CRIAR AZURE DATABASE FOR POSTGRESQL
# ============================================
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "2. Criando Azure Database for PostgreSQL..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$POSTGRES_SERVER_NAME = "skillbridge-postgres-$(Get-Random -Minimum 1000 -Maximum 9999)"
$POSTGRES_DATABASE_NAME = "skillbridgedb"
$POSTGRES_ADMIN_USER = "skillbridgeadmin"
# Gerar senha complexa que atende aos requisitos do PostgreSQL
$lowercase = "abcdefghijklmnopqrstuvwxyz"
$uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
$numbers = "0123456789"
$special = "!@#$%&*"
$allChars = $lowercase + $uppercase + $numbers + $special

# Garantir pelo menos um de cada tipo
$passwordParts = @(
    (Get-Random -InputObject $lowercase.ToCharArray() -Count 1),
    (Get-Random -InputObject $uppercase.ToCharArray() -Count 1),
    (Get-Random -InputObject $numbers.ToCharArray() -Count 1),
    (Get-Random -InputObject $special.ToCharArray() -Count 1)
)

# Adicionar mais caracteres aleatórios para completar 16 caracteres
$passwordParts += Get-Random -InputObject $allChars.ToCharArray() -Count 12

# Embaralhar os caracteres
$POSTGRES_ADMIN_PASSWORD = -join ($passwordParts | Sort-Object { Get-Random })

Write-Host "PostgreSQL Server: $POSTGRES_SERVER_NAME" -ForegroundColor White
Write-Host "Database: $POSTGRES_DATABASE_NAME" -ForegroundColor White
Write-Host "Admin User: $POSTGRES_ADMIN_USER" -ForegroundColor White
Write-Host ""

# Criar PostgreSQL Server (Flexible Server)
Write-Host "Criando PostgreSQL Server..." -ForegroundColor Yellow
az postgres flexible-server create `
  --resource-group $ResourceGroupName `
  --name $POSTGRES_SERVER_NAME `
  --location $Location `
  --admin-user $POSTGRES_ADMIN_USER `
  --admin-password $POSTGRES_ADMIN_PASSWORD `
  --sku-name Standard_B1ms `
  --tier Burstable `
  --version 16 `
  --storage-size 32 `
  --public-access 0.0.0.0 `
  --high-availability Disabled

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao criar PostgreSQL Server" -ForegroundColor Red
    exit 1
}

# Criar firewall rule para permitir serviços Azure
Write-Host "Configurando firewall para permitir serviços Azure..." -ForegroundColor Yellow
az postgres flexible-server firewall-rule create `
  --resource-group $ResourceGroupName `
  --name $POSTGRES_SERVER_NAME `
  --rule-name AllowAzureServices `
  --start-ip-address 0.0.0.0 `
  --end-ip-address 0.0.0.0

# Criar banco de dados
Write-Host "Criando banco de dados..." -ForegroundColor Yellow
az postgres flexible-server db create `
  --resource-group $ResourceGroupName `
  --server-name $POSTGRES_SERVER_NAME `
  --database-name $POSTGRES_DATABASE_NAME

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao criar banco de dados" -ForegroundColor Red
    exit 1
}

Write-Host "PostgreSQL criado com sucesso!" -ForegroundColor Green
Write-Host ""

# ============================================
# 3. CRIAR APP SERVICE PLAN E WEB APP
# ============================================
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "3. Criando App Service Plan e Web App..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$APP_SERVICE_PLAN_NAME = "asp-skillbridge-devops"
$WEB_APP_NAME = "skillbridge-api-$(Get-Random -Minimum 1000 -Maximum 9999)"
$RUNTIME = "DOTNETCORE:8.0"

Write-Host "App Service Plan: $APP_SERVICE_PLAN_NAME" -ForegroundColor White
Write-Host "Web App: $WEB_APP_NAME" -ForegroundColor White
Write-Host "Runtime: $RUNTIME" -ForegroundColor White
Write-Host ""

# Criar App Service Plan
Write-Host "Criando App Service Plan..." -ForegroundColor Yellow
az appservice plan create `
  --name $APP_SERVICE_PLAN_NAME `
  --resource-group $ResourceGroupName `
  --location $Location `
  --sku B1 `
  --is-linux

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao criar App Service Plan" -ForegroundColor Red
    exit 1
}

# Criar Web App
Write-Host "Criando Web App..." -ForegroundColor Yellow
az webapp create `
  --resource-group $ResourceGroupName `
  --plan $APP_SERVICE_PLAN_NAME `
  --name $WEB_APP_NAME `
  --runtime $RUNTIME

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao criar Web App" -ForegroundColor Red
    exit 1
}

Write-Host "Web App criada com sucesso!" -ForegroundColor Green
Write-Host ""

# ============================================
# RESUMO FINAL
# ============================================
Write-Host "========================================" -ForegroundColor Green
Write-Host "  INFRAESTRUTURA CRIADA COM SUCESSO!    " -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "RESOURCE GROUP:" -ForegroundColor Cyan
Write-Host "  Nome: $ResourceGroupName" -ForegroundColor White
Write-Host "  Região: $Location" -ForegroundColor White
Write-Host ""
Write-Host "AZURE DATABASE FOR POSTGRESQL:" -ForegroundColor Cyan
Write-Host "  Server: $POSTGRES_SERVER_NAME.postgres.database.azure.com" -ForegroundColor White
Write-Host "  Database: $POSTGRES_DATABASE_NAME" -ForegroundColor White
Write-Host "  Admin User: $POSTGRES_ADMIN_USER" -ForegroundColor White
Write-Host "  Admin Password: $POSTGRES_ADMIN_PASSWORD" -ForegroundColor Yellow
Write-Host ""
Write-Host "WEB APP:" -ForegroundColor Cyan
Write-Host "  Nome: $WEB_APP_NAME" -ForegroundColor White
Write-Host "  URL: https://$WEB_APP_NAME.azurewebsites.net" -ForegroundColor White
Write-Host "  App Service Plan: $APP_SERVICE_PLAN_NAME" -ForegroundColor White
Write-Host ""
Write-Host "========================================" -ForegroundColor Yellow
Write-Host "  PRÓXIMOS PASSOS:" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Salve as credenciais do PostgreSQL em variáveis seguras do Azure DevOps:" -ForegroundColor White
Write-Host "   - DB_SERVER: $POSTGRES_SERVER_NAME.postgres.database.azure.com" -ForegroundColor Gray
Write-Host "   - DB_USER: $POSTGRES_ADMIN_USER" -ForegroundColor Gray
Write-Host "   - DB_PASSWORD: $POSTGRES_ADMIN_PASSWORD (marque como secret)" -ForegroundColor Gray
Write-Host "   - DB_NAME: $POSTGRES_DATABASE_NAME" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Configure as variáveis no Azure DevOps Variable Group 'skillbridge-variables':" -ForegroundColor White
Write-Host "   - DB_SERVER" -ForegroundColor Gray
Write-Host "   - DB_USER" -ForegroundColor Gray
Write-Host "   - DB_PASSWORD (marque como secret)" -ForegroundColor Gray
Write-Host "   - DB_NAME" -ForegroundColor Gray
Write-Host "   - WEB_APP_NAME: $WEB_APP_NAME" -ForegroundColor Gray
Write-Host "   - WEB_APP_URL: https://$WEB_APP_NAME.azurewebsites.net" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Execute o script SQL no banco de dados:" -ForegroundColor White
Write-Host "   scripts/script-bd.sql" -ForegroundColor Gray
Write-Host "   Use o Query Editor no Azure Portal" -ForegroundColor Gray
Write-Host ""
Write-Host "4. Configure a connection string no App Service via Pipeline" -ForegroundColor White
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
