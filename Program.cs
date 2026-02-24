using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using LibraryAPI.Data;
using LibraryAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Database (EntityFramework + SQLite) ---
builder.Services.AddDbContext<LibraryDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// --- JWT Authentication ---
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<JwtService>();

// --- Controllers ---
builder.Services.AddControllers();

// --- Swagger / OpenAPI ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v3", new OpenApiInfo
    {
        Title = "📚 API Bibliotēka",
        Version = "v3",
        Description = "REST API bibliotēka grāmatu (Books), autoru (Authors), kategoriju (Categories) un aizdevumu (Loans) pārvaldībai.<br /><br />" +
                      "Padomi aplikācijas lietošanai:<br />"+
                      "  • Izveidojiet jaunu lietotāju --> POST/api/auth/register.<br />"+
                      "  • Ienāciet sistēmā, izmantojot tikko izveidotā lietotāja datus (vai admina datus, kas ir priekšiestatīti) --> POST/api/auth/login.<br />"+
                      "  • Nokopējiet JWT Bearer tokenu, ko atgriež šī metode, autorizējieties ar to --> zaļā poga 'Authorize'<br />"+
                      "  • Baudiet bibliotēkas funkcionalitāti! Mēs iepriekš iestatījām datu bāzēs divas grāmatas dažādās kategorijās.<br />"+
                      "  • Precīzāk programmas darbība ir aprakstīta prezentācijā.",
        Contact = new OpenApiContact { Name = "E-bibliotēkas atbalsta komanda - uzklikšķiniet virsū, lai sazinātos ar mums! (šīs ē-pasts mums nepieder un varbūt pat neeksistē, tas ir pievienots Swagger iespēju lietošanas demonstrācijai).", Email = "info@library.lv" }
    });

    // JWT Auth button in Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ievadiet JWT tokenu:"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments for endpoint summaries
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// --- Auto-migrate & seed DB on startup ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    db.Database.EnsureCreated();
}

// --- Middleware Pipeline ---
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v3/swagger.json", "Bibliotēka API v3");
    c.RoutePrefix = string.Empty; // Swagger pieejams uz /
    c.DocumentTitle = "📚 API Bbliotēka";
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
