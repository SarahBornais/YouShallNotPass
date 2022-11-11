using YouShallNotPassBackend.Storage;
using YouShallNotPassBackend.Security;
using Microsoft.Extensions.Logging.AzureAppServices;
using Microsoft.Net.Http.Headers;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Logging.AddConsole();

builder.Services.Configure<AzureFileLoggerOptions>(options =>
{
    options.FileName = "azure-diagnostics-";
    options.FileSizeLimit = 50 * 1024;
    options.RetainedFileCountLimit = 5;
});

builder.Services.AddCors();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SupportNonNullableReferenceTypes();

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    options.OperationFilter<AuthorizationOperationFilter>();

    options.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            Description =
                "JWT Authorization header using the Bearer scheme. \r\n\r\n " +
                "Enter token without 'Brearer ' prefix",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer"
        });
});

string entriesLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "entries");
Directory.CreateDirectory(entriesLocation);

byte[] serverKey = Convert.FromHexString(builder.Configuration["ServerKey"] ??
    throw new ArgumentException("ServerKey is not defined. For local deployments, see README for instructions."));

string issuer = builder.Configuration["Jwt:Issuer"] ?? throw new ArgumentException("Jwt:Issuer is not defined.");
string audience = builder.Configuration["Jwt:Audience"] ?? throw new ArgumentException("Jwt:Audience is not defined.");

Crypto crypto = new(serverKey);
Storage storage = new(entriesLocation);
StorageManager storageManager = new(storage, crypto, 60 * 1000);

string usersDbLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "users.db");
IdentityDatabase authenticator = new(usersDbLocation);

JwtAuthority jwtAuthority = new(issuer, audience, serverKey);

builder.Services.AddSingleton<IStorageManager>(storageManager);
builder.Services.AddSingleton<IAuthenticator>(authenticator);
builder.Services.AddSingleton<ITokenAuthority>(jwtAuthority);

var app = builder.Build();

app.MapGet("/isAlive", () => Results.Ok()).AllowAnonymous();

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();

app.UseMiddleware<BearerAuthorizationMiddleware>();

app.MapControllers();

app.Run();
