#!/bin/bash
# Script para deletar todos os recursos do Azure criados pelo script-infra-completo
# Uso: bash scripts/script-deletar-recursos.sh

RESOURCE_GROUP_NAME="${1:-rg-skillbridge-devops}"

echo "========================================"
echo "  DELETANDO RECURSOS DO AZURE (PaaS)   "
echo "========================================"
echo ""
echo "ATENÇÃO: Esta operação é IRREVERSÍVEL!"
echo "Todos os recursos no Resource Group '$RESOURCE_GROUP_NAME' serão deletados."
echo "Isso inclui:"
echo "  - Azure Database for PostgreSQL"
echo "  - Azure App Service"
echo "  - App Service Plan"
echo ""

read -p "Digite 'SIM' para confirmar a exclusão: " confirmation

if [ "$confirmation" != "SIM" ]; then
    echo "Operação cancelada."
    exit 0
fi

echo ""
echo "Deletando Resource Group: $RESOURCE_GROUP_NAME..."
echo "Isso pode levar alguns minutos..."
echo ""

az group delete \
    --name "$RESOURCE_GROUP_NAME" \
    --yes \
    --no-wait

if [ $? -eq 0 ]; then
    echo ""
    echo "Resource Group marcado para exclusão!"
    echo "A exclusão está sendo processada em background."
    echo ""
    echo "Para verificar o status:"
    echo "  az group show --name $RESOURCE_GROUP_NAME"
else
    echo ""
    echo "Erro ao deletar Resource Group"
    echo "Verifique se o Resource Group existe e se você tem permissões."
    exit 1
fi

