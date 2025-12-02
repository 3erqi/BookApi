using BookApi.Data;
using Microsoft.EntityFrameworkCore;
using BookApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

//Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//add DbContext with SQLite
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=books.db"));

//CORS for Angular
builder.Services.AddCors(options => 
{
    options.AddPolicy("AllowAngular",
        policy => policy
            .SetIsOriginAllowed(origin => 
            {
                // Allow localhost for development
                if (origin.StartsWith("http://localhost") || origin.StartsWith("https://localhost"))
                    return true;
                
                // Allow local network IPs (192.168.x.x, 10.x.x.x, 172.16-31.x.x)
                if (origin.StartsWith("http://192.168.") || origin.StartsWith("https://192.168.") ||
                    origin.StartsWith("http://10.") || origin.StartsWith("https://10.") ||
                    (origin.StartsWith("http://172.") || origin.StartsWith("https://172.")))
                    return true;
                
                // Allow ngrok URLs
                if (origin.Contains(".ngrok") || origin.Contains("ngrok.io") || origin.Contains("ngrok-free.app"))
                    return true;
                
                // Allow any Netlify subdomain
                if (origin.EndsWith(".netlify.app"))
                    return true;
                
                // Add your specific production domains here if needed
                // Example: if (origin == "https://yourdomain.com") return true;
                
                return false;
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromSeconds(2520)));
});

//Register AuthService
builder.Services.AddScoped<AuthService>();

//Configure authentication with Jwt
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

var app = builder.Build();

// CORS must be before authentication
app.UseCors("AllowAngular");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Don't use HTTPS redirection in development to avoid issues
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Use the PORT environment variable if available (for Heroku deployment)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5156";
app.Run($"http://0.0.0.0:{port}");