using Microsoft.EntityFrameworkCore;
using SamuBarber.Api.Data;
using SamuBarber.Api.Services;

var builder = WebApplication.CreateBuilder(args);


// --- AGREGAR ESTAS LÍNEAS PARA CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});
// --------------------------------------
// Configurar conexión a PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

builder.Services.AddControllers();

// 1. Agregar servicios de Swagger y Auditoría
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<AuditoriaService>();

var app = builder.Build();

// 2. Activar la interfaz de Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors("AllowAll");
app.MapControllers();

app.Run();