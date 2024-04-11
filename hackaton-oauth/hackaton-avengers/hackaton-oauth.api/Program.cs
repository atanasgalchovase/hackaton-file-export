using hackaton_oauth.common;
using hackaton_oauth.common.Models;
using hackaton_oauth.data;
using hackaton_oauth.data.Models;
using hackaton_oauth.services;
using hackaton_oauth.services.Inderfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Reflection;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services.AddDbContext<DataContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("WebApiDatabase"),
        x => x.MigrationsHistoryTable("_EfMigrations", builder.Configuration.GetValue<string>("PostgresUserSchema"))));

builder.Services.AddHealthChecks();

var appSettingsSection = builder.Configuration.GetSection("AppSettings");
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
            OnTokenValidated = context =>
            {
                var db = context.HttpContext.RequestServices.GetRequiredService<DataContext>();
                var userId = context.Principal?.Identity?.Name;
                if (userId == null)
                {
                    context.Fail("Unauthorized");
                    return Task.CompletedTask;
                }

                var user = db.Users.AsNoTracking().Include(x => x.Role)
                    .FirstOrDefault(x => x.Id == Guid.Parse(userId));

                if (user == null)
                {
                    context.Fail("Unauthorized");
                    return Task.CompletedTask;
                }

                if (user.RoleId != null)
                {
                    var identity = context.Principal?.Identity as ClaimsIdentity;
                    identity?.AddClaim(new Claim(ClaimTypes.Role, user.Role.Name));
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddCors();

builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

await SeedInitialData(app);

app.UseRouting();

app.UseCors(configuration => configuration.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web API V1");
    c.DisplayRequestDuration();
});
app.Run();

static async Task SeedInitialData(WebApplication app) 
{
    using var serviceScope = app.Services.GetService<IServiceScopeFactory>()?.CreateScope();

    if (serviceScope == null) 
        throw new ApplicationException("Cannot create service scope.");

    var db = serviceScope.ServiceProvider.GetService<DataContext>();

    var userSchemaName = app.Configuration.GetValue<string>("PostgresUserSchema");

    var userService = serviceScope.ServiceProvider.GetService<IUserService>();
    if (db == null || userService == null) 
        throw new ApplicationException("Cannot get service.");

    var appSettings = app.Configuration.GetSection("AppSettings").Get<AppSettings>();

    //db.Database.Migrate();

    if (db.Users.FirstOrDefault(x => x.Username == appSettings.AdminUsername) == null)
    {
        var (passwordHash, passwordSalt) = userService.CreateHash(appSettings.AdminPassword);

        var superAdmin = new User
        {
            Id = Guid.NewGuid(),
            Username = appSettings.AdminUsername,
            Email = appSettings.AdminEmail,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        };

        db.Users.Add(superAdmin);


        var (passwordHashS2S, passwordSaltS2S) = userService.CreateHash(appSettings.S2SPassword);
        var s2sUser = new User
        {
            Id = Guid.NewGuid(),
            Username = appSettings.S2SUsername,
            Email = appSettings.AdminEmail,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            PasswordHash = passwordHashS2S,
            PasswordSalt = passwordSaltS2S
        };

        db.Users.Add(s2sUser);

        db.SaveChanges();

        var superAdminRole = new Role
        {
            Id = Guid.NewGuid(),
            Description = "Admin Role",
            Name = "SuperAdmin",
            CreatedById = superAdmin.Id
        };

        db.Roles.Add(superAdminRole);
        superAdmin.RoleId = superAdminRole.Id;

        var s2sUserRole = new Role
        {
            Id = Guid.NewGuid(),
            Description = "S2S User Role",
            Name = "S2S",
            CreatedById = superAdmin.Id
        };

        db.Roles.Add(s2sUserRole);
        s2sUser.RoleId = s2sUserRole.Id;

        var userRole = new Role
        {
            Id = Guid.NewGuid(),
            Description = "User Role",
            Name = "User",
            CreatedById = superAdmin.Id
        };
        db.Roles.Add(userRole);

        var readOnlyRole = new Role
        {
            Id = Guid.NewGuid(),
            Description = "Read Only User Role",
            Name = "ReadOnly",
            CreatedById = superAdmin.Id
        };
        db.Roles.Add(readOnlyRole);

        db.SaveChanges();
    }
}