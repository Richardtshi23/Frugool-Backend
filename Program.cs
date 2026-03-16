using CloudinaryDotNet;
using HolyWater.Server.Interfaces;
using HolyWater.Server.Models;
using HolyWater.Server.services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


var cloudName = builder.Configuration["CloudinarySettings:CloudName"];
var apiKey = builder.Configuration["CloudinarySettings:ApiKey"];
var apiSecret = builder.Configuration["CloudinarySettings:ApiSecret"];

// Create the Cloudinary Account object
Account account = new Account(cloudName, apiKey, apiSecret);
Cloudinary cloudinary = new Cloudinary(account);

// Register it as a Singleton (one instance for the whole app)
builder.Services.AddSingleton(cloudinary);

// --- 1. RENDER DYNAMIC PORT SETUP ---
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// --- 2. DATABASE SETUP (ULTIMATE ROBUST VERSION) ---
var rawConnString = Environment.GetEnvironmentVariable("DATABASE_URL")
                    ?? builder.Configuration.GetConnectionString("DefaultConnection");

string finalConnString = "";

if (!string.IsNullOrEmpty(rawConnString) && (rawConnString.StartsWith("postgres://") || rawConnString.StartsWith("postgresql://")))
{
    try
    {
        // Standardize prefix for Uri parser
        var formattedUri = rawConnString.Replace("postgresql://", "postgres://");
        var databaseUri = new Uri(formattedUri);

        var userInfo = databaseUri.UserInfo.Split(':');
        var user = userInfo[0];
        var password = userInfo.Length > 1 ? userInfo[1] : "";
        var host = databaseUri.Host;

        // FIX: If port is -1 (missing from string), default to 5432
        var portNumber = databaseUri.Port <= 0 ? 5432 : databaseUri.Port;
        var database = databaseUri.LocalPath.TrimStart('/');

        finalConnString = $"Host={host};Port={portNumber};Database={database};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
    }
    catch (Exception ex)
    {
        Console.WriteLine($"DATABASE PARSING ERROR: {ex.Message}");
        finalConnString = rawConnString; // Fallback to raw if logic fails
    }
}
else
{
    finalConnString = rawConnString ?? "";
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(finalConnString));

// --- 3. SERVICES ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IProductsService, ProductsService>();
builder.Services.AddScoped<IProductsAdminService, ProductsAdminService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// JWT Setup
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? builder.Configuration["Jwt:Key"];
var key = Encoding.UTF8.GetBytes(jwtKey ?? "YourFallbackSecretKey_MustBeLongEnough");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});

// --- 4. CORS CONFIGURATION ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy => policy.WithOrigins("https://frugool-angular.onrender.com")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
});

// Large File Upload Support
builder.Services.Configure<IISServerOptions>(options => { options.MaxRequestBodySize = int.MaxValue; });
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(x =>
{
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = int.MaxValue;
    x.MemoryBufferThreshold = int.MaxValue;
});

var app = builder.Build();

// --- 5. AUTOMATIC DATABASE MIGRATION ---
// This creates your tables on Render if they don't exist yet
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
        Console.WriteLine("Database Migration Successful.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred during migration: {ex.Message}");
    }
}

// --- 6. MIDDLEWARE PIPELINE ---

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            context.Response.Headers.Append("Access-Control-Allow-Origin", "https://frugool-angular.onrender.com");
            context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
            await context.Response.WriteAsync("{\"error\":\"Internal Server Error. Check Render logs.\"}");
        });
    });
}

app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAngularApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();