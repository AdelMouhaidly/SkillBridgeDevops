# SkillBridge - Plataforma de Requalificação e Empregabilidade

## Descrição da Solução

SkillBridge é uma API desenvolvida em .NET 8.0 que apoia a solução da Global Solution FIAP 2025/2 para o tema **O Futuro do Trabalho**. A solução oferece gerenciamento completo de usuários, vagas e aplicações, enriquecido com versionamento de rotas, HATEOAS, paginação e um motor de compatibilidade baseado em ML.NET.

A aplicação está hospedada em nuvem utilizando Azure App Service (PaaS) e Azure Database for PostgreSQL (PaaS), com CI/CD automatizado através do Azure DevOps Pipelines.

## Arquitetura



### Componentes Principais

- **API**: ASP.NET Core Web API (.NET 8.0) hospedada no Azure App Service
- **Banco de Dados**: Azure Database for PostgreSQL (PaaS)
- **CI/CD**: Azure Pipelines (Build + Release automatizado)
- **Versionamento**: Azure Repos (Git)
- **Gerenciamento**: Azure Boards
- **Infraestrutura**: Provisionada via Azure CLI

### Funcionalidades Principais

- CRUD completo para usuários, vagas e aplicações
- Paginação, ordenação e HATEOAS em todas as coleções
- Health check em `/health`
- Versionamento de API (`/api/v1`, `/api/v2`)
- Cálculo automático de compatibilidade entre competências e requisitos (ML.NET)
- Logging configurado (Console + HttpLogging)
- Tracing via OpenTelemetry

## Link da Organização Azure DevOps

[Adicione aqui o link da sua organização Azure DevOps]
Exemplo: https://dev.azure.com/[sua-organizacao]/SkillBridge

## Pré-requisitos

### Desenvolvimento Local

- .NET SDK 8.0 ou superior
- Docker Desktop (para PostgreSQL local) ou PostgreSQL instalado
- Ferramentas EF Core: `dotnet tool install --global dotnet-ef`

### Deploy no Azure

- Azure CLI instalado
- Conta Azure com permissões para criar recursos
- Azure DevOps configurado
- Service Connection configurada no Azure DevOps

## Como Rodar Localmente

### 1. Iniciar PostgreSQL com Docker

**Opção A: Docker Compose (Recomendado)**

```bash
cd dockerfiles
docker-compose up -d
```

**Opção B: Docker Run**

```bash
docker run -d \
  --name skillbridge-postgres \
  -e POSTGRES_DB=skillbridgedb \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 \
  postgres:16-alpine
```

**Opção C: PostgreSQL Instalado Localmente**

Se você já tem PostgreSQL instalado, certifique-se de:

- Criar o banco: `CREATE DATABASE skillbridgedb;`
- Usuário: `postgres` / Senha: `postgres`
- Porta: `5432`

### 2. Executar Migrations

```bash
dotnet ef database update --project SkillBridge.API/SkillBridge.API.csproj
```

Isso criará todas as tabelas no banco de dados.

### 3. Rodar a Aplicação

**Opção A: Visual Studio / VS Code**

- Abra o projeto
- Pressione F5 ou clique em "Run"

**Opção B: Terminal**

```bash
dotnet run --project SkillBridge.API/SkillBridge.API.csproj
```

A aplicação iniciará em:

- HTTP: `http://localhost:5039`
- HTTPS: `https://localhost:7277`

### 4. Acessar a API

- Swagger UI: http://localhost:5039/swagger
- Health Check: http://localhost:5039/health
- API v1: http://localhost:5039/api/v1/usuarios

### Verificar se PostgreSQL está rodando

```bash
# Com Docker
docker ps | grep postgres

# Ou testar conexão
docker exec -it skillbridge-postgres psql -U postgres -d skillbridgedb -c "SELECT version();"
```

## Como Rodar em Produção (Azure)

### 1. Provisionar Infraestrutura

Execute o script de infraestrutura para criar todos os recursos no Azure:

**PowerShell (Windows):**

```powershell
.\scripts\script-infra-completo.ps1
```

**Bash (Linux/Mac):**

```bash
bash scripts/script-infra-completo.sh
```

O script criará automaticamente:

- Resource Group
- Azure Database for PostgreSQL (Flexible Server)
- App Service Plan e Web App

**Importante:** O script exibirá as credenciais geradas. Salve as credenciais do PostgreSQL (Server name, admin user, password) para configurar no Azure DevOps.

### 2. Configurar Banco de Dados

Execute o script SQL no banco criado:

1. Acesse o Portal Azure
2. Vá no seu servidor PostgreSQL
3. Clique em "Query Editor"
4. Execute o conteúdo do arquivo `scripts/script-bd.sql`

### 3. Configurar Azure DevOps

#### 3.1 Criar Projeto no Azure DevOps

1. Acesse https://dev.azure.com
2. Crie um novo projeto
3. Convide o professor com permissões:
   - Organização: Basic
   - Projeto: Contributor

#### 3.2 Configurar Variáveis de Ambiente

No Azure DevOps, vá em **Pipelines > Library** e crie um Variable Group chamado `skillbridge-variables` com:

- `DB_SERVER`: Nome do servidor PostgreSQL (ex: `skillbridge-postgres-xxxx.postgres.database.azure.com`)
- `DB_USER`: Usuário admin do PostgreSQL
- `DB_PASSWORD`: Senha do PostgreSQL (marque como secreto)
- `DB_NAME`: Nome do banco de dados (ex: `skillbridgedb`)
- `WEB_APP_NAME`: Nome da Web App criada
- `WEB_APP_URL`: URL da Web App (ex: `https://skillbridge-api-xxxx.azurewebsites.net`)
- `AZURE_SERVICE_CONNECTION`: Nome da Service Connection do Azure
- `RESOURCE_GROUP_NAME`: Nome do Resource Group (ex: `rg-skillbridge-devops`)

#### 3.3 Criar Service Connection

1. Vá em **Project Settings > Service connections**
2. Crie uma nova conexão do tipo **Azure Resource Manager**
3. Configure com sua subscription do Azure
4. Salve o nome da conexão na variável `AZURE_SERVICE_CONNECTION`

#### 3.4 Configurar Branch Protection

1. Vá em **Repos > Branches**
2. Configure a branch `main` ou `master` com:
   - Revisor obrigatório
   - Vinculação de Work Item obrigatória
   - Revisor padrão (seu RM)

#### 3.5 Configurar Pipeline

1. Vá em **Pipelines > Pipelines**
2. Clique em **New Pipeline**
3. Selecione **Azure Repos Git**
4. Escolha o repositório
5. Selecione **Existing Azure Pipelines YAML file**
6. Escolha o arquivo `azure-pipelines.yml` na raiz
7. Salve e execute

### 4. Deploy Automático

Após configurar a pipeline, o deploy será automático:

1. Faça commit e push na branch `main` (ou via Pull Request)
2. A pipeline de Build será acionada automaticamente
3. Após o Build bem-sucedido, a pipeline de Release será acionada
4. A aplicação será implantada automaticamente no Azure App Service

### 5. Acessar Aplicação em Produção

- Swagger UI: `https://[WEB_APP_NAME].azurewebsites.net/swagger`
- Health Check: `https://[WEB_APP_NAME].azurewebsites.net/health`
- API v1: `https://[WEB_APP_NAME].azurewebsites.net/api/v1/usuarios`

## Endpoints da API

### Versão 1 (`/api/v1`)

#### Usuários

- `GET /api/v1/usuarios` - Lista usuários (paginação e ordenação via query string)
- `GET /api/v1/usuarios/{id}` - Obtém usuário por ID
- `POST /api/v1/usuarios` - Cria usuário
- `PUT /api/v1/usuarios/{id}` - Atualiza usuário
- `DELETE /api/v1/usuarios/{id}` - Remove usuário

#### Vagas

- `GET /api/v1/vagas` - Lista vagas (paginação e ordenação)
- `GET /api/v1/vagas/{id}` - Obtém vaga por ID
- `POST /api/v1/vagas` - Cria vaga
- `PUT /api/v1/vagas/{id}` - Atualiza vaga
- `DELETE /api/v1/vagas/{id}` - Remove vaga

#### Aplicações

- `GET /api/v1/aplicacoes` - Lista aplicações (paginação e ordenação)
- `GET /api/v1/aplicacoes/{id}` - Obtém aplicação por ID
- `GET /api/v1/aplicacoes/usuario/{usuarioId}` - Lista aplicações de um usuário
- `GET /api/v1/aplicacoes/vaga/{vagaId}` - Lista aplicações de uma vaga
- `POST /api/v1/aplicacoes` - Cria aplicação com cálculo automático de compatibilidade
- `PUT /api/v1/aplicacoes/{id}` - Atualiza aplicação
- `DELETE /api/v1/aplicacoes/{id}` - Remove aplicação

#### Match

- `POST /api/v1/match` - Calcula compatibilidade entre competências e requisitos

### Versão 2 (`/api/v2`)

- `GET /api/v2/usuarios` - Visão resumida de usuários com estatísticas de aplicações

### Health Check

- `GET /health` - Retorna status do serviço e timestamp

## Exemplos de CRUD em JSON

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

```
GET /api/v1/usuarios?pageNumber=1&pageSize=10&orderBy=Nome&sortDirection=asc
```

#### READ - Obter Usuário por ID

```
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

```
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

```
GET /api/v1/vagas?pageNumber=1&pageSize=10&orderBy=Titulo&sortDirection=asc
```

#### READ - Obter Vaga por ID

```
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

```
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

```
GET /api/v1/aplicacoes?pageNumber=1&pageSize=10&orderBy=DataAplicacao&sortDirection=desc
```

#### READ - Obter Aplicação por ID

```
GET /api/v1/aplicacoes/{id}
```

#### READ - Listar Aplicações por Usuário

```
GET /api/v1/aplicacoes/usuario/{usuarioId}
```

#### READ - Listar Aplicações por Vaga

```
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

```
DELETE /api/v1/aplicacoes/{id}
```

## Paginação e HATEOAS

Todas as coleções suportam paginação e ordenação:

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

## ML.NET - Score de Compatibilidade

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

## Testes Automatizados

Execute todos os testes xUnit:

```bash
dotnet test
```

Os testes cobrem serviços principais (`UsuarioService`, `MatchService`) e repositórios (`AplicacaoRepository`).

## CI/CD Pipeline

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

O pipeline é acionado automaticamente após merge de Pull Request na branch `main`.

## Estrutura de Arquivos

```
SkillBridgeNET/
├── azure-pipelines.yml          # Pipeline YAML para CI/CD
├── README.md                     # Este arquivo
├── scripts/
│   ├── script-infra-completo.ps1 # Script completo para criar toda infraestrutura (Windows)
│   ├── script-infra-completo.sh  # Script completo para criar toda infraestrutura (Linux/Mac)
│   ├── script-deletar-recursos.ps1 # Script para deletar recursos (Windows)
│   ├── script-deletar-recursos.sh  # Script para deletar recursos (Linux/Mac)
│   ├── script-bd.sql             # Script SQL para criar tabelas
│   └── exemplos-crud.json        # Exemplos de requisições CRUD
├── dockerfiles/
│   ├── docker-compose.yml        # Docker Compose para PostgreSQL local
│   └── Dockerfile                # Dockerfile para containerização (opcional)
├── SkillBridge.API/             # Projeto principal da API
└── SkillBridge.Tests/           # Projeto de testes
```

## Segurança

- Variáveis sensíveis (senhas, connection strings) são armazenadas como secrets no Azure DevOps
- Connection strings não são commitadas no repositório
- Utilização de variáveis de ambiente para configuração
- SSL/TLS habilitado para conexões com o banco de dados

## Referências

- [Documentação .NET 8](https://learn.microsoft.com/dotnet/)
- [Azure App Service](https://learn.microsoft.com/azure/app-service/)
- [Azure Database for PostgreSQL](https://learn.microsoft.com/azure/postgresql/)
- [Azure Pipelines](https://learn.microsoft.com/azure/devops/pipelines/)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)

## Autores


- Afonso Correia Pereira - RM557863 - 2TDSA
- Adel Mouhaidly - RM557705 - 2TDSA
- Tiago Augusto Desiderato - RM558485 - 2TDSB

## Licença

Este projeto foi desenvolvido para a Global Solution FIAP 2025/2.
