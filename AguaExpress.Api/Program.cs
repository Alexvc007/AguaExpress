using System.Text;
using System.Text.Json.Serialization;
using AguaExpress.Api.Datos;
using AguaExpress.Api.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AguaExpressDb>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Neon")).UseSnakeCaseNamingConvention());

builder.Services.AddSingleton<ServicioToken>();
builder.Services.AddSingleton<ServicioNotificaciones>();

builder.Services.AddControllers().AddJsonOptions(o =>
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.MapInboundClaims = false;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        NameClaimType = "sub",
        RoleClaimType = "role",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Clave"]!))
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();

using (var alcance = app.Services.CreateScope())
{
    var db = alcance.ServiceProvider.GetRequiredService<AguaExpressDb>();
    await db.Database.EnsureCreatedAsync();
    await Semilla.EjecutarAsync(db, app.Configuration);
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => "AguaExpress API activa");
app.Run();
