using BackendTorneosS.Contexto;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using DotNetEnv;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BackendTorneosS.Servicios;
using BackendTorneosS.Contexto;
using BackendTorneosS.Servicios;
using Microsoft.AspNetCore.Antiforgery;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Carga de variables del archivo .env

DotNetEnv.Env.Load();

var jwtKey = Environment.GetEnvironmentVariable("JWT__KEY");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT__ISSUER");
var jwtAudience = Environment.GetEnvironmentVariable("JWT__AUDIENCE");
var jwtMinutes = Environment.GetEnvironmentVariable("JWT__EXPIREMINUTES");
var DefaultConnection = Environment.GetEnvironmentVariable("DefaultConnection");
// Configuracion de JWT

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx => {
            if (string.IsNullOrEmpty(ctx.Token) &&
                ctx.Request.Cookies.TryGetValue("access_token", out var t))
                ctx.Token = t;
            return Task.CompletedTask;
        },
        OnTokenValidated = async ctx =>
        {
            var db = ctx.HttpContext.RequestServices.GetRequiredService<DBContext>();
            var jti = ctx.Principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            // ¿está revocado o expirado?
            var invalid = await db.UsuarioSesion.AnyAsync(s =>
                s.strJti == jti && (s.bitRevoked || s.datExpiracion <= DateTime.UtcNow));

            if (invalid)
                ctx.Fail("Token revocado o expirado en servidor");
        }
    };
});

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddHttpClient(); // necesario para IHttpClientFactory
builder.Services.AddScoped<OpenAIService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DBContext>(opts =>
    opts.UseSqlServer(DefaultConnection));

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", builder =>
    {
        builder.WithOrigins("http://localhost:5173", "http://mikhamxn.somee.com", "https://www.gridstudio.app", "https://gridstudio.app")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

builder.Services.AddAntiforgery(o => {
    o.Cookie.Name = "XSRF-TOKEN";
    o.HeaderName = "X-XSRF-TOKEN";
    o.Cookie.SameSite = SameSiteMode.None;
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MiAPI v1");

    });
}
app.UseHttpsRedirection();
app.UseHsts();


app.UseCors("PermitirFrontend");

// 1) Autenticación
app.UseAuthentication();

// 2) Autorización
app.UseAuthorization();


// Middleware para emitir cookie XSRF
app.Use(async (ctx, next) => {
    var af = ctx.RequestServices.GetRequiredService<IAntiforgery>();
    var tokens = af.GetAndStoreTokens(ctx);
    ctx.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!, new CookieOptions
    {
        HttpOnly = false,
        Secure = true,
        SameSite = SameSiteMode.None,
        Path = "/"
    });
    await next();
});

app.MapControllers();
app.Run();
