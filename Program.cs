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

var builder = WebApplication.CreateBuilder(args);

// Carga de variables del archivo .env

DotNetEnv.Env.Load();

var jwtKey = Environment.GetEnvironmentVariable("JWT__KEY");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT__ISSUER");
var jwtAudience = Environment.GetEnvironmentVariable("JWT__AUDIENCE");
var jwtMinutes = Environment.GetEnvironmentVariable("JWT__EXPIREMINUTES");

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
        OnMessageReceived = context =>
        {
            // lee la cookie llamada "access_token"
            if (context.Request.Cookies.TryGetValue("access_token", out var token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
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
    opts.UseSqlServer("workstation id=idgsbMikhamxn.mssql.somee.com;packet size=4096;user id=Mikhamxn_SQLLogin_1;pwd=q79ibl4iyr;data source=idgsbMikhamxn.mssql.somee.com;persist security info=False;initial catalog=idgsbMikhamxn;TrustServerCertificate=True\r\n"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", builder =>
    {
        builder.WithOrigins("http://localhost:5173", "http://mikhamxn.somee.com")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
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
//app.UseHttpsRedirection();

app.UseCors("PermitirFrontend");

// 1) Autenticación
app.UseAuthentication();

// 2) Autorización
app.UseAuthorization();

app.MapControllers();
app.Run();
