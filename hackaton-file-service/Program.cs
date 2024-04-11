using hackathon_file_import.Core.Interfaces;
using hackathon_file_import.Core.Services;
using hackathon_file_import.Infrastructure.DataAccess;
using hackathon_file_import.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddSingleton<IRepository<byte[]>, BlobRepository>(provider =>
//    new BlobRepository(builder.Configuration.GetConnectionString("MongoDB")));
builder.Services.AddSingleton<MongoDbContext>(provider =>
    new MongoDbContext(
        builder.Configuration.GetConnectionString("MongoDB"),
        builder.Configuration.GetValue<string>("DatabaseName")));

builder.Services.AddTransient<IFileImportService, FileImportService>();
builder.Services.AddTransient<IRepository<byte[]>, BlobRepository>();
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

app.Run();

