using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace OhLivrosApp.Data
{
    // Garante que o EF consegue criar o DbContext em design-time (migrations/update-database)
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // 1) ponto de partida: diretório atual do design-time
            var basePath = Directory.GetCurrentDirectory();

            // 2) tentar localizar a pasta do projeto que contém o appsettings.json
            // Estrutura que tens: <repo>/OhLivros/OhLivrosApp/appsettings.json
            string?[] candidatos =
            {
                Path.Combine(basePath, "appsettings.json"),
                Path.Combine(basePath, "OhLivros", "OhLivrosApp", "appsettings.json"),
                Path.Combine(basePath, "..", "OhLivros", "OhLivrosApp", "appsettings.json"),
                Path.Combine(basePath, "..", "..", "OhLivros", "OhLivrosApp", "appsettings.json")
            };

            string? appsettingsPath = null;
            foreach (var c in candidatos)
            {
                if (File.Exists(c!))
                {
                    appsettingsPath = c;
                    break;
                }
            }

            if (appsettingsPath == null)
                throw new InvalidOperationException("Não encontrei appsettings.json (tenta colocar a ConnectionString em variável de ambiente DefaultConnection).");

            var projectDir = Path.GetDirectoryName(appsettingsPath)!;

            var config = new ConfigurationBuilder()
                .SetBasePath(projectDir)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            // 3) obter connection string do appsettings ou de env var
            var conn = config.GetConnectionString("DefaultConnection")
                      ?? Environment.GetEnvironmentVariable("DefaultConnection");

            if (string.IsNullOrWhiteSpace(conn))
                throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada no appsettings nem em variável de ambiente.");

            var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(conn)
                .Options;

            return new ApplicationDbContext(opts);
        }
    }
}
