# SkillBridge – Plataforma de Requalificação e Empregabilidade

SkillBridge é uma API desenvolvida em .NET 8.0 que apoia a solução da Global Solution FIAP 2025/2 para o tema **O Futuro do Trabalho**. Ela oferece gerenciamento completo de usuários, vagas e aplicações, enriquecido com versionamento de rotas, HATEOAS, paginação e um motor de compatibilidade baseado em ML.NET.

## 🏗️ Arquitetura

A solução utiliza uma arquitetura em camadas com os seguintes componentes:

```
┌─────────────────────────────────────────────────────────────┐
│                    Azure App Service                         │
│              (SkillBridge API - .NET 8.0)                    │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       │ Entity Framework Core
                       │
┌──────────────────────▼──────────────────────────────────────┐
│                  Azure SQL Database                          │
│              (SkillBridgeDb - PaaS)                          │
└─────────────────────────────────────────────────────────────┘

Fluxo de CI/CD:
┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐
│  Azure   │───▶│  Azure   │───▶│  Azure   │───▶│  Azure   │
│  Repos   │    │  Boards  │    │ Pipeline │    │   App    │
│          │    │          │    │  Build   │    │ Service  │
└──────────┘    └──────────┘    └────┬─────┘    └──────────┘
                                     │
                                     ▼
                              ┌──────────┐
                              │  Azure   │
                              │ Pipeline │
                              │ Release  │
                              └──────────┘
```

### Componentes Principais

- **Frontend/API**: ASP.NET Core Web API (.NET 8.0)
- **Banco de Dados**: Azure SQL Database (PaaS)
- **CI/CD**: Azure Pipelines (Build + Release)
- **Versionamento**: Azure Repos (Git)
- **Gerenciamento**: Azure Boards
- **Infraestrutura**: Provisionada via Azure CLI

## 📋 Visão Geral

- **Projeto principal:** `SkillBridge.API` (ASP.NET Core Web API)
- **Testes:** `SkillBridge.Tests` (xUnit)
- **Banco de dados:** Azure SQL Database (EF Core + Migrations)
- **Versionamento:** `/api/v1`, `/api/v2`
- **Funcionalidades chave:**
  - CRUD completo para usuários, vagas e aplicações
  - Paginação, ordenação e HATEOAS em todas as coleções
  - Health check em `/health`
  - Logging configurado (Console + HttpLogging)
  - Tracing via OpenTelemetry (Console exporter)
  - Compatibilidade de competências pelo endpoint `/api/v1/match`

## 🚀 Pré-requisitos

### Desenvolvimento Local

- .NET SDK 8.0+
- SQL Server local ou remoto
- Ferramentas EF Core (`dotnet tool install --global dotnet-ef`)

### Deploy no Azure

- Azure CLI instalado
- Conta Azure com permissões para criar recursos
- Azure DevOps configurado

## ⚙️ Configuração Local

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

## ☁️ Provisionamento de Infraestrutura no Azure

### Criar Toda a Infraestrutura

**PowerShell (Windows):**

```powershell
.\scripts\script-infra-completo.ps1
```

**Bash (Linux/Mac):**

```bash
bash scripts/script-infra-completo.sh
```

**Importante:** O script criará todos os recursos e exibirá as credenciais geradas. Salve as credenciais do SQL Server (Server name, admin user, password) para configurar no Azure DevOps.

O script criará automaticamente:

- Resource Group
- Azure SQL Server e Database
- App Service Plan e Web App

### Configurar Banco de Dados

Execute o script SQL no banco criado:

```sql
-- Conecte-se ao Azure SQL Database e execute:
-- scripts/script-bd.sql
```

## 🔧 Configuração do Azure DevOps

### 1. Criar Projeto no Azure DevOps

1. Acesse https://dev.azure.com
2. Crie um novo projeto
3. Convide o professor com permissões:
   - Organização: Basic
   - Projeto: Contributor

### 2. Configurar Variáveis de Ambiente

No Azure DevOps, vá em **Pipelines > Library** e crie um Variable Group chamado `skillbridge-variables` com:

- `DB_SERVER`: Nome do servidor SQL (ex: `skillbridge-sql-server-xxxx.database.windows.net`)
- `DB_USER`: Usuário admin do SQL
- `DB_PASSWORD`: Senha do SQL (marque como secreto)
- `DB_CONNECTION_STRING`: Connection string completa (marque como secreto)
- `WEB_APP_NAME`: Nome da Web App criada
- `WEB_APP_URL`: URL da Web App (ex: `https://skillbridge-api-xxxx.azurewebsites.net`)
- `AZURE_SERVICE_CONNECTION`: Nome da Service Connection do Azure

### 3. Criar Service Connection

1. Vá em **Project Settings > Service connections**
2. Crie uma nova conexão do tipo **Azure Resource Manager**
3. Configure com sua subscription do Azure
4. Salve o nome da conexão na variável `AZURE_SERVICE_CONNECTION`

### 4. Configurar Branch Protection

1. Vá em **Repos > Branches**
2. Configure a branch `main` ou `master` com:
   - Revisor obrigatório
   - Vinculação de Work Item obrigatória
   - Revisor padrão (seu RM)

## 📝 Endpoints Principais

### Versão 1 (`/api/v1`)

#### Usuários

- `GET /api/v1/usuarios` – lista usuários (paginação e ordenação via query string)
- `GET /api/v1/usuarios/{id}` – obtém usuário por ID
- `POST /api/v1/usuarios` – cria usuário
- `PUT /api/v1/usuarios/{id}` – atualiza usuário
- `DELETE /api/v1/usuarios/{id}` – remove usuário

#### Vagas

- `GET /api/v1/vagas` – lista vagas
- `GET /api/v1/vagas/{id}` – obtém vaga por ID
- `POST /api/v1/vagas` – cria vaga
- `PUT /api/v1/vagas/{id}` – atualiza vaga
- `DELETE /api/v1/vagas/{id}` – remove vaga

#### Aplicações

- `GET /api/v1/aplicacoes` – lista aplicações
- `GET /api/v1/aplicacoes/{id}` – obtém aplicação por ID
- `GET /api/v1/aplicacoes/usuario/{usuarioId}` – lista aplicações de um usuário
- `GET /api/v1/aplicacoes/vaga/{vagaId}` – lista aplicações de uma vaga
- `POST /api/v1/aplicacoes` – cria aplicação com cálculo automático de compatibilidade
- `PUT /api/v1/aplicacoes/{id}` – atualiza aplicação
- `DELETE /api/v1/aplicacoes/{id}` – remove aplicação

#### Match

- `POST /api/v1/match` – calcula compatibilidade entre competências e requisitos

### Versão 2 (`/api/v2`)

- `GET /api/v2/usuarios` – visão resumida de usuários com estatísticas de aplicações

### Health Check

- `GET /health` – retorna status do serviço e timestamp

## 📋 Exemplos de CRUD em JSON

### Tabela: Usuarios

#### CREATE - Criar Usuário

```json
POST /api/v1/usuarios
Content-Type: application/json

{
  "nome": "João Silva",
  "email": "joao.silva@email.com",
  "competencias": "C#, .NET, SQL Server, Azure, Entity Framework"
}
```

#### READ - Listar Usuários

```json
GET /api/v1/usuarios?pageNumber=1&pageSize=10&orderBy=Nome&sortDirection=asc
```

#### READ - Obter Usuário por ID

```json
GET /api/v1/usuarios/{id}
```

#### UPDATE - Atualizar Usuário

```json
PUT /api/v1/usuarios/{id}
Content-Type: application/json

{
  "nome": "João Silva Santos",
  "email": "joao.silva@email.com",
  "competencias": "C#, .NET, SQL Server, Azure, Entity Framework, Docker"
}
```

#### DELETE - Remover Usuário

```json
DELETE /api/v1/usuarios/{id}
```

### Tabela: Vagas

#### CREATE - Criar Vaga

```json
POST /api/v1/vagas
Content-Type: application/json

{
  "titulo": "Desenvolvedor .NET Senior",
  "empresa": "TechCorp",
  "requisitos": "C#, .NET 8, Azure, SQL Server, Entity Framework, Docker",
  "salario": 12000.00,
  "tipoContrato": "CLT"
}
```

#### READ - Listar Vagas

```json
GET /api/v1/vagas?pageNumber=1&pageSize=10&orderBy=Titulo&sortDirection=asc
```

#### READ - Obter Vaga por ID

```json
GET /api/v1/vagas/{id}
```

#### UPDATE - Atualizar Vaga

```json
PUT /api/v1/vagas/{id}
Content-Type: application/json

{
  "titulo": "Desenvolvedor .NET Senior",
  "empresa": "TechCorp Solutions",
  "requisitos": "C#, .NET 8, Azure, SQL Server, Entity Framework, Docker, Kubernetes",
  "salario": 15000.00,
  "tipoContrato": "CLT"
}
```

#### DELETE - Remover Vaga

```json
DELETE /api/v1/vagas/{id}
```

### Tabela: Aplicacoes

#### CREATE - Criar Aplicação

```json
POST /api/v1/aplicacoes
Content-Type: application/json

{
  "usuarioId": "550e8400-e29b-41d4-a716-446655440000",
  "vagaId": "6ba7b810-9dad-11d1-80b4-00c04fd430c8"
}
```

#### READ - Listar Aplicações

```json
GET /api/v1/aplicacoes?pageNumber=1&pageSize=10&orderBy=DataAplicacao&sortDirection=desc
```

#### READ - Obter Aplicação por ID

```json
GET /api/v1/aplicacoes/{id}
```

#### READ - Listar Aplicações por Usuário

```json
GET /api/v1/aplicacoes/usuario/{usuarioId}
```

#### READ - Listar Aplicações por Vaga

```json
GET /api/v1/aplicacoes/vaga/{vagaId}
```

#### UPDATE - Atualizar Aplicação

```json
PUT /api/v1/aplicacoes/{id}
Content-Type: application/json

{
  "pontuacaoCompatibilidade": 85.5
}
```

#### DELETE - Remover Aplicação

```json
DELETE /api/v1/aplicacoes/{id}
```

## 📄 Paginação & HATEOAS

- Parâmetros padrão: `pageNumber=1`, `pageSize=10`
- Ordenação: `orderBy` (nome da propriedade) e `sortDirection=asc|desc`
- Cada resposta inclui links hipertextuais para navegação (`self`, `next`, `prev`, recursos relacionados)

Exemplo de resposta paginada:

```json
{
  "items": [...],
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 5,
  "totalCount": 50,
  "links": [
    {
      "href": "/api/v1/usuarios?pageNumber=1",
      "rel": "self",
      "method": "GET"
    },
    {
      "href": "/api/v1/usuarios?pageNumber=2",
      "rel": "next",
      "method": "GET"
    }
  ]
}
```

## 🤖 ML.NET – Score de Compatibilidade

O serviço `MatchService` utiliza `MLContext` com featurização de texto para calcular similaridade e combina resultados com análise léxica.

Disponível via `POST /api/v1/match` ou ao criar uma aplicação (`POST /api/v1/aplicacoes`).

Exemplo de requisição de match:

```json
POST /api/v1/match
Content-Type: application/json

{
  "competencias": "C#, .NET, SQL Server, Azure",
  "requisitos": "C#, .NET 8, Azure, SQL Server, Entity Framework"
}
```

## 🧪 Testes Automatizados

Execute todos os testes xUnit:

```bash
dotnet test
```

Os testes cobrem serviços principais (`UsuarioService`, `MatchService`) e repositórios (`AplicacaoRepository`).

## 📊 Logging e Tracing

- Logs são emitidos no console e incluem tentativas de criação, atualização e remoção.
- OpenTelemetry está configurado com exportação para o console, permitindo integração futura com Azure Monitor ou Application Insights.

## 🔄 CI/CD Pipeline

O pipeline está configurado no arquivo `azure-pipelines.yml` e inclui:

### Build Stage

- Restauração de dependências
- Compilação do projeto
- Execução de testes unitários
- Publicação de resultados de testes e cobertura de código
- Publicação de artefatos

### Release Stage

- Execução de migrações do banco de dados
- Deploy para Azure App Service
- Health check pós-deploy

### Configuração do Pipeline

1. No Azure DevOps, vá em **Pipelines > Pipelines**
2. Clique em **New Pipeline**
3. Selecione **Azure Repos Git**
4. Escolha o repositório
5. Selecione **Existing Azure Pipelines YAML file**
6. Escolha o arquivo `azure-pipelines.yml` na raiz
7. Configure as variáveis necessárias (veja seção de variáveis acima)

## 📁 Estrutura de Arquivos

```
SkillBridgeNET/
├── azure-pipelines.yml          # Pipeline YAML para CI/CD
├── README.md                     # Este arquivo
├── scripts/
│   ├── script-infra-completo.ps1 # Script completo para criar toda infraestrutura (Windows)
│   ├── script-infra-completo.sh  # Script completo para criar toda infraestrutura (Linux/Mac)
│   ├── script-bd.sql             # Script SQL para criar tabelas
│   └── exemplos-crud.json        # Exemplos de requisições CRUD
├── SkillBridge.API/             # Projeto principal da API
└── SkillBridge.Tests/           # Projeto de testes
```

## 🔐 Segurança

- Variáveis sensíveis (senhas, connection strings) são armazenadas como secrets no Azure DevOps
- Connection strings não são commitadas no repositório
- Utilização de variáveis de ambiente para configuração

## 📚 Referências

- [Documentação .NET 8](https://learn.microsoft.com/dotnet/)
- [Azure App Service](https://learn.microsoft.com/azure/app-service/)
- [Azure SQL Database](https://learn.microsoft.com/azure/azure-sql/)
- [Azure Pipelines](https://learn.microsoft.com/azure/devops/pipelines/)

## 👥 Autores

- [Nome do Grupo]
- [RM e Nome dos Integrantes]

## 📄 Licença

Este projeto foi desenvolvido para a Global Solution FIAP 2025/2.
