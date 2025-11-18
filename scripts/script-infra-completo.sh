#!/bin/bash
# Script Azure CLI completo para criar toda a infraestrutura do SkillBridge (PostgreSQL PaaS)
# Uso: bash scripts/script-infra-completo.sh

RESOURCE_GROUP_NAME="${1:-rg-skillbridge-devops}"
LOCATION="${2:-brazilsouth}"

echo "========================================"
echo "  SkillBridge - Provisionamento Azure  "
echo "  (PostgreSQL PaaS)                    "
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
# 2. CRIAR AZURE DATABASE FOR POSTGRESQL
# ============================================
echo "========================================"
echo "2. Criando Azure Database for PostgreSQL..."
echo "========================================"

POSTGRES_SERVER_NAME="skillbridge-postgres-$(openssl rand -hex 3)"
POSTGRES_DATABASE_NAME="skillbridgedb"
POSTGRES_ADMIN_USER="skillbridgeadmin"
# Gerar senha complexa
LOWER=$(openssl rand -base64 32 | tr -d "=+/" | grep -o '[a-z]' | head -c 1)
UPPER=$(openssl rand -base64 32 | tr -d "=+/" | grep -o '[A-Z]' | head -c 1)
NUMBER=$(openssl rand -base64 32 | tr -d "=+/" | grep -o '[0-9]' | head -c 1)
SPECIAL=$(echo "!@#$%&*" | fold -w1 | shuf | head -c 1)
RANDOM_PART=$(openssl rand -base64 32 | tr -d "=+/" | head -c 12)
POSTGRES_ADMIN_PASSWORD="${LOWER}${UPPER}${NUMBER}${SPECIAL}${RANDOM_PART}"
POSTGRES_ADMIN_PASSWORD=$(echo "$POSTGRES_ADMIN_PASSWORD" | fold -w1 | shuf | tr -d '\n')

echo "PostgreSQL Server: $POSTGRES_SERVER_NAME"
echo "Database: $POSTGRES_DATABASE_NAME"
echo "Admin User: $POSTGRES_ADMIN_USER"
echo ""

# Criar PostgreSQL Server (Flexible Server)
echo "Criando PostgreSQL Server..."
az postgres flexible-server create \
  --resource-group "$RESOURCE_GROUP_NAME" \
  --name "$POSTGRES_SERVER_NAME" \
  --location "$LOCATION" \
  --admin-user "$POSTGRES_ADMIN_USER" \
  --admin-password "$POSTGRES_ADMIN_PASSWORD" \
  --sku-name Standard_B1ms \
  --tier Burstable \
  --version 16 \
  --storage-size 32 \
  --public-access 0.0.0.0 \
  --high-availability Disabled

if [ $? -ne 0 ]; then
    echo "Erro ao criar PostgreSQL Server"
    exit 1
fi

# Criar firewall rule para permitir serviços Azure
echo "Configurando firewall para permitir serviços Azure..."
az postgres flexible-server firewall-rule create \
  --resource-group "$RESOURCE_GROUP_NAME" \
  --name "$POSTGRES_SERVER_NAME" \
  --rule-name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Criar banco de dados
echo "Criando banco de dados..."
az postgres flexible-server db create \
  --resource-group "$RESOURCE_GROUP_NAME" \
  --server-name "$POSTGRES_SERVER_NAME" \
  --database-name "$POSTGRES_DATABASE_NAME"

if [ $? -ne 0 ]; then
    echo "Erro ao criar banco de dados"
    exit 1
fi

echo "PostgreSQL criado com sucesso!"
echo ""

# ============================================
# 3. CRIAR APP SERVICE PLAN E WEB APP
# ============================================
echo "========================================"
echo "3. Criando App Service Plan e Web App..."
echo "========================================"

APP_SERVICE_PLAN_NAME="asp-skillbridge-devops"
WEB_APP_NAME="skillbridge-api-$(openssl rand -hex 3)"
RUNTIME="DOTNETCORE:8.0"

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
echo "AZURE DATABASE FOR POSTGRESQL:"
echo "  Server: $POSTGRES_SERVER_NAME.postgres.database.azure.com"
echo "  Database: $POSTGRES_DATABASE_NAME"
echo "  Admin User: $POSTGRES_ADMIN_USER"
echo "  Admin Password: $POSTGRES_ADMIN_PASSWORD"
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
echo "1. Salve as credenciais do PostgreSQL em variáveis seguras do Azure DevOps"
echo "2. Configure as variáveis no Azure DevOps Variable Group 'skillbridge-variables'"
echo "3. Execute o script SQL no banco de dados usando o Query Editor no Azure Portal"
echo "4. Configure a connection string no App Service via Pipeline"
echo ""
echo "========================================"
