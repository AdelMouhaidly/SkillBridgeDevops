-- Script SQL para criação do banco de dados SkillBridge (PostgreSQL)
-- Este script cria as tabelas e índices necessários
-- Uso: Execute este script no Azure Database for PostgreSQL após criar o banco

-- Tabela Usuarios
CREATE TABLE IF NOT EXISTS "Usuarios" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "Nome" VARCHAR(120) NOT NULL,
    "Email" VARCHAR(180) NOT NULL,
    "Competencias" TEXT NOT NULL,
    "DataCadastro" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Índice único para Email
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Usuarios_Email" ON "Usuarios" ("Email");

-- Tabela Vagas
CREATE TABLE IF NOT EXISTS "Vagas" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "Titulo" VARCHAR(180) NOT NULL,
    "Empresa" VARCHAR(180) NOT NULL,
    "Requisitos" TEXT NOT NULL,
    "Salario" DECIMAL(18,2) NOT NULL,
    "TipoContrato" VARCHAR(80) NOT NULL
);

-- Tabela Aplicacoes
CREATE TABLE IF NOT EXISTS "Aplicacoes" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "UsuarioId" UUID NOT NULL,
    "VagaId" UUID NOT NULL,
    "DataAplicacao" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "PontuacaoCompatibilidade" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    CONSTRAINT "FK_Aplicacoes_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") 
        REFERENCES "Usuarios" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Aplicacoes_Vagas_VagaId" FOREIGN KEY ("VagaId") 
        REFERENCES "Vagas" ("Id") ON DELETE CASCADE
);

-- Índices para Aplicacoes
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Aplicacoes_UsuarioId_VagaId" ON "Aplicacoes" ("UsuarioId", "VagaId");
CREATE INDEX IF NOT EXISTS "IX_Aplicacoes_VagaId" ON "Aplicacoes" ("VagaId");

-- Dados de exemplo (opcional)
INSERT INTO "Usuarios" ("Id", "Nome", "Email", "Competencias") 
SELECT gen_random_uuid(), 'João Silva', 'joao.silva@email.com', 'C#, .NET, SQL Server, Azure'
WHERE NOT EXISTS (SELECT 1 FROM "Usuarios" WHERE "Email" = 'joao.silva@email.com');

INSERT INTO "Usuarios" ("Id", "Nome", "Email", "Competencias") 
SELECT gen_random_uuid(), 'Maria Santos', 'maria.santos@email.com', 'Java, Spring Boot, PostgreSQL, Docker'
WHERE NOT EXISTS (SELECT 1 FROM "Usuarios" WHERE "Email" = 'maria.santos@email.com');

-- Verificar se já existem vagas antes de inserir
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "Vagas" LIMIT 1) THEN
        INSERT INTO "Vagas" ("Id", "Titulo", "Empresa", "Requisitos", "Salario", "TipoContrato") VALUES
        (gen_random_uuid(), 'Desenvolvedor .NET Senior', 'TechCorp', 'C#, .NET 8, Azure, SQL Server, Entity Framework', 12000.00, 'CLT'),
        (gen_random_uuid(), 'Desenvolvedor Full Stack', 'StartupXYZ', 'C#, JavaScript, React, Azure, Docker', 10000.00, 'PJ');
    END IF;
END $$;

-- Mensagem de sucesso
DO $$
BEGIN
    RAISE NOTICE 'Banco de dados SkillBridgeDb configurado com sucesso!';
END $$;

