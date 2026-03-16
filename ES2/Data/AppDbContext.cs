using System;
using System.Collections.Generic;
using ES2.Models;
using Microsoft.EntityFrameworkCore;

namespace ES2.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Atividade> Atividades { get; set; }

    public virtual DbSet<Bilhete> Bilhetes { get; set; }

    public virtual DbSet<BilheteUtil> BilheteUtils { get; set; }

    public virtual DbSet<BilhetesEvento> BilhetesEventos { get; set; }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<CategoriaEvento> CategoriaEventos { get; set; }

    public virtual DbSet<CodigoPostal> CodigoPostals { get; set; }

    public virtual DbSet<Evento> Eventos { get; set; }

    public virtual DbSet<FeedbackAtv> FeedbackAtvs { get; set; }

    public virtual DbSet<FeedbackEvnt> FeedbackEvnts { get; set; }

    public virtual DbSet<Mensagem> Mensagens { get; set; }

    public virtual DbSet<Recibo> Recibos { get; set; }

    public virtual DbSet<RegistoAtividade> RegistoAtividades { get; set; }

    public virtual DbSet<RegistoEvento> RegistoEventos { get; set; }

    public virtual DbSet<TipoBilhete> TipoBilhetes { get; set; }

    public virtual DbSet<TipoPagamento> TipoPagamentos { get; set; }

    public virtual DbSet<TipoUtilizador> TipoUtilizadors { get; set; }

    public virtual DbSet<Utilizador> Utilizadores { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=ES2;Username=postgres;Password=123");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Atividade>(entity =>
        {
            entity.HasKey(e => e.IdAtividade).HasName("Atividades_pkey");

            entity.ToTable("Atividades", "ES2");

            entity.Property(e => e.IdAtividade)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID_Atividade");
            entity.Property(e => e.IdCategoria).HasColumnName("ID_Categoria");
            entity.Property(e => e.IdEvento).HasColumnName("ID_Evento");
            entity.Property(e => e.Local).HasMaxLength(100);
            entity.Property(e => e.Nome).HasMaxLength(100);

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Atividades)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Categoria");

            entity.HasOne(d => d.IdEventoNavigation).WithMany(p => p.Atividades)
                .HasForeignKey(d => d.IdEvento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Evento");
        });

        modelBuilder.Entity<Bilhete>(entity =>
        {
            entity.HasKey(e => e.IdBilhete).HasName("Bilhete_pkey");

            entity.ToTable("Bilhete", "ES2");

            entity.Property(e => e.IdBilhete)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID_Bilhete");
            entity.Property(e => e.IdTipo).HasColumnName("ID_Tipo");
            entity.Property(e => e.Nome).HasMaxLength(40);

            entity.HasOne(d => d.IdTipoNavigation).WithMany(p => p.Bilhetes)
                .HasForeignKey(d => d.IdTipo)
                .HasConstraintName("Tipo_Bilhete");
        });

        modelBuilder.Entity<BilheteUtil>(entity =>
        {
            entity.HasKey(e => e.IdBiUti).HasName("BilheteUtil_pkey");

            entity.ToTable("BilheteUtil", "ES2");

            entity.Property(e => e.IdBiUti)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID_BiUti");
            entity.Property(e => e.IdBiEv).HasColumnName("ID_BiEv");
            entity.Property(e => e.IdUtilizador).HasColumnName("ID_Utilizador");

            entity.HasOne(d => d.IdBiEvNavigation).WithMany(p => p.BilheteUtils)
                .HasForeignKey(d => d.IdBiEv)
                .HasConstraintName("Bilhete_Evento");

            entity.HasOne(d => d.IdUtilizadorNavigation).WithMany(p => p.BilheteUtils)
                .HasForeignKey(d => d.IdUtilizador)
                .HasConstraintName("Utilizador");
        });

        modelBuilder.Entity<BilhetesEvento>(entity =>
        {
            entity.HasKey(e => e.IdBiEv).HasName("Bilhetes_Eventos_pkey");

            entity.ToTable("Bilhetes_Eventos", "ES2");

            entity.Property(e => e.IdBiEv)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID_BiEv");
            entity.Property(e => e.IdBilhete).HasColumnName("ID_Bilhete");
            entity.Property(e => e.IdEvento).HasColumnName("ID_Evento");

            entity.HasOne(d => d.IdBilheteNavigation).WithMany(p => p.BilhetesEventos)
                .HasForeignKey(d => d.IdBilhete)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Bilhete");

            entity.HasOne(d => d.IdEventoNavigation).WithMany(p => p.BilhetesEventos)
                .HasForeignKey(d => d.IdEvento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Evento");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("Categorias_pkey");

            entity.ToTable("Categorias", "ES2");

            entity.Property(e => e.IdCategoria)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID_Categoria");
            entity.Property(e => e.Nome).HasMaxLength(40);
        });

        modelBuilder.Entity<CategoriaEvento>(entity =>
        {
            entity.HasKey(e => e.IdCatEve).HasName("Categoria_Eventos_pkey");

            entity.ToTable("Categoria_Eventos", "ES2");

            entity.Property(e => e.IdCatEve)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID_CatEve");
            entity.Property(e => e.IdCategoria).HasColumnName("ID_Categoria");
            entity.Property(e => e.IdEvento).HasColumnName("ID_Evento");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.CategoriaEventos)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Categoria");

            entity.HasOne(d => d.IdEventoNavigation).WithMany(p => p.CategoriaEventos)
                .HasForeignKey(d => d.IdEvento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Evento");
        });

        modelBuilder.Entity<CodigoPostal>(entity =>
        {
            entity.HasKey(e => e.IdCodPostal).HasName("CodigoPostal_pkey");

            entity.ToTable("CodigoPostal", "ES2");

            entity.Property(e => e.IdCodPostal)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID_CodPostal");
            entity.Property(e => e.CodPostal).HasMaxLength(10);
        });

        modelBuilder.Entity<Evento>(entity =>
        {
            entity.HasKey(e => e.IdEvento).HasName("Evento_pkey");

            entity.ToTable("Evento", "ES2");

            entity.Property(e => e.IdEvento)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID_Evento");
            entity.Property(e => e.CapMax).HasColumnName("Cap_Max");
            entity.Property(e => e.HoraFim).HasColumnName("hora_fim");
            entity.Property(e => e.HoraInicio).HasColumnName("hora_inicio");
            entity.Property(e => e.IdCategoria).HasColumnName("ID_Categoria");
            entity.Property(e => e.Local).HasMaxLength(50);
            entity.Property(e => e.Nome).HasMaxLength(100);

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Eventos)
                .HasForeignKey(d => d.IdCategoria)
                .HasConstraintName("Categoria");
        });

        modelBuilder.Entity<FeedbackAtv>(entity =>
        {
            entity.HasKey(e => e.IdFbati).HasName("FeedbackAtv_pkey");

            entity.ToTable("FeedbackAtv", "ES2");

            entity.Property(e => e.IdFbati)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID_FBAti");
            entity.Property(e => e.IdAtividade).HasColumnName("ID_Atividade");
            entity.Property(e => e.IdUti).HasColumnName("ID_Uti");

            entity.HasOne(d => d.IdAtividadeNavigation).WithMany(p => p.FeedbackAtvs)
                .HasForeignKey(d => d.IdAtividade)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Atividade");

            entity.HasOne(d => d.IdUtiNavigation).WithMany(p => p.FeedbackAtvs)
                .HasForeignKey(d => d.IdUti)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Utilizador");
        });

        modelBuilder.Entity<FeedbackEvnt>(entity =>
        {
            entity.HasKey(e => e.IdFbevnt).HasName("FeedbackEvnt_pkey");

            entity.ToTable("FeedbackEvnt", "ES2");

            entity.Property(e => e.IdFbevnt)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID_FBEvnt");
            entity.Property(e => e.IdEvento).HasColumnName("ID_Evento");
            entity.Property(e => e.IdUti).HasColumnName("ID_Uti");

            entity.HasOne(d => d.IdEventoNavigation).WithMany(p => p.FeedbackEvnts)
                .HasForeignKey(d => d.IdEvento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Evento");

            entity.HasOne(d => d.IdUtiNavigation).WithMany(p => p.FeedbackEvnts)
                .HasForeignKey(d => d.IdUti)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Utilizador");
        });

        modelBuilder.Entity<Mensagem>(entity =>
        {
            entity.HasKey(e => e.IdEnvio).HasName("Mensagens_pkey");

            entity.ToTable("Mensagens", "ES2");

            entity.Property(e => e.IdEnvio)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID_Envio");
            entity.Property(e => e.IdEnviador).HasColumnName("ID_Enviador");
            entity.Property(e => e.IdRecetor).HasColumnName("ID_Recetor");

            entity.HasOne(d => d.IdEnviadorNavigation).WithMany(p => p.MensagenIdEnviadorNavigations)
                .HasForeignKey(d => d.IdEnviador)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Utilizador_Env");

            entity.HasOne(d => d.IdRecetorNavigation).WithMany(p => p.MensagenIdRecetorNavigations)
                .HasForeignKey(d => d.IdRecetor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Utilizador_Rec");
        });

        modelBuilder.Entity<Recibo>(entity =>
        {
            entity.HasKey(e => e.IdRecibo).HasName("Recibo_pkey");

            entity.ToTable("Recibo", "ES2");

            entity.Property(e => e.IdRecibo)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID_Recibo");
            entity.Property(e => e.IdBiUti).HasColumnName("ID_BiUti");
            entity.Property(e => e.IdTipoPag).HasColumnName("ID_TipoPag");
            entity.Property(e => e.IdUtilizador).HasColumnName("ID_Utilizador");

            entity.HasOne(d => d.IdBiUtiNavigation).WithMany(p => p.Recibos)
                .HasForeignKey(d => d.IdBiUti)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Bilhetes_Util");

            entity.HasOne(d => d.IdTipoPagNavigation).WithMany(p => p.Recibos)
                .HasForeignKey(d => d.IdTipoPag)
                .HasConstraintName("TipoPagamento");

            entity.HasOne(d => d.IdUtilizadorNavigation).WithMany(p => p.Recibos)
                .HasForeignKey(d => d.IdUtilizador)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Utilizador");
        });

        modelBuilder.Entity<RegistoAtividade>(entity =>
        {
            entity.HasKey(e => e.IdRegAt).HasName("Registo_Atividades_pkey");

            entity.ToTable("Registo_Atividades", "ES2");

            entity.Property(e => e.IdRegAt)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID_RegAt");
            entity.Property(e => e.IdAtividade).HasColumnName("ID_Atividade");
            entity.Property(e => e.IdUti).HasColumnName("ID_Uti");
            entity.Property(e => e.IsCancelado).HasColumnName("isCancelado");

            entity.HasOne(d => d.IdAtividadeNavigation).WithMany(p => p.RegistoAtividades)
                .HasForeignKey(d => d.IdAtividade)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Atividade");

            entity.HasOne(d => d.IdUtiNavigation).WithMany(p => p.RegistoAtividades)
                .HasForeignKey(d => d.IdUti)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Utilizador");
        });

        modelBuilder.Entity<RegistoEvento>(entity =>
        {
            entity.HasKey(e => e.IdRegEv).HasName("Registo_eventos_pkey");

            entity.ToTable("Registo_eventos", "ES2");

            entity.Property(e => e.IdRegEv)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID_RegEv");
            entity.Property(e => e.IdEvento).HasColumnName("ID_Evento");
            entity.Property(e => e.IdUti).HasColumnName("ID_Uti");
            entity.Property(e => e.IsCancelado).HasColumnName("isCancelado");

            entity.HasOne(d => d.IdEventoNavigation).WithMany(p => p.RegistoEventos)
                .HasForeignKey(d => d.IdEvento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Eventos");

            entity.HasOne(d => d.IdUtiNavigation).WithMany(p => p.RegistoEventos)
                .HasForeignKey(d => d.IdUti)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Utilizador");
        });

        modelBuilder.Entity<TipoBilhete>(entity =>
        {
            entity.HasKey(e => e.IdTipo).HasName("Tipo_Bilhete_pkey");

            entity.ToTable("Tipo_Bilhete", "ES2");

            entity.Property(e => e.IdTipo)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID_Tipo");
            entity.Property(e => e.Nome).HasMaxLength(40);
        });

        modelBuilder.Entity<TipoPagamento>(entity =>
        {
            entity.HasKey(e => e.IdTipo).HasName("Tipo_Pagamento_pkey");

            entity.ToTable("Tipo_Pagamento", "ES2");

            entity.Property(e => e.IdTipo)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID_Tipo");
            entity.Property(e => e.Nome).HasColumnType("character varying");
        });

        modelBuilder.Entity<TipoUtilizador>(entity =>
        {
            entity.HasKey(e => e.IdTpUti).HasName("Tipo_Utilizador_pkey");

            entity.ToTable("Tipo_Utilizador", "ES2");

            entity.Property(e => e.IdTpUti)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID_tpUti");
            entity.Property(e => e.Nome).HasMaxLength(40);
        });

        modelBuilder.Entity<Utilizador>(entity =>
        {
            entity.HasKey(e => e.IdUti).HasName("Utilizadores_pkey");

            entity.ToTable("Utilizadores", "ES2");

            entity.Property(e => e.IdUti)
                .UseIdentityAlwaysColumn()
                .HasColumnName("ID_Uti");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.IdCodPostal).HasColumnName("ID_CodPostal");
            entity.Property(e => e.Morada).HasMaxLength(150);
            entity.Property(e => e.Nome).HasMaxLength(40);
            entity.Property(e => e.Password).HasMaxLength(40);
            entity.Property(e => e.Telemovel).HasMaxLength(9);
            entity.Property(e => e.TipoUti).HasColumnName("Tipo_Uti");

            entity.HasOne(d => d.IdCodPostalNavigation).WithMany(p => p.Utilizadores)
                .HasForeignKey(d => d.IdCodPostal)
                .HasConstraintName("CodPostal");

            entity.HasOne(d => d.TipoUtiNavigation).WithMany(p => p.Utilizadores)
                .HasForeignKey(d => d.TipoUti)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Tipo_Util");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
