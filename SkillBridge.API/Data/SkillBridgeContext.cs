using Microsoft.EntityFrameworkCore;
using SkillBridge.API.Models;

namespace SkillBridge.API.Data;

public class SkillBridgeContext : DbContext
{
    public SkillBridgeContext(DbContextOptions<SkillBridgeContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Vaga> Vagas => Set<Vaga>();
    public DbSet<Aplicacao> Aplicacoes => Set<Aplicacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.DataCadastro)
                  .HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<Vaga>(entity =>
        {
            entity.Property(v => v.Salario)
                  .HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Aplicacao>(entity =>
        {
            entity.HasIndex(a => new { a.UsuarioId, a.VagaId }).IsUnique();
            entity.Property(a => a.DataAplicacao)
                  .HasDefaultValueSql("GETUTCDATE()");
            entity.Property(a => a.PontuacaoCompatibilidade)
                  .HasDefaultValue(0);

            entity.HasOne(a => a.Usuario)
                  .WithMany(u => u.Aplicacoes)
                  .HasForeignKey(a => a.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Vaga)
                  .WithMany(v => v.Aplicacoes)
                  .HasForeignKey(a => a.VagaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

