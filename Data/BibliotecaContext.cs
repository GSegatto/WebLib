using Biblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Data;

public class BibliotecaContext : DbContext
{
    public BibliotecaContext(DbContextOptions<BibliotecaContext> options) : base(options) { }

    public DbSet<TipoUsuario> TiposUsuario => Set<TipoUsuario>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Editora> Editoras => Set<Editora>();
    public DbSet<Autor> Autores => Set<Autor>();
    public DbSet<Livro> Livros => Set<Livro>();
    public DbSet<Exemplar> Exemplares => Set<Exemplar>();
    public DbSet<Emprestimo> Emprestimos => Set<Emprestimo>();
    public DbSet<Newsletter> Newsletters => Set<Newsletter>();
    public DbSet<Favorito> Favoritos => Set<Favorito>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<TipoUsuario>(e =>
        {
            e.ToTable("Tipos_Usuario");
            e.Property(x => x.Descricao).HasMaxLength(50).IsRequired();
            e.HasData(
                new TipoUsuario { Id = 1, Descricao = "Comum" },
                new TipoUsuario { Id = 2, Descricao = "Administrador" });
        });

        mb.Entity<Usuario>(e =>
        {
            e.ToTable("Usuarios");
            e.Property(x => x.Nome).HasMaxLength(150).IsRequired();
            e.Property(x => x.Email).HasMaxLength(150).IsRequired();
            e.Property(x => x.SenhaHash).HasMaxLength(255).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.HasOne(x => x.TipoUsuario).WithMany(t => t.Usuarios)
             .HasForeignKey(x => x.TipoUsuarioId).OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<Categoria>(e =>
        {
            e.ToTable("Categorias");
            e.Property(x => x.Nome).HasMaxLength(100).IsRequired();
        });

        mb.Entity<Editora>(e =>
        {
            e.ToTable("Editoras");
            e.Property(x => x.Nome).HasMaxLength(100).IsRequired();
        });

        mb.Entity<Autor>(e =>
        {
            e.ToTable("Autores");
            e.Property(x => x.Nome).HasMaxLength(100).IsRequired();
        });

        mb.Entity<Livro>(e =>
        {
            e.ToTable("Livros");
            e.Property(x => x.Titulo).HasMaxLength(150).IsRequired();
            e.Property(x => x.Isbn).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.Isbn).IsUnique();

            e.HasOne(x => x.Categoria).WithMany(c => c.Livros)
             .HasForeignKey(x => x.CategoriaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Editora).WithMany(c => c.Livros)
             .HasForeignKey(x => x.EditoraId).OnDelete(DeleteBehavior.Restrict);

            // Relação N:N Livro <-> Autor, materializada na tabela Livro_Autor (PK composta)
            e.HasMany(x => x.Autores).WithMany(a => a.Livros)
             .UsingEntity(j => j.ToTable("Livro_Autor"));
        });

        mb.Entity<Exemplar>(e =>
        {
            e.ToTable("Exemplares");
            e.Property(x => x.NumeroExemplar).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.NumeroExemplar).IsUnique();
            e.Property(x => x.Situacao).HasConversion<string>().HasMaxLength(20);
            e.HasOne(x => x.Livro).WithMany(l => l.Exemplares)
             .HasForeignKey(x => x.LivroId).OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<Emprestimo>(e =>
        {
            e.ToTable("Emprestimos");
            e.Property(x => x.DataRetirada).HasColumnType("date");
            e.Property(x => x.DataPrevistaDevolucao).HasColumnType("date");
            e.Property(x => x.DataDevolucao).HasColumnType("date");

            e.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(StatusEmprestimo.Pendente);

            e.HasOne(x => x.Usuario).WithMany(u => u.Emprestimos)
             .HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Exemplar).WithMany(x => x.Emprestimos)
             .HasForeignKey(x => x.ExemplarId).OnDelete(DeleteBehavior.Restrict);

            // Um exemplar só pode ter uma solicitação ativa,
            // uma reserva ou um empréstimo em andamento.
            e.HasIndex(x => x.ExemplarId)
                .IsUnique()
                .HasFilter("[Status] IN ('Pendente', 'AguardandoRetirada', 'EmAndamento')")
                .HasDatabaseName("UX_Emprestimo_Exemplar_EmAberto");
        });

        mb.Entity<Newsletter>(e =>
        {
            e.ToTable("Newsletters");

            e.Property(x => x.Email)
                .HasMaxLength(150)
                .IsRequired();

            e.HasIndex(x => x.Email)
                .IsUnique();

            e.Property(x => x.DataInscricao)
                .HasColumnType("datetime");
        });

        mb.Entity<Favorito>(e =>
        {
            e.ToTable("Favoritos");

            e.HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Livro)
                .WithMany()
                .HasForeignKey(x => x.LivroId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.UsuarioId, x.LivroId })
                .IsUnique();
        });
    }
}
