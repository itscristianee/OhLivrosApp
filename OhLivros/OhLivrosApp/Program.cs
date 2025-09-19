using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OhLivrosApp;
using OhLivrosApp.Data;
using OhLivrosApp.Data.Seed; // UseItToSeedSqlServer()
using OhLivrosApp.Repositorios;
using OhLivrosApp.Servicos;
using System.Reflection; // IncludeXmlComments
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ============================
// DB
// ============================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ============================
// Identity + Roles (cookies por defeito)
// ============================
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true; // em dev, podes pôr false
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// ============================
// MVC (Views + Razor) e API (Controllers) + JSON
// ============================
builder.Services.AddControllersWithViews()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddRazorPages(); // necessário se usas UI do Identity

// ============================
// DI dos repositórios/serviços
// ============================
builder.Services.AddTransient<IHomeRepositorio, HomeRepositorio>();
builder.Services.AddTransient<ICarrinhoRepositorio, CarrinhoRepositorio>();
builder.Services.AddTransient<IEncUtilizadorRepositorio, EncUtilizadorRepositorio>();
builder.Services.AddScoped<IStockRepositorio, StockRepositorio>();
builder.Services.AddScoped<ILivroRepositorio, LivroRepositorio>();
builder.Services.AddScoped<IGeneroRepositorio, GeneroRepositorio>();
builder.Services.AddTransient<IFicheiroServico, FicheiroServico>();
builder.Services.AddScoped<TokenService>();

// ============================
// Cache + Session (uma vez só)
// ============================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ============================
// CORS (SPA local e Render)
// ============================
builder.Services.AddCors(options =>
{
    options.AddPolicy("SpaWithCookies", policy =>
    {
        policy.WithOrigins(
                "https://localhost:5173",
                "http://localhost:5173",
                "https://ohlivrosapp.onrender.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// (Render) confiar nos cabeçalhos do proxy
builder.Services.Configure<ForwardedHeadersOptions>(opts =>
{
    opts.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor;
});

// ============================
// JWT (Bearer) — coexiste com cookie do Identity
// ============================
var jwtSection = builder.Configuration.GetSection("Jwt");
var keyString = jwtSection.GetValue<string>("Key")
               ?? throw new InvalidOperationException("Jwt:Key não definido.");
var keyBytes = Encoding.UTF8.GetBytes(keyString);

builder.Services.AddAuthentication() // cookie do Identity continua como default
    .AddJwtBearer("Bearer", options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ============================
// Swagger (com XML docs)
// ============================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OhLivros API",
        Version = "v1",
        Description = "API para gestão de géneros, livros e utilizadores"
    });

    // XML de comentários – só inclui se existir
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});
// SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// ============================
// Pipeline HTTP
// ============================
app.UseForwardedHeaders(); // antes de auth/redirecionamentos

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}

// Swagger (ativo também em produção para o Publish do VS)
app.UseSwagger();
app.UseSwaggerUI(opt =>
{
    opt.SwaggerEndpoint("/swagger/v1/swagger.json", "OhLivros API v1");
    opt.RoutePrefix = "swagger"; // /swagger
});

// app.UseHttpsRedirection(); // se no Render der loop, deixa comentado
app.UseStaticFiles();

app.UseRouting();

app.UseCors("SpaWithCookies");

app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// ============================
// DB: aplicar migrações + seed (UMA vez, controlado por flag)
// ============================
var skipMigrations = app.Configuration.GetValue<bool>("SkipMigrationsOnStartup")
                     || Environment.GetEnvironmentVariable("SKIP_MIGRATIONS") == "1";

if (!skipMigrations)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    // Seed (a tua extensão)
    app.UseItToSeedSqlServer();
}

// ============================
// Endpoints
// ============================
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Hubs (SignalR) — ajusta o namespace se o teu LojaHub estiver noutro
app.MapHub<LojaHub>("/hubs/loja");

app.Run();
