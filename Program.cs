using System.Text;
using System.IO;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using ManoVecinaAPI.Data;
using ManoVecinaAPI.Mappings;
using ManoVecinaAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// DATABASE
// =========================================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// =========================================================
// AUTOMAPPER
// =========================================================
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// =========================================================
// SERVICES
// =========================================================
builder.Services.AddScoped<ImageStorageService>();
builder.Services.AddScoped<GeoapifyService>();
builder.Services.AddScoped<FirebaseService>();
builder.Services.AddHttpClient();

// =========================================================
// CONTROLLERS
// =========================================================
builder.Services.AddControllers();

// =========================================================
// CORS
// =========================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// =========================================================
// JWT AUTH
// =========================================================
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection.GetValue<string>("Key") 
             ?? "THIS_KEY_MUST_BE_32_CHARS_MINIMUM_12345678";

if (jwtKey.Length < 32)
    throw new Exception("JWT Key debe tener 32 caracteres mínimo.");

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection.GetValue<string>("Issuer"),
            ValidAudience = jwtSection.GetValue<string>("Audience"),
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ClockSkew = TimeSpan.Zero
        };
    });

// =========================================================
// SWAGGER
// =========================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ManoVecinaAPI",
        Version = "v1",
        Description = "API para la app ManoVecina"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Bearer auth. Ejemplo: Bearer eyJhbGci...",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });
});

// =========================================================
// FIREBASE
// =========================================================
var firebaseSection = builder.Configuration.GetSection("Firebase");
var firebaseCredentialsPath = firebaseSection.GetValue<string>("CredentialsFilePath");

if (!string.IsNullOrEmpty(firebaseCredentialsPath) && File.Exists(firebaseCredentialsPath))
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromFile(firebaseCredentialsPath)
    });
}

var app = builder.Build();

// =========================================================
// SWAGGER ALWAYS ENABLED
// =========================================================
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ManoVecinaAPI v1");
    c.RoutePrefix = "swagger";
});

// =========================================================
// CORS HEADER (fix Flutter Web / Azure)
// =========================================================
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
    await next();
});

// =========================================================
// STATIC FILES: WWWROOT
// =========================================================
app.UseStaticFiles();

// =========================================================
// STATIC FILES: UPLOADS (CRÍTICO)
// =========================================================
var uploadsRoot = Path.Combine(
    builder.Environment.WebRootPath ?? "wwwroot",
    "uploads"
);

if (!Directory.Exists(uploadsRoot))
    Directory.CreateDirectory(uploadsRoot);

var contentTypeProvider = new FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".jpg"] = "image/jpeg";
contentTypeProvider.Mappings[".jpeg"] = "image/jpeg";
contentTypeProvider.Mappings[".png"] = "image/png";
contentTypeProvider.Mappings[".gif"] = "image/gif";
contentTypeProvider.Mappings[".webp"] = "image/webp";

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsRoot),
    RequestPath = "/uploads",
    ServeUnknownFileTypes = true,
    ContentTypeProvider = contentTypeProvider
});

// =========================================================
// PIPELINE
// =========================================================
app.UseCors("DefaultCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
