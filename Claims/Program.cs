using Claims.Auditing;
using Claims.Data;
using Claims.Services;
using Claims.Validators;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddControllers()
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// SQL Server (LocalDB) for Audit
builder.Services.AddDbContext<AuditContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("AuditDb"));
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// MongoDB Atlas for Claims
builder.Services.AddDbContext<ClaimsContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDb");
    var databaseName = builder.Configuration["MongoDb:DatabaseName"];
    var client = new MongoClient(connectionString);
    options.UseMongoDB(client, databaseName);
});

// Register validators (SOLID: Dependency Inversion - depend on abstractions)
builder.Services.AddScoped<ICoverValidator, CoverValidator>();
builder.Services.AddScoped<IClaimValidator, ClaimValidator>();

// Register services
builder.Services.AddScoped<ICoversService, CoversService>();
builder.Services.AddScoped<IClaimsService, ClaimsService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Apply EF migrations for SQL Server
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AuditContext>();
    context.Database.Migrate();
}

app.Run();

public partial class Program { }
