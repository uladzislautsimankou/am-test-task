using AM.TestTask.Api.Handlers;
using AM.TestTask.Business.Extensions;
using AM.TestTask.Data.Relational.AppDatabase.PostgreSql.Extensions;
using AM.TestTask.Data.Cache.Extentions;
using AM.TestTask.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBusiness();
builder.Services.AddDataRelationalPostgreSql(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddDataCache(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddHealthChecks();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod();

        if (allowedOrigins != null && allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .AllowCredentials();
        }
        else
        {
            policy.AllowAnyOrigin();
        }
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");

app.UseExceptionHandler();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Meteorite API");
});

app.UseHttpsRedirection();

app.Run();
