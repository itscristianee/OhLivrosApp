using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OhLivrosApp;
using OhLivrosApp.Data;
using OhLivrosApp.Data.Seed;
using OhLivrosApp.Repositorios;
using OhLivrosApp.Servicos;
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
// Identity + Roles (Cookies por defeito)
// ============================
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true; // ajusta se quiseres false em dev
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// ============================
// MVC (Views + Razor) e API (Controllers)
// ============================
builder.Services.AddControllersWithViews();

builder.Services.AddControllers() // para a API
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// ============================
// DI dos repositórios/serviços
// ============================
builder.Services.AddTransient<IHomeRepositorio, HomeRepositorio>();
builder.Services.AddTransient<ICarrinhoRepositorio, CarrinhoRepositorio>();
builder.Services.AddTransient<IEncUtilizadorRepositorio, EncUtilizadorRepositorio>();
builder.Services.AddScoped<IStockRepositorio, StockRepositorio>();
builder.Services.AddScoped<ILivroRepositorio, LivroRepositorio>();
builder.Services.AddScoped<IGeneroRepositorio, GeneroRepositorio>();
builder.Services.AddTransient<IFicheiroServico, FicheiroServico>(); // <- resolve o serviço de ficheiros

// ============================
// Session + Cache
// ============================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ============================
// CORS (se expores a API a um SPA)
// ============================
builder.Services.AddCors(options =>
{
    options.AddPolicy("SpaWithCookies", policy =>
    {
        policy
            .WithOrigins("https://localhost:5173", "http://localhost:5173") // ajusta conforme o teu SPA
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ============================
// JWT (Bearer) – sem mexer nos cookies do Identity
// ============================
var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"]!);

builder.Services.AddAuthentication() // mantém o esquema de cookies do Identity como default
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddScoped<TokenService>();

// ============================
// Swagger (opcional mas útil para testar a API)
// ============================
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// ============================
// Pipeline HTTP
// ============================
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Seed DB/roles/users
app.UseItToSeedSqlServer();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("SpaWithCookies");

app.UseCookiePolicy();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// *** IMPORTANTE para a API ***
app.MapControllers(); // sem isto, /api/* devolve 404

// MVC (UI)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
