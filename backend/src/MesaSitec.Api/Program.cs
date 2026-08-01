using System.IdentityModel.Tokens.Jwt;
using System.Text;
using MesaSitec.Infraestructura;
using MesaSitec.Infraestructura.Persistencia;
using MesaSitec.Infraestructura.Persistencia.Semilla;
using MesaSitec.Infraestructura.Seguridad;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(opciones =>
{
    opciones.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "Introduce el token JWT obtenido en /api/v1/auth/login."
        });

    opciones.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
});

string cadenaConexion =
    builder.Configuration.GetConnectionString(
        "DefaultConnection")
    ?? throw new InvalidOperationException(
        "No se encontró la cadena de conexión DefaultConnection.");

string jwtSecret =
    builder.Configuration["JWT_SECRET"]
    ?? throw new InvalidOperationException(
        "La variable de entorno JWT_SECRET es obligatoria.");

if (Encoding.UTF8.GetByteCount(jwtSecret) < 32)
{
    throw new InvalidOperationException(
        "JWT_SECRET debe contener al menos 32 caracteres.");
}

builder.Services.AgregarInfraestructura(
    cadenaConexion,
    jwtSecret);

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.MapInboundClaims = false;

        opciones.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = JwtConfiguracion.Emisor,

                ValidateAudience = true,
                ValidAudience = JwtConfiguracion.Audiencia,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSecret)),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,

                NameClaimType =
                    JwtRegisteredClaimNames.Sub,

                RoleClaimType = "rol"
            };

        opciones.Events = new JwtBearerEvents
        {
            OnChallenge = async contexto =>
            {
                contexto.HandleResponse();

                await Results.Problem(
                    statusCode:
                        StatusCodes.Status401Unauthorized,

                    type:
                        "https://mesasitec.local/errores/no-autenticado",

                    title:
                        "No autenticado",

                    detail:
                        "El token está ausente, es inválido o ha expirado.",

                    extensions:
                        new Dictionary<string, object?>
                        {
                            ["codigo"] = "NO_AUTENTICADO"
                        })
                    .ExecuteAsync(contexto.HttpContext);
            },

            OnForbidden = async contexto =>
            {
                await Results.Problem(
                    statusCode:
                        StatusCodes.Status403Forbidden,

                    type:
                        "https://mesasitec.local/errores/operacion-no-permitida",

                    title:
                        "Operación no permitida",

                    detail:
                        "El usuario no tiene permiso para realizar esta operación.",

                    extensions:
                        new Dictionary<string, object?>
                        {
                            ["codigo"] =
                                "OPERACION_NO_PERMITIDA"
                        })
                    .ExecuteAsync(contexto.HttpContext);
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet(
        "/api/v1/health",
        () => Results.Ok(new { estado = "ok" }))
    .AllowAnonymous();

await using (var alcance =
             app.Services.CreateAsyncScope())
{
    MesaSitecDbContext dbContext =
        alcance.ServiceProvider
            .GetRequiredService<MesaSitecDbContext>();

    await dbContext.Database.MigrateAsync();

    string? fechaBaseSemilla =
        builder.Configuration["SEED_FECHA_BASE"];

    await DatosSemilla.SembrarAsync(
        dbContext,
        fechaBaseSemilla);
}

await app.RunAsync();