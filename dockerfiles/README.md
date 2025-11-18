# Docker para Desenvolvimento Local

## PostgreSQL Local com Docker

Para rodar PostgreSQL localmente usando Docker:

### Opção 1: Docker Compose (Recomendado)

```bash
# Iniciar PostgreSQL
docker-compose -f dockerfiles/docker-compose.yml up -d

# Parar PostgreSQL
docker-compose -f dockerfiles/docker-compose.yml down

# Ver logs
docker-compose -f dockerfiles/docker-compose.yml logs -f
```

### Opção 2: Docker Run

```bash
docker run -d \
  --name skillbridge-postgres \
  -e POSTGRES_DB=SkillBridgeDb \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 \
  postgres:16-alpine
```

### Connection String Local

Após iniciar o PostgreSQL, use esta connection string no `appsettings.Development.json`:

```json
"DefaultConnection": "Host=localhost;Database=SkillBridgeDb;Username=postgres;Password=postgres"
```

## Aplicação em Docker (Opcional)

Para rodar a aplicação completa em Docker:

```bash
# Build da imagem
docker build -t skillbridge-api -f dockerfiles/Dockerfile .

# Rodar container
docker run -d \
  --name skillbridge-api \
  -p 8080:80 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Database=SkillBridgeDb;Username=postgres;Password=postgres" \
  skillbridge-api
```

## Nota Importante

**Para produção no Azure, NÃO é necessário Docker!**

O Azure Database for PostgreSQL é um serviço PaaS (Platform as a Service) gerenciado, então:
- ✅ Não precisa instalar PostgreSQL
- ✅ Não precisa gerenciar o servidor
- ✅ Não precisa Docker
- ✅ Apenas use a connection string fornecida pelo Azure

Os arquivos Docker aqui são **apenas para desenvolvimento local**, se você quiser testar sem usar o Azure.

