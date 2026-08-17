using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Nexit.API.Middleware;
using Nexit.Application;
using Nexit.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().WriteTo.File("logs/nexit-.log", rollingInterval: RollingInterval.Day).CreateLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Jwt:Authority"];
        options.Audience = builder.Configuration["Jwt:Audience"];
        options.TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = builder.Configuration.GetValue("Jwt:ValidateIssuer", true), ValidateAudience = builder.Configuration.GetValue("Jwt:ValidateAudience", true), ValidateLifetime = true };
    });
    builder.Services.AddAuthorization(options => options.AddPolicy("AdminOnly", policy => policy.RequireAuthenticatedUser().RequireAssertion(context =>
        context.User.IsInRole("admin") || context.User.HasClaim("app_role", "admin") || context.User.HasClaim("user_role", "admin"))));
    builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
    builder.Services.AddControllers();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "Nexit API", Version = "v1", Description = "API REST para la gestión operativa de Nexit." });
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Name = "Authorization", In = ParameterLocation.Header, Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT" });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, [] } });
    });
    var app = builder.Build();
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.Run();
}
catch (Exception ex) { Log.Fatal(ex, "Nexit API terminated unexpectedly"); }
finally { Log.CloseAndFlush(); }
