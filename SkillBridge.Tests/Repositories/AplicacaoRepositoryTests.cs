using FluentAssertions;
using SkillBridge.API.DTOs;
using SkillBridge.API.Models;
using SkillBridge.API.Repositories.Implementations;
using SkillBridge.Tests.Common;
using Xunit;

namespace SkillBridge.Tests.Repositories;

public class AplicacaoRepositoryTests
{
    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedApplicationsWithIncludes()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var usuario = new Usuario { Id = Guid.NewGuid(), Nome = "Test", Email = "test@mail.com", Competencias = "C#" };
        var vaga = new Vaga { Id = Guid.NewGuid(), Titulo = "Dev", Empresa = "SkillBridge", Requisitos = "C#", TipoContrato = "CLT" };
        context.Usuarios.Add(usuario);
        context.Vagas.Add(vaga);

        for (var i = 0; i < 12; i++)
        {
            context.Aplicacoes.Add(new Aplicacao
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuario.Id,
                VagaId = vaga.Id,
                DataAplicacao = DateTime.UtcNow.AddDays(-i),
                PontuacaoCompatibilidade = 50 + i
            });
        }

        await context.SaveChangesAsync();
        var repository = new AplicacaoRepository(context);
        var query = new PaginationQuery(PageNumber: 2, PageSize: 5, OrderBy: nameof(Aplicacao.DataAplicacao), SortDirection: "desc");

        // Act
        var (items, total) = await repository.GetPagedAsync(query);

        // Assert
        items.Should().HaveCount(5);
        total.Should().Be(12);
        items.Should().OnlyContain(a => a.Usuario != null && a.Vaga != null);
    }

    [Fact]
    public async Task GetByUsuarioEVagaAsync_ShouldReturnExistingApplication()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var usuario = new Usuario { Id = Guid.NewGuid(), Nome = "User", Email = "user@mail.com", Competencias = "SQL" };
        var vaga = new Vaga { Id = Guid.NewGuid(), Titulo = "DBA", Empresa = "SkillBridge", Requisitos = "SQL", TipoContrato = "PJ" };
        var aplicacao = new Aplicacao
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuario.Id,
            VagaId = vaga.Id,
            DataAplicacao = DateTime.UtcNow,
            PontuacaoCompatibilidade = 80
        };

        context.Usuarios.Add(usuario);
        context.Vagas.Add(vaga);
        context.Aplicacoes.Add(aplicacao);
        await context.SaveChangesAsync();

        var repository = new AplicacaoRepository(context);

        // Act
        var result = await repository.GetByUsuarioEVagaAsync(usuario.Id, vaga.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Usuario.Should().NotBeNull();
        result.Vaga.Should().NotBeNull();
    }
}
