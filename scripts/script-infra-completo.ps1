# Script Azure CLI completo para criar toda a infraestrutura do SkillBridge (PowerShell)
# Uso: .\script-infra-completo.ps1
# 
# Este script cria:
# 1. Resource Group
# 2. Azure SQL Server e Database
# 3. App Service Plan e Web App
#
# Pré-requisitos:
# - Azure CLI instalado
# - Login no Azure (az login)
# - Permissões para criar recursos na subscription

param(
    [string]$ResourceGroupName = "rg-skillbridge-devops",
    [string]$Location = "brazilsouth"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  SkillBridge - Provisionamento Azure  " -ForegroundColor Cyan
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
# 2. CRIAR AZURE SQL SERVER E DATABASE
# ============================================
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "2. Criando Azure SQL Server e Database..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$SQL_SERVER_NAME = "skillbridge-sql-$(Get-Random -Minimum 1000 -Maximum 9999)"
$SQL_DATABASE_NAME = "SkillBridgeDb"
$SQL_ADMIN_USER = "sqladmin"
# Gerar senha complexa que atende aos requisitos do Azure SQL
# Requisitos: min 8 caracteres, 1 maiúscula, 1 minúscula, 1 número, 1 caractere especial
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
$SQL_ADMIN_PASSWORD = -join ($passwordParts | Sort-Object { Get-Random })

Write-Host "SQL Server: $SQL_SERVER_NAME" -ForegroundColor White
Write-Host "Database: $SQL_DATABASE_NAME" -ForegroundColor White
Write-Host "Admin User: $SQL_ADMIN_USER" -ForegroundColor White
Write-Host ""

# Criar SQL Server
Write-Host "Criando SQL Server..." -ForegroundColor Yellow
az sql server create `
  --resource-group $ResourceGroupName `
  --name $SQL_SERVER_NAME `
  --location $Location `
  --admin-user $SQL_ADMIN_USER `
  --admin-password $SQL_ADMIN_PASSWORD

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao criar SQL Server" -ForegroundColor Red
    exit 1
}

# Configurar firewall para permitir serviços Azure
Write-Host "Configurando firewall para permitir serviços Azure..." -ForegroundColor Yellow
az sql server firewall-rule create `
  --resource-group $ResourceGroupName `
  --server $SQL_SERVER_NAME `
  --name AllowAzureServices `
  --start-ip-address 0.0.0.0 `
  --end-ip-address 0.0.0.0

# Criar banco de dados
Write-Host "Criando banco de dados..." -ForegroundColor Yellow
az sql db create `
  --resource-group $ResourceGroupName `
  --server $SQL_SERVER_NAME `
  --name $SQL_DATABASE_NAME `
  --service-objective Basic `
  --backup-storage-redundancy Local

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao criar banco de dados" -ForegroundColor Red
    exit 1
}

Write-Host "Azure SQL criado com sucesso!" -ForegroundColor Green
Write-Host ""

# ============================================
# 3. CRIAR APP SERVICE PLAN E WEB APP
# ============================================
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "3. Criando App Service Plan e Web App..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$APP_SERVICE_PLAN_NAME = "asp-skillbridge-devops"
$WEB_APP_NAME = "skillbridge-api-$(Get-Random -Minimum 1000 -Maximum 9999)"
$RUNTIME = "DOTNET:8.0"

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
Write-Host "AZURE SQL DATABASE:" -ForegroundColor Cyan
Write-Host "  Server: $SQL_SERVER_NAME.database.windows.net" -ForegroundColor White
Write-Host "  Database: $SQL_DATABASE_NAME" -ForegroundColor White
Write-Host "  Admin User: $SQL_ADMIN_USER" -ForegroundColor White
Write-Host "  Admin Password: $SQL_ADMIN_PASSWORD" -ForegroundColor Yellow
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
Write-Host "1. Salve as credenciais do SQL Server em variáveis seguras do Azure DevOps:" -ForegroundColor White
Write-Host "   - DB_SERVER: $SQL_SERVER_NAME.database.windows.net" -ForegroundColor Gray
Write-Host "   - DB_USER: $SQL_ADMIN_USER" -ForegroundColor Gray
Write-Host "   - DB_PASSWORD: $SQL_ADMIN_PASSWORD" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Configure as variáveis no Azure DevOps Variable Group 'skillbridge-variables':" -ForegroundColor White
Write-Host "   - DB_SERVER" -ForegroundColor Gray
Write-Host "   - DB_USER" -ForegroundColor Gray
Write-Host "   - DB_PASSWORD (marque como secret)" -ForegroundColor Gray
Write-Host "   - WEB_APP_NAME: $WEB_APP_NAME" -ForegroundColor Gray
Write-Host "   - WEB_APP_URL: https://$WEB_APP_NAME.azurewebsites.net" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Execute o script SQL no banco de dados:" -ForegroundColor White
Write-Host "   scripts/script-bd.sql" -ForegroundColor Gray
Write-Host ""
Write-Host "4. Configure a connection string no App Service:" -ForegroundColor White
Write-Host "   ConnectionStrings__DefaultConnection" -ForegroundColor Gray
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan

