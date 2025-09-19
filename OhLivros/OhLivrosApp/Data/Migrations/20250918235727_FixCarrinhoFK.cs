using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OhLivrosApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixCarrinhoFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Coluna temporária INT
            migrationBuilder.AddColumn<int>(
                name: "MetodoPagamentoTmp",
                table: "Encomendas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // 2) Migrar valores string -> int (ajuste o mapping ao seu enum)
            migrationBuilder.Sql(@"
UPDATE Encomendas
SET MetodoPagamentoTmp =
    CASE UPPER(LTRIM(RTRIM(MetodoPagamento)))
        WHEN 'MBWAY'           THEN 1
        WHEN 'MULTIBANCO'      THEN 2
        WHEN 'CARTAOCREDITO'   THEN 3
        WHEN 'TRANSFERENCIA'   THEN 4
        ELSE TRY_CAST(MetodoPagamento AS int)
    END
");

            // 3) Remover antiga e renomear a nova
            migrationBuilder.DropColumn(
                name: "MetodoPagamento",
                table: "Encomendas");

            migrationBuilder.RenameColumn(
                name: "MetodoPagamentoTmp",
                table: "Encomendas",
                newName: "MetodoPagamento");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MetodoPagamento",
                table: "Encomendas",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "MBWay");

            migrationBuilder.Sql(@"
UPDATE Encomendas SET MetodoPagamento =
    CASE MetodoPagamento
        WHEN 1 THEN 'MBWay'
        WHEN 2 THEN 'Multibanco'
        WHEN 3 THEN 'CartaoCredito'
        WHEN 4 THEN 'Transferencia'
        ELSE 'MBWay'
    END
");

            migrationBuilder.DropColumn(
                name: "MetodoPagamento",
                table: "Encomendas");

            migrationBuilder.RenameColumn(
                name: "MetodoPagamentoTmp",
                table: "Encomendas",
                newName: "MetodoPagamento");
        }
    }
}
