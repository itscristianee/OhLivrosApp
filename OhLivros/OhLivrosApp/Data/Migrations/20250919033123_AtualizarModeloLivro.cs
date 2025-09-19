using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OhLivrosApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AtualizarModeloLivro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quantidade",
                table: "Livros",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantidade",
                table: "Livros");
        }
    }
}
