using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OhLivrosApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTblUtili : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carrinhos_Utilizadores_DonoId",
                table: "Carrinhos");

            migrationBuilder.DropIndex(
                name: "IX_Carrinhos_DonoId",
                table: "Carrinhos");

            migrationBuilder.DropColumn(
                name: "DonoId",
                table: "Carrinhos");

            migrationBuilder.CreateIndex(
                name: "IX_Carrinhos_DonoFK",
                table: "Carrinhos",
                column: "DonoFK");

            migrationBuilder.AddForeignKey(
                name: "FK_Carrinhos_Utilizadores_DonoFK",
                table: "Carrinhos",
                column: "DonoFK",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carrinhos_Utilizadores_DonoFK",
                table: "Carrinhos");

            migrationBuilder.DropIndex(
                name: "IX_Carrinhos_DonoFK",
                table: "Carrinhos");

            migrationBuilder.AddColumn<int>(
                name: "DonoId",
                table: "Carrinhos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Carrinhos_DonoId",
                table: "Carrinhos",
                column: "DonoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carrinhos_Utilizadores_DonoId",
                table: "Carrinhos",
                column: "DonoId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
