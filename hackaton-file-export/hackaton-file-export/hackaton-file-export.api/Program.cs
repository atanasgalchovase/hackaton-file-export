using hackaton_file_export.common.Authorization;
using hackaton_file_export.common.Interfaces;
using hackaton_file_export.common.Models;
using hackaton_file_export.data.Interfaces;
using hackaton_file_export.data.Repositories;
using hackaton_file_export.services;
using hackaton_file_export.services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var appSettingsSection = builder.Configuration.GetSection(AppSettings.SectionName);
builder.Services.Configure<AppSettings>(appSettingsSection);
var appSettings = appSettingsSection.Get<AppSettings>();

var key = Encoding.ASCII.GetBytes(appSettings.Secret);
builder.Services
    .AddAuthentication(configuration =>
    {
        configuration.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        configuration.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(configuration =>
    {
        configuration.Events = new JwtBearerEvents
        {
            OnTokenValidated = context => {
                var userId = context.Principal?.Identity?.Name;
                if (userId == null)
                {
                    context.Fail("Unauthorized");
                    return Task.CompletedTask;
                }

                return Task.CompletedTask;
            }
        };
        configuration.RequireHttpsMetadata = false;
        configuration.SaveToken = true;
        configuration.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    })
    .AddCookie();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var mongoDbSection = builder.Configuration.GetSection(MongoDbSettings.SectionName);
builder.Services.Configure<MongoDbSettings>(mongoDbSection);
var mongoDbOptions = mongoDbSection.Get<MongoDbSettings>();

var connectionString = mongoDbOptions.ConnectionString;

var client = new MongoClient(connectionString);
var database = client.GetDatabase(mongoDbOptions.DatabaseName);

builder.Services.AddSingleton<IBlobRepository, BlobRepository>(provider =>
    new BlobRepository(connectionString));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IFileImportService, FileImportService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

var tokenValidationSettings = builder.Configuration.GetSection(TokenValidationSettings.SectionName);
builder.Services.Configure<TokenValidationSettings>(tokenValidationSettings);
var servicesUrlsOptions = tokenValidationSettings.Get<TokenValidationSettings>();

builder.Services.AddHttpClient("oauth", httpClient =>
{
    httpClient.BaseAddress = new Uri(servicesUrlsOptions.OAuthServiceUrl);
    httpClient.DefaultRequestHeaders.ConnectionClose = true;
});

builder.Services.AddSwaggerGen(configuration =>
{
    configuration.SwaggerDoc("v1", new OpenApiInfo { Title = "Web API", Version = "v1" });
    string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    configuration.IncludeXmlComments(xmlPath);
    configuration.CustomSchemaIds(type => type.ToString());
    configuration.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
            "JWT Authorization header using the Bearer scheme.\r\n\r\n" +
            "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
            "Example: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    configuration.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header
                },
                new List<string>()
            }
        });
});

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