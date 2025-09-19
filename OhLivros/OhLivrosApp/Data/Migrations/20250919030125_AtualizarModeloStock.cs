using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OhLivrosApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AtualizarModeloStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stock_LivroFK",
                table: "Stock");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_LivroFK",
                table: "Stock",
                column: "LivroFK",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stock_LivroFK",
                table: "Stock");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_LivroFK",
                table: "Stock",
                column: "LivroFK");
        }
    }
}
