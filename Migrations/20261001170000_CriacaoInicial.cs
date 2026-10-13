using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Biblioteca.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Autores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Autores", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Categorias", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "Editoras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Editoras", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "Tipos_Usuario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descricao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Tipos_Usuario", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "Livros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    EditoraId = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Isbn = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AnoPublicacao = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Livros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Livros_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Livros_Editoras_EditoraId",
                        column: x => x.EditoraId,
                        principalTable: "Editoras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoUsuarioId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Tipos_Usuario_TipoUsuarioId",
                        column: x => x.TipoUsuarioId,
                        principalTable: "Tipos_Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Exemplares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LivroId = table.Column<int>(type: "int", nullable: false),
                    NumeroExemplar = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Situacao = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exemplares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exemplares_Livros_LivroId",
                        column: x => x.LivroId,
                        principalTable: "Livros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Tabela associativa da relação N:N Livro <-> Autor
            migrationBuilder.CreateTable(
                name: "Livro_Autor",
                columns: table => new
                {
                    AutoresId = table.Column<int>(type: "int", nullable: false),
                    LivrosId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Livro_Autor", x => new { x.AutoresId, x.LivrosId });
                    table.ForeignKey(
                        name: "FK_Livro_Autor_Autores_AutoresId",
                        column: x => x.AutoresId,
                        principalTable: "Autores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Livro_Autor_Livros_LivrosId",
                        column: x => x.LivrosId,
                        principalTable: "Livros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Emprestimos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    ExemplarId = table.Column<int>(type: "int", nullable: false),
                    DataRetirada = table.Column<DateTime>(type: "date", nullable: false),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "date", nullable: false),
                    DataDevolucao = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emprestimos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Emprestimos_Exemplares_ExemplarId",
                        column: x => x.ExemplarId,
                        principalTable: "Exemplares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Emprestimos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Tipos_Usuario",
                columns: new[] { "Id", "Descricao" },
                values: new object[,]
                {
                    { 1, "Comum" },
                    { 2, "Administrador" }
                });

            migrationBuilder.CreateIndex(name: "IX_Emprestimos_UsuarioId", table: "Emprestimos", column: "UsuarioId");

            // REGRA DE NEGÓCIO: no máximo 1 empréstimo EM ABERTO (sem devolução) por exemplar
            migrationBuilder.CreateIndex(
                name: "UX_Emprestimo_Exemplar_EmAberto",
                table: "Emprestimos",
                column: "ExemplarId",
                unique: true,
                filter: "[DataDevolucao] IS NULL");

            migrationBuilder.CreateIndex(name: "IX_Exemplares_LivroId", table: "Exemplares", column: "LivroId");
            migrationBuilder.CreateIndex(name: "IX_Exemplares_NumeroExemplar", table: "Exemplares", column: "NumeroExemplar", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Livro_Autor_LivrosId", table: "Livro_Autor", column: "LivrosId");
            migrationBuilder.CreateIndex(name: "IX_Livros_CategoriaId", table: "Livros", column: "CategoriaId");
            migrationBuilder.CreateIndex(name: "IX_Livros_EditoraId", table: "Livros", column: "EditoraId");
            migrationBuilder.CreateIndex(name: "IX_Livros_Isbn", table: "Livros", column: "Isbn", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Usuarios_Email", table: "Usuarios", column: "Email", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Usuarios_TipoUsuarioId", table: "Usuarios", column: "TipoUsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Emprestimos");
            migrationBuilder.DropTable(name: "Livro_Autor");
            migrationBuilder.DropTable(name: "Exemplares");
            migrationBuilder.DropTable(name: "Usuarios");
            migrationBuilder.DropTable(name: "Autores");
            migrationBuilder.DropTable(name: "Livros");
            migrationBuilder.DropTable(name: "Tipos_Usuario");
            migrationBuilder.DropTable(name: "Categorias");
            migrationBuilder.DropTable(name: "Editoras");
        }
    }
}
