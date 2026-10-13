using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biblioteca.Migrations
{
    /// <inheritdoc />
    public partial class AjustarFluxoSolicitacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Emprestimo_Exemplar_EmAberto",
                table: "Emprestimos");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Emprestimos",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Pendente",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldDefaultValue: "EmAndamento");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataRetirada",
                table: "Emprestimos",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataPrevistaDevolucao",
                table: "Emprestimos",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.CreateIndex(
                name: "UX_Emprestimo_Exemplar_EmAberto",
                table: "Emprestimos",
                column: "ExemplarId",
                unique: true,
                filter: "[Status] IN ('Pendente', 'AguardandoRetirada', 'EmAndamento')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Emprestimo_Exemplar_EmAberto",
                table: "Emprestimos");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Emprestimos",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "EmAndamento",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldDefaultValue: "Pendente");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataRetirada",
                table: "Emprestimos",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataPrevistaDevolucao",
                table: "Emprestimos",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_Emprestimo_Exemplar_EmAberto",
                table: "Emprestimos",
                column: "ExemplarId",
                unique: true,
                filter: "[DataDevolucao] IS NULL");
        }
    }
}
