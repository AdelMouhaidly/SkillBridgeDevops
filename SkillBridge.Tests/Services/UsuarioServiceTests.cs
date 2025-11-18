using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SkillBridge.API.DTOs;
using SkillBridge.API.DTOs.Usuarios;
using SkillBridge.API.Repositories.Implementations;
using SkillBridge.API.Services;
using SkillBridge.Tests.Common;
using Xunit;

namespace SkillBridge.Tests.Services;

public class UsuarioServiceTests
{
    private readonly Mock<ILogger<UsuarioService>> _loggerMock = new();

    [Fact]
    public async Task CreateAsync_ShouldCreateUser_WhenEmailIsUnique()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        var repository = new UsuarioRepository(context);
        var service = new UsuarioService(repository, _loggerMock.Object);
        var request = new CreateUsuarioRequest
        {
            Nome = "Alice",
            Email = "alice@skillbridge.io",
            Competencias = "C#, Azure"
        };

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(request.Email.ToLowerInvariant());
        context.Usuarios.Should().Contain(u => u.Email == request.Email.ToLowerInvariant());
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenEmailAlreadyExists()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        context.Usuarios.Add(new SkillBridge.API.Models.Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Existing",
            Email = "existing@skillbridge.io",
            Competencias = "SQL"
        });
        await context.SaveChangesAsync();

        var repository = new UsuarioRepository(context);
        var service = new UsuarioService(repository, _loggerMock.Object);
        var request = new CreateUsuarioRequest
        {
            Nome = "New",
            Email = "existing@skillbridge.io",
            Competencias = "C#"
        };

        // Act
        var action = () => service.CreateAsync(request);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Já existe um usuário cadastrado com este e-mail.");
    }

    [Fact]
    public async Task GetAsync_ShouldReturnPagedData()
    {
        // Arrange
        await using var context = TestDbContextFactory.CreateContext();
        for (var i = 0; i < 15; i++)
        {
            context.Usuarios.Add(new SkillBridge.API.Models.Usuario
            {
                Id = Guid.NewGuid(),
                Nome = $"Usuario {i}",
                Email = $"usuario{i}@mail.com",
                Competencias = "C#, Azure"
            });
        }
        await context.SaveChangesAsync();

        var repository = new UsuarioRepository(context);
        var service = new UsuarioService(repository, _loggerMock.Object);
        var query = new PaginationQuery(PageNumber: 2, PageSize: 5, OrderBy: nameof(SkillBridge.API.Models.Usuario.Nome), SortDirection: "asc");

        // Act
        var result = await service.GetAsync(query);

        // Assert
        result.Items.Should().HaveCount(5);
        result.PageNumber.Should().Be(2);
        result.TotalCount.Should().Be(15);
        result.TotalPages.Should().Be(3);
    }
}
