using YouShallNotPassBackend.Storage;
using YouShallNotPassBackend.Cryptography;
using Microsoft.Extensions.Logging.AzureAppServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Logging.AddConsole();

builder.Services.Configure<AzureFileLoggerOptions>(options =>
{
    options.FileName = "azure-diagnostics-";
    options.FileSizeLimit = 50 * 1024;
    options.RetainedFileCountLimit = 5;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string entriesLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "entries");
Directory.CreateDirectory(entriesLocation);

Crypto crypto = new(builder.Configuration["ServerKey"]);
Storage storage = new(entriesLocation);
StorageManager storageManager = new(storage, crypto);

builder.Services.AddSingleton<IStorageManager>(storageManager);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();