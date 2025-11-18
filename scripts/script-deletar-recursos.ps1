# Script para deletar todos os recursos do Azure criados pelo script-infra-completo
# Uso: .\scripts\script-deletar-recursos.ps1

param(
    [string]$ResourceGroupName = "rg-skillbridge-devops"
)

Write-Host "========================================" -ForegroundColor Red
Write-Host "  DELETANDO RECURSOS DO AZURE (PaaS)   " -ForegroundColor Red
Write-Host "========================================" -ForegroundColor Red
Write-Host ""
Write-Host "ATENÇÃO: Esta operação é IRREVERSÍVEL!" -ForegroundColor Yellow
Write-Host "Todos os recursos no Resource Group '$ResourceGroupName' serão deletados." -ForegroundColor Yellow
Write-Host "Isso inclui:" -ForegroundColor Yellow
Write-Host "  - Azure Database for PostgreSQL" -ForegroundColor White
Write-Host "  - Azure App Service" -ForegroundColor White
Write-Host "  - App Service Plan" -ForegroundColor White
Write-Host ""

$confirmation = Read-Host "Digite 'SIM' para confirmar a exclusão"

if ($confirmation -ne "SIM") {
    Write-Host "Operação cancelada." -ForegroundColor Green
    exit 0
}

Write-Host ""
Write-Host "Deletando Resource Group: $ResourceGroupName..." -ForegroundColor Yellow
Write-Host "Isso pode levar alguns minutos..." -ForegroundColor Yellow
Write-Host ""

az group delete `
    --name $ResourceGroupName `
    --yes `
    --no-wait

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "Resource Group marcado para exclusão!" -ForegroundColor Green
    Write-Host "A exclusão está sendo processada em background." -ForegroundColor Green
    Write-Host ""
    Write-Host "Para verificar o status:" -ForegroundColor Yellow
    Write-Host "  az group show --name $ResourceGroupName" -ForegroundColor Gray
} else {
    Write-Host ""
    Write-Host "Erro ao deletar Resource Group" -ForegroundColor Red
    Write-Host "Verifique se o Resource Group existe e se você tem permissões." -ForegroundColor Yellow
    exit 1
}

