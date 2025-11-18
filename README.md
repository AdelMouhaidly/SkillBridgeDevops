# SkillBridge – Plataforma de Requalificação e Empregabilidade

SkillBridge é uma API desenvolvida em .NET 8.0 que apoia a solução da Global Solution FIAP 2025/2 para o tema **O Futuro do Trabalho**. Ela oferece gerenciamento completo de usuários, vagas e aplicações, enriquecido com versionamento de rotas, HATEOAS, paginação e um motor de compatibilidade baseado em ML.NET.

## Visão Geral
- **Projeto principal:** `SkillBridge.API` (ASP.NET Core Web API)
- **Testes:** `SkillBridge.Tests` (xUnit)
- **Banco de dados:** SQL Server (EF Core + Migrations)
- **Versionamento:** `/api/v1`, `/api/v2`
- **Funcionalidades chave:**
  - CRUD completo para usuários, vagas e aplicações
  - Paginação, ordenação e HATEOAS em todas as coleções
  - Health check em `/health`
  - Logging configurado (Console + HttpLogging)
  - Tracing via OpenTelemetry (Console exporter)
  - Compatibilidade de competências pelo endpoint `/api/v1/match`

## Pré-requisitos
- .NET SDK 8.0+
- SQL Server local ou remoto
- Ferramentas EF Core (`dotnet tool install --global dotnet-ef` se necessário)

## Configuração
1. Clone o repositório e navegue até o diretório do projeto.
2. Ajuste a connection string em `SkillBridge.API/appsettings.json` conforme o seu ambiente SQL Server.
3. Crie o banco e as tabelas:
   ```bash
   dotnet ef database update --project SkillBridge.API/SkillBridge.API.csproj --startup-project SkillBridge.API/SkillBridge.API.csproj
   ```
4. Execute a solução:
   ```bash
   dotnet run --project SkillBridge.API/SkillBridge.API.csproj
   ```
5. Acesse a documentação interativa no Swagger em `http://localhost:{porta}/swagger`.

## Endpoints Principais
### Versão 1 (`/api/v1`)
- `GET /usuarios` – lista usuários (paginação e ordenação via query string)
- `POST /usuarios` – cria usuário
- `PUT /usuarios/{id}` – atualiza usuário
- `DELETE /usuarios/{id}` – remove usuário
- `GET /vagas` – lista vagas
- `POST /vagas`
- `GET /aplicacoes` – lista aplicações
- `POST /aplicacoes` – cria aplicação com cálculo automático de compatibilidade
- `POST /match` – calcula compatibilidade entre competências e requisitos

### Versão 2 (`/api/v2`)
- `GET /usuarios` – visão resumida de usuários com estatísticas de aplicações

### Health Check
- `GET /health` – retorna status do serviço e timestamp

## Paginação & HATEOAS
- Parâmetros padrão: `pageNumber=1`, `pageSize=10`
- Ordenação: `orderBy` (nome da propriedade) e `sortDirection=asc|desc`
- Cada resposta inclui links hipertextuais para navegação (`self`, `next`, `prev`, recursos relacionados)

## ML.NET – Score de Compatibilidade
- O serviço `MatchService` utiliza `MLContext` com featurização de texto para calcular similaridade e combina resultados com análise léxica.
- Disponível via `POST /api/v1/match` ou ao criar uma aplicação (`POST /api/v1/aplicacoes`).

## Testes Automatizados
Execute todos os testes xUnit:
```bash
dotnet test
```
Os testes cobrem serviços principais (`UsuarioService`, `MatchService`) e repositórios (`AplicacaoRepository`).

## Logging e Tracing
- Logs são emitidos no console e incluem tentativas de criação, atualização e remoção.
- OpenTelemetry está configurado com exportação para o console, permitindo integração futura com Azure Monitor ou Application Insights.

## Build & Deploy
### Build local
```bash
dotnet build
```

### Atualizar o banco
```bash
dotnet ef database update --project SkillBridge.API/SkillBridge.API.csproj --startup-project SkillBridge.API/SkillBridge.API.csproj
```

### Deploy no Azure App Service
1. Configure as variáveis de ambiente em `appsettings.Production.json` (string de conexão via Key Vault ou Configuration).
2. Publique o projeto:
   ```bash
   dotnet publish SkillBridge.API/SkillBridge.API.csproj -c Release -o ./publish
   ```
3. Faça o deploy do conteúdo da pasta `publish` para o App Service.
4. Certifique-se de expor `ASPNETCORE_ENVIRONMENT=Production` e aplicar migrações durante o startup.

