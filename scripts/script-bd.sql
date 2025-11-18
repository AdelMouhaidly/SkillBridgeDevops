-- Script SQL para criação do banco de dados SkillBridge
-- Este script cria as tabelas e índices necessários
-- Uso: Execute este script no Azure SQL Database após criar o banco

USE [SkillBridgeDb]
GO

-- Tabela Usuarios
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Usuarios]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Usuarios] (
        [Id] uniqueidentifier NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [Nome] nvarchar(120) NOT NULL,
        [Email] nvarchar(180) NOT NULL,
        [Competencias] nvarchar(max) NOT NULL,
        [DataCadastro] datetime2 NOT NULL DEFAULT GETUTCDATE()
    )
    
    CREATE UNIQUE INDEX [IX_Usuarios_Email] ON [dbo].[Usuarios] ([Email])
END
GO

-- Tabela Vagas
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Vagas]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Vagas] (
        [Id] uniqueidentifier NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [Titulo] nvarchar(180) NOT NULL,
        [Empresa] nvarchar(180) NOT NULL,
        [Requisitos] nvarchar(max) NOT NULL,
        [Salario] decimal(18,2) NOT NULL,
        [TipoContrato] nvarchar(80) NOT NULL
    )
END
GO

-- Tabela Aplicacoes
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Aplicacoes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Aplicacoes] (
        [Id] uniqueidentifier NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [UsuarioId] uniqueidentifier NOT NULL,
        [VagaId] uniqueidentifier NOT NULL,
        [DataAplicacao] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [PontuacaoCompatibilidade] float NOT NULL DEFAULT 0.0,
        CONSTRAINT [FK_Aplicacoes_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) 
            REFERENCES [dbo].[Usuarios] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Aplicacoes_Vagas_VagaId] FOREIGN KEY ([VagaId]) 
            REFERENCES [dbo].[Vagas] ([Id]) ON DELETE CASCADE
    )
    
    CREATE UNIQUE INDEX [IX_Aplicacoes_UsuarioId_VagaId] ON [dbo].[Aplicacoes] ([UsuarioId], [VagaId])
    CREATE INDEX [IX_Aplicacoes_VagaId] ON [dbo].[Aplicacoes] ([VagaId])
END
GO

-- Dados de exemplo (opcional)
IF NOT EXISTS (SELECT * FROM [dbo].[Usuarios])
BEGIN
    INSERT INTO [dbo].[Usuarios] ([Id], [Nome], [Email], [Competencias]) VALUES
    (NEWID(), 'João Silva', 'joao.silva@email.com', 'C#, .NET, SQL Server, Azure'),
    (NEWID(), 'Maria Santos', 'maria.santos@email.com', 'Java, Spring Boot, PostgreSQL, Docker')
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[Vagas])
BEGIN
    INSERT INTO [dbo].[Vagas] ([Id], [Titulo], [Empresa], [Requisitos], [Salario], [TipoContrato]) VALUES
    (NEWID(), 'Desenvolvedor .NET Senior', 'TechCorp', 'C#, .NET 8, Azure, SQL Server, Entity Framework', 12000.00, 'CLT'),
    (NEWID(), 'Desenvolvedor Full Stack', 'StartupXYZ', 'C#, JavaScript, React, Azure, Docker', 10000.00, 'PJ')
END
GO

PRINT 'Banco de dados SkillBridgeDb configurado com sucesso!'
GO

