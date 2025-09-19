using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OhLivrosApp;
using OhLivrosApp.Data;
using OhLivrosApp.Data.Seed;
using OhLivrosApp.Repositorios;
using OhLivrosApp.Servicos;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ============================
// DB (com retries p/ Azure SQL)
// ============================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sql =>
    {
        sql.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
    }));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ============================
// Identity + Roles
// ============================
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true; // em dev pode ser false
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// ============================
// MVC + JSON
// ============================
builder.Services.AddControllersWithViews()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddRazorPages();

// ============================
// DI
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
// Cache + Session
// ============================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ============================
// CORS
// ============================
// Troque as URLs abaixo pelas REAIS do seu front
var allowedOrigins = new[]
{
    "http://localhost:5173",
    "https://localhost:5173",
    "https://ohlivrosapp.onrender.com",        // Front no Render (exemplo)
    "https://SEU-FRONT.azurerestaticapps.net"  // Front no Azure Static Apps (exemplo)
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("SpaWithCookies", p =>
        p.WithOrigins(allowedOrigins)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials());

    options.AddPolicy("ProdCors", p =>
        p.WithOrigins(allowedOrigins)
         .AllowAnyHeader()
         .AllowAnyMethod());
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

builder.Services.AddAuthentication() // cookie do Identity fica default
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
// Swagger
// ============================
builder.Services.AddEndpointsApiExplorer(); // ajuda o publish do VS
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "OhLivrosApp API", Version = "v1" });

    // XML comments (se o projeto gerar o .xml em Build)
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
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}

// Swagger (permitir em prod se Swagger:Enabled = true)
var enableSwagger = builder.Configuration.GetValue<bool>("Swagger:Enabled");
if (app.Environment.IsDevelopment() || enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OhLivros API v1"));
}

// app.UseHttpsRedirection(); // habilite se o host lidar bem com HTTPS
app.UseStaticFiles();

app.UseRouting();

app.UseCors("SpaWithCookies");

app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// ============================
// DB: migrar + seed (controlado por flag)
// ============================
var skipMigrations = app.Configuration.GetValue<bool>("SkipMigrationsOnStartup")
                     || Environment.GetEnvironmentVariable("SKIP_MIGRATIONS") == "1";

if (!skipMigrations)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

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

// Ajuste o namespace do seu Hub, se for diferente
app.MapHub<LojaHub>("/hubs/loja");

app.Run();
