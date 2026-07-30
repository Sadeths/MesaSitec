using MesaSitec.Infraestructura;
using MesaSitec.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var cadenaConexion =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "No se encontro la cadena de conexion DefaultConnection");

builder.Services.AgregarInfraEstructura(cadenaConexion);

var app = builder.Build();

//Swagger debe estar disponible para el evaluador revise la API
app.UseSwagger();
app.UseSwaggerUI();


app.UseAuthorization();

app.MapControllers();

//Endpoint publico requerido por el enunciado
app.MapGet(
    "/api/v1/health",
    () => Results.Ok(new{estado = "ok"}))
    .AllowAnonymous();

//Aplicar automaticamente las migraciones pendientes.
await using (var alcance = app.Services.CreateAsyncScope())
{
    var dbContext = alcance.ServiceProvider
        .GetRequiredService<MesaSitecDbContext>();


    await dbContext.Database.MigrateAsync();
}

await app.RunAsync();