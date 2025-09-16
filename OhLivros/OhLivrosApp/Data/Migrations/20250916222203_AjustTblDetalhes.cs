using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OhLivrosApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AjustTblDetalhes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetalhesCarrinho_Carrinhos_CarrinhoFK",
                table: "DetalhesCarrinho");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalhesCarrinho_Livros_LivroFK",
                table: "DetalhesCarrinho");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalhesEncomenda_Encomendas_EncomendaFK",
                table: "DetalhesEncomenda");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalhesEncomenda_Livros_LivroFK",
                table: "DetalhesEncomenda");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetalhesEncomenda",
                table: "DetalhesEncomenda");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetalhesCarrinho",
                table: "DetalhesCarrinho");

            migrationBuilder.RenameTable(
                name: "DetalhesEncomenda",
                newName: "DetalhesEncomendas");

            migrationBuilder.RenameTable(
                name: "DetalhesCarrinho",
                newName: "DetalhesCarrinhos");

            migrationBuilder.RenameIndex(
                name: "IX_DetalhesEncomenda_LivroFK",
                table: "DetalhesEncomendas",
                newName: "IX_DetalhesEncomendas_LivroFK");

            migrationBuilder.RenameIndex(
                name: "IX_DetalhesEncomenda_EncomendaFK",
                table: "DetalhesEncomendas",
                newName: "IX_DetalhesEncomendas_EncomendaFK");

            migrationBuilder.RenameIndex(
                name: "IX_DetalhesCarrinho_LivroFK",
                table: "DetalhesCarrinhos",
                newName: "IX_DetalhesCarrinhos_LivroFK");

            migrationBuilder.RenameIndex(
                name: "IX_DetalhesCarrinho_CarrinhoFK",
                table: "DetalhesCarrinhos",
                newName: "IX_DetalhesCarrinhos_CarrinhoFK");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetalhesEncomendas",
                table: "DetalhesEncomendas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetalhesCarrinhos",
                table: "DetalhesCarrinhos",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DetalhesCarrinhos_Carrinhos_CarrinhoFK",
                table: "DetalhesCarrinhos",
                column: "CarrinhoFK",
                principalTable: "Carrinhos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalhesCarrinhos_Livros_LivroFK",
                table: "DetalhesCarrinhos",
                column: "LivroFK",
                principalTable: "Livros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalhesEncomendas_Encomendas_EncomendaFK",
                table: "DetalhesEncomendas",
                column: "EncomendaFK",
                principalTable: "Encomendas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalhesEncomendas_Livros_LivroFK",
                table: "DetalhesEncomendas",
                column: "LivroFK",
                principalTable: "Livros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetalhesCarrinhos_Carrinhos_CarrinhoFK",
                table: "DetalhesCarrinhos");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalhesCarrinhos_Livros_LivroFK",
                table: "DetalhesCarrinhos");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalhesEncomendas_Encomendas_EncomendaFK",
                table: "DetalhesEncomendas");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalhesEncomendas_Livros_LivroFK",
                table: "DetalhesEncomendas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetalhesEncomendas",
                table: "DetalhesEncomendas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetalhesCarrinhos",
                table: "DetalhesCarrinhos");

            migrationBuilder.RenameTable(
                name: "DetalhesEncomendas",
                newName: "DetalhesEncomenda");

            migrationBuilder.RenameTable(
                name: "DetalhesCarrinhos",
                newName: "DetalhesCarrinho");

            migrationBuilder.RenameIndex(
                name: "IX_DetalhesEncomendas_LivroFK",
                table: "DetalhesEncomenda",
                newName: "IX_DetalhesEncomenda_LivroFK");

            migrationBuilder.RenameIndex(
                name: "IX_DetalhesEncomendas_EncomendaFK",
                table: "DetalhesEncomenda",
                newName: "IX_DetalhesEncomenda_EncomendaFK");

            migrationBuilder.RenameIndex(
                name: "IX_DetalhesCarrinhos_LivroFK",
                table: "DetalhesCarrinho",
                newName: "IX_DetalhesCarrinho_LivroFK");

            migrationBuilder.RenameIndex(
                name: "IX_DetalhesCarrinhos_CarrinhoFK",
                table: "DetalhesCarrinho",
                newName: "IX_DetalhesCarrinho_CarrinhoFK");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetalhesEncomenda",
                table: "DetalhesEncomenda",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetalhesCarrinho",
                table: "DetalhesCarrinho",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DetalhesCarrinho_Carrinhos_CarrinhoFK",
                table: "DetalhesCarrinho",
                column: "CarrinhoFK",
                principalTable: "Carrinhos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalhesCarrinho_Livros_LivroFK",
                table: "DetalhesCarrinho",
                column: "LivroFK",
                principalTable: "Livros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalhesEncomenda_Encomendas_EncomendaFK",
                table: "DetalhesEncomenda",
                column: "EncomendaFK",
                principalTable: "Encomendas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalhesEncomenda_Livros_LivroFK",
                table: "DetalhesEncomenda",
                column: "LivroFK",
                principalTable: "Livros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
