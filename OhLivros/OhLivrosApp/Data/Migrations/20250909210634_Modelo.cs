using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OhLivrosApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class Modelo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Generos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Generos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Utilizadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Morada = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CodPostal = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Pais = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NIF = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Telemovel = table.Column<string>(type: "nvarchar(18)", maxLength: 18, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilizadores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Livros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Imagem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Autor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Preco = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    GeneroFK = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Livros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Livros_Generos_GeneroFK",
                        column: x => x.GeneroFK,
                        principalTable: "Generos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Carrinhos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Eliminado = table.Column<bool>(type: "bit", nullable: false),
                    DonoFK = table.Column<int>(type: "int", nullable: false),
                    DonoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carrinhos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carrinhos_Utilizadores_DonoId",
                        column: x => x.DonoId,
                        principalTable: "Utilizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Encomendas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Eliminado = table.Column<bool>(type: "bit", nullable: false),
                    MetodoPagamento = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Pago = table.Column<bool>(type: "bit", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    CompradorFK = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Encomendas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Encomendas_Utilizadores_CompradorFK",
                        column: x => x.CompradorFK,
                        principalTable: "Utilizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetalhesCarrinho",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    PrecoUnitario = table.Column<double>(type: "float", nullable: false),
                    LivroFK = table.Column<int>(type: "int", nullable: false),
                    CarrinhoFK = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalhesCarrinho", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetalhesCarrinho_Carrinhos_CarrinhoFK",
                        column: x => x.CarrinhoFK,
                        principalTable: "Carrinhos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetalhesCarrinho_Livros_LivroFK",
                        column: x => x.LivroFK,
                        principalTable: "Livros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetalhesEncomenda",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    EncomendaFK = table.Column<int>(type: "int", nullable: false),
                    LivroFK = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalhesEncomenda", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetalhesEncomenda_Encomendas_EncomendaFK",
                        column: x => x.EncomendaFK,
                        principalTable: "Encomendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetalhesEncomenda_Livros_LivroFK",
                        column: x => x.LivroFK,
                        principalTable: "Livros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Carrinhos_DonoId",
                table: "Carrinhos",
                column: "DonoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalhesCarrinho_CarrinhoFK",
                table: "DetalhesCarrinho",
                column: "CarrinhoFK");

            migrationBuilder.CreateIndex(
                name: "IX_DetalhesCarrinho_LivroFK",
                table: "DetalhesCarrinho",
                column: "LivroFK");

            migrationBuilder.CreateIndex(
                name: "IX_DetalhesEncomenda_EncomendaFK",
                table: "DetalhesEncomenda",
                column: "EncomendaFK");

            migrationBuilder.CreateIndex(
                name: "IX_DetalhesEncomenda_LivroFK",
                table: "DetalhesEncomenda",
                column: "LivroFK");

            migrationBuilder.CreateIndex(
                name: "IX_Encomendas_CompradorFK",
                table: "Encomendas",
                column: "CompradorFK");

            migrationBuilder.CreateIndex(
                name: "IX_Livros_GeneroFK",
                table: "Livros",
                column: "GeneroFK");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetalhesCarrinho");

            migrationBuilder.DropTable(
                name: "DetalhesEncomenda");

            migrationBuilder.DropTable(
                name: "Carrinhos");

            migrationBuilder.DropTable(
                name: "Encomendas");

            migrationBuilder.DropTable(
                name: "Livros");

            migrationBuilder.DropTable(
                name: "Utilizadores");

            migrationBuilder.DropTable(
                name: "Generos");
        }
    }
}
