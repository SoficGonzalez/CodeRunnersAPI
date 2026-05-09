using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SGA.Identity;
using SGA.Models;

namespace SGA.Data;

public class SgaDbContext : IdentityDbContext<AppIdentityUser>
{
    public SgaDbContext(DbContextOptions<SgaDbContext> options) : base(options)
    {
    }

    public DbSet<EstadoPlantilla> EstadosPlantilla => Set<EstadoPlantilla>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Modulo> Modulos => Set<Modulo>();
    public DbSet<RolModulo> RolModulos => Set<RolModulo>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Plantilla> Plantillas => Set<Plantilla>();
    public DbSet<CampoPlantilla> CamposPlantilla => Set<CampoPlantilla>();
    public DbSet<DocumentoLlenado> DocumentosLlenados => Set<DocumentoLlenado>();
    public DbSet<ValorCampo> ValoresCampo => Set<ValorCampo>();
    public DbSet<ArchivoEvidencia> ArchivosEvidencia => Set<ArchivoEvidencia>();
    public DbSet<Sede> Sedes => Set<Sede>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<EstadoPlantilla>(entity =>
        {
            entity.ToTable("EstadoPlantilla");
            entity.HasKey(e => e.EstadoPlantillaId);
            entity.HasIndex(e => e.Codigo).IsUnique();
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("Rol");
            entity.HasKey(e => e.RolId);
            entity.HasIndex(e => e.NombreRol).IsUnique();
        });

        modelBuilder.Entity<Modulo>(entity =>
        {
            entity.ToTable("Modulo");
            entity.HasKey(e => e.ModuloId);
            entity.HasIndex(e => e.CodigoModulo).IsUnique();
        });

        modelBuilder.Entity<RolModulo>(entity =>
        {
            entity.ToTable("RolModulo");
            entity.HasKey(e => e.RolModuloId);
            entity.HasIndex(e => new { e.RolId, e.ModuloId }).IsUnique();

            entity.HasOne(e => e.Rol)
                  .WithMany(r => r.RolModulos)
                  .HasForeignKey(e => e.RolId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Modulo)
                  .WithMany(m => m.RolModulos)
                  .HasForeignKey(e => e.ModuloId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuario");
            entity.HasKey(e => e.UsuarioId);
            entity.HasIndex(e => e.NombreUsuario).IsUnique();
            entity.HasIndex(e => e.Correo).IsUnique();
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime2");

            entity.HasOne(e => e.Rol)
                  .WithMany(r => r.Usuarios)
                  .HasForeignKey(e => e.RolId)
                  .OnDelete(DeleteBehavior.Restrict);

           entity.HasOne(e => e.Sede)
          .WithMany(s => s.Usuarios)
          .HasForeignKey(e => e.SedeId)
          .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Sede>(entity =>
        {
            entity.ToTable("Sede");
            entity.HasKey(e => e.SedeId);
            entity.HasIndex(e => e.Nombre).IsUnique();
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime2");
        });

        modelBuilder.Entity<Plantilla>(entity =>
        {
            entity.ToTable("Plantilla");
            entity.HasKey(e => e.PlantillaId);
            entity.HasIndex(e => e.Nombre);
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime2");
            entity.Property(e => e.FechaActualizacion).HasColumnType("datetime2");

            entity.HasOne(e => e.EstadoPlantilla)
                  .WithMany(es => es.Plantillas)
                  .HasForeignKey(e => e.EstadoPlantillaId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CreadoPor)
                  .WithMany(u => u.PlantillasCreadas)
                  .HasForeignKey(e => e.CreadoPorUsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CampoPlantilla>(entity =>
        {
            entity.ToTable("CampoPlantilla");
            entity.HasKey(e => e.CampoPlantillaId);
            entity.HasIndex(e => new { e.PlantillaId, e.ClaveCampo }).IsUnique();
            entity.HasIndex(e => new { e.PlantillaId, e.Orden });

            entity.HasOne(e => e.Plantilla)
                  .WithMany(p => p.Campos)
                  .HasForeignKey(e => e.PlantillaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DocumentoLlenado>(entity =>
        {
            entity.ToTable("DocumentoLlenado");
            entity.HasKey(e => e.DocumentoLlenadoId);
            entity.HasIndex(e => e.PlantillaId);
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime2");

            entity.HasOne(e => e.Plantilla)
                  .WithMany(p => p.Llenados)
                  .HasForeignKey(e => e.PlantillaId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.RegistradoPor)
                  .WithMany(u => u.DocumentosLlenados)
                  .HasForeignKey(e => e.RegistradoPorUsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ValorCampo>(entity =>
        {
            entity.ToTable("ValorCampo");
            entity.HasKey(e => e.ValorCampoId);
            entity.HasIndex(e => new { e.DocumentoLlenadoId, e.CampoPlantillaId }).IsUnique();
            entity.Property(e => e.FechaGuardado).HasColumnType("datetime2");

            entity.HasOne(e => e.DocumentoLlenado)
                  .WithMany(d => d.Valores)
                  .HasForeignKey(e => e.DocumentoLlenadoId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CampoPlantilla)
                  .WithMany(c => c.Valores)
                  .HasForeignKey(e => e.CampoPlantillaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ArchivoEvidencia>(entity =>
        {
            entity.ToTable("ArchivoEvidencia");
            entity.HasKey(e => e.ArchivoEvidenciaId);
            entity.HasIndex(e => e.DocumentoLlenadoId);
            entity.Property(e => e.FechaSubida).HasColumnType("datetime2");

            entity.HasOne(e => e.DocumentoLlenado)
                  .WithMany(d => d.Evidencias)
                  .HasForeignKey(e => e.DocumentoLlenadoId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SubidoPor)
                  .WithMany(u => u.EvidenciasSubidas)
                  .HasForeignKey(e => e.SubidoPorUsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
