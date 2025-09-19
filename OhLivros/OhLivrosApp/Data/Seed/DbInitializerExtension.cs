// Data/Seed/DbInitializerExtension.cs
using Microsoft.Extensions.Logging;
using OhLivrosApp.Data.Seed;

namespace OhLivrosApp.Data.Seed // (usa o namespace do teu projeto)
{
    internal static class DbInitializerExtension
    {
        public static IApplicationBuilder UseItToSeedSqlServer(this IApplicationBuilder app)
        {
            ArgumentNullException.ThrowIfNull(app);

            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DbSeeder");

            try
            {
                // chama o seeder assíncrono de forma síncrona (extensões não podem ser async)
                DbInitializer.InitializeAsync(services).GetAwaiter().GetResult();
                logger.LogInformation("Seed da base de dados concluído com sucesso.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao executar o seed da base de dados.");
                throw; // opcional: relançar para falhar no arranque
            }

            return app;
        }
    }
}
