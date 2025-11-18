#!/bin/bash
# Script Azure CLI completo para criar toda a infraestrutura do SkillBridge (Bash)
# Uso: bash script-infra-completo.sh
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

# Variáveis (ajustar conforme necessário)
RESOURCE_GROUP_NAME="${1:-rg-skillbridge-devops}"
LOCATION="${2:-brazilsouth}"

echo "========================================"
echo "  SkillBridge - Provisionamento Azure  "
echo "========================================"
echo ""

# Verificar se está logado no Azure
echo "Verificando login no Azure..."
if ! az account show &>/dev/null; then
    echo "Erro: Não está logado no Azure. Execute 'az login' primeiro."
    exit 1
fi

ACCOUNT_NAME=$(az account show --query user.name -o tsv)
SUBSCRIPTION_NAME=$(az account show --query name -o tsv)
echo "Logado como: $ACCOUNT_NAME"
echo "Subscription: $SUBSCRIPTION_NAME"
echo ""

# ============================================
# 1. CRIAR RESOURCE GROUP
# ============================================
echo "========================================"
echo "1. Criando Resource Group..."
echo "========================================"
echo "Nome: $RESOURCE_GROUP_NAME"
echo "Região: $LOCATION"
echo ""

az group create \
  --name "$RESOURCE_GROUP_NAME" \
  --location "$LOCATION"

if [ $? -ne 0 ]; then
    echo "Erro ao criar Resource Group"
    exit 1
fi

echo "Resource Group criado com sucesso!"
echo ""

# ============================================
# 2. CRIAR AZURE SQL SERVER E DATABASE
# ============================================
echo "========================================"
echo "2. Criando Azure SQL Server e Database..."
echo "========================================"

SQL_SERVER_NAME="skillbridge-sql-$(openssl rand -hex 3)"
SQL_DATABASE_NAME="SkillBridgeDb"
SQL_ADMIN_USER="sqladmin"
# Gerar senha complexa que atende aos requisitos do Azure SQL
# Requisitos: min 8 caracteres, 1 maiúscula, 1 minúscula, 1 número, 1 caractere especial
# Gerar partes obrigatórias
LOWER=$(openssl rand -base64 32 | tr -d "=+/" | grep -o '[a-z]' | head -c 1)
UPPER=$(openssl rand -base64 32 | tr -d "=+/" | grep -o '[A-Z]' | head -c 1)
NUMBER=$(openssl rand -base64 32 | tr -d "=+/" | grep -o '[0-9]' | head -c 1)
SPECIAL=$(echo "!@#$%&*" | fold -w1 | shuf | head -c 1)
# Gerar parte aleatória adicional
RANDOM_PART=$(openssl rand -base64 32 | tr -d "=+/" | head -c 12)
# Combinar e embaralhar
SQL_ADMIN_PASSWORD="${LOWER}${UPPER}${NUMBER}${SPECIAL}${RANDOM_PART}"
SQL_ADMIN_PASSWORD=$(echo "$SQL_ADMIN_PASSWORD" | fold -w1 | shuf | tr -d '\n')

echo "SQL Server: $SQL_SERVER_NAME"
echo "Database: $SQL_DATABASE_NAME"
echo "Admin User: $SQL_ADMIN_USER"
echo ""

# Criar SQL Server
echo "Criando SQL Server..."
az sql server create \
  --resource-group "$RESOURCE_GROUP_NAME" \
  --name "$SQL_SERVER_NAME" \
  --location "$LOCATION" \
  --admin-user "$SQL_ADMIN_USER" \
  --admin-password "$SQL_ADMIN_PASSWORD"

if [ $? -ne 0 ]; then
    echo "Erro ao criar SQL Server"
    exit 1
fi

# Configurar firewall para permitir serviços Azure
echo "Configurando firewall para permitir serviços Azure..."
az sql server firewall-rule create \
  --resource-group "$RESOURCE_GROUP_NAME" \
  --server "$SQL_SERVER_NAME" \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Criar banco de dados
echo "Criando banco de dados..."
az sql db create \
  --resource-group "$RESOURCE_GROUP_NAME" \
  --server "$SQL_SERVER_NAME" \
  --name "$SQL_DATABASE_NAME" \
  --service-objective Basic \
  --backup-storage-redundancy Local

if [ $? -ne 0 ]; then
    echo "Erro ao criar banco de dados"
    exit 1
fi

echo "Azure SQL criado com sucesso!"
echo ""

# ============================================
# 3. CRIAR APP SERVICE PLAN E WEB APP
# ============================================
echo "========================================"
echo "3. Criando App Service Plan e Web App..."
echo "========================================"

APP_SERVICE_PLAN_NAME="asp-skillbridge-devops"
WEB_APP_NAME="skillbridge-api-$(openssl rand -hex 3)"
RUNTIME="DOTNET:8.0"

echo "App Service Plan: $APP_SERVICE_PLAN_NAME"
echo "Web App: $WEB_APP_NAME"
echo "Runtime: $RUNTIME"
echo ""

# Criar App Service Plan
echo "Criando App Service Plan..."
az appservice plan create \
  --name "$APP_SERVICE_PLAN_NAME" \
  --resource-group "$RESOURCE_GROUP_NAME" \
  --location "$LOCATION" \
  --sku B1 \
  --is-linux

if [ $? -ne 0 ]; then
    echo "Erro ao criar App Service Plan"
    exit 1
fi

# Criar Web App
echo "Criando Web App..."
az webapp create \
  --resource-group "$RESOURCE_GROUP_NAME" \
  --plan "$APP_SERVICE_PLAN_NAME" \
  --name "$WEB_APP_NAME" \
  --runtime "$RUNTIME"

if [ $? -ne 0 ]; then
    echo "Erro ao criar Web App"
    exit 1
fi

echo "Web App criada com sucesso!"
echo ""

# ============================================
# RESUMO FINAL
# ============================================
echo "========================================"
echo "  INFRAESTRUTURA CRIADA COM SUCESSO!    "
echo "========================================"
echo ""
echo "RESOURCE GROUP:"
echo "  Nome: $RESOURCE_GROUP_NAME"
echo "  Região: $LOCATION"
echo ""
echo "AZURE SQL DATABASE:"
echo "  Server: $SQL_SERVER_NAME.database.windows.net"
echo "  Database: $SQL_DATABASE_NAME"
echo "  Admin User: $SQL_ADMIN_USER"
echo "  Admin Password: $SQL_ADMIN_PASSWORD"
echo ""
echo "WEB APP:"
echo "  Nome: $WEB_APP_NAME"
echo "  URL: https://$WEB_APP_NAME.azurewebsites.net"
echo "  App Service Plan: $APP_SERVICE_PLAN_NAME"
echo ""
echo "========================================"
echo "  PRÓXIMOS PASSOS:"
echo "========================================"
echo ""
echo "1. Salve as credenciais do SQL Server em variáveis seguras do Azure DevOps:"
echo "   - DB_SERVER: $SQL_SERVER_NAME.database.windows.net"
echo "   - DB_USER: $SQL_ADMIN_USER"
echo "   - DB_PASSWORD: $SQL_ADMIN_PASSWORD"
echo ""
echo "2. Configure as variáveis no Azure DevOps Variable Group 'skillbridge-variables':"
echo "   - DB_SERVER"
echo "   - DB_USER"
echo "   - DB_PASSWORD (marque como secret)"
echo "   - WEB_APP_NAME: $WEB_APP_NAME"
echo "   - WEB_APP_URL: https://$WEB_APP_NAME.azurewebsites.net"
echo ""
echo "3. Execute o script SQL no banco de dados:"
echo "   scripts/script-bd.sql"
echo ""
echo "4. Configure a connection string no App Service:"
echo "   ConnectionStrings__DefaultConnection"
echo ""
echo "========================================"

