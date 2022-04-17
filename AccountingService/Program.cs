using AccountingService.Orchestration;
using AccountingService.Repositories;
using AccountingService.Services;
using AutoMapper;
using Serilog;
using CorrelationId;
using CorrelationId.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using IdentityServer4.AccessTokenValidation;

var builder = WebApplication.CreateBuilder(args);

//  Configure the host
builder.Host

    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddEnvironmentVariables();
    })
    .UseSerilog((hostingcontext, config) => 
    {
        config.ReadFrom.Configuration(hostingcontext.Configuration);
    });

// Add services to the container.
//  Add versioning
builder.Services.AddApiVersioning(options =>
{
    options.ApiVersionReader = new HeaderVersionReader();
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
});

builder.Services.AddDefaultCorrelationId(options => 
{
    options.IncludeInResponse = true;
    options.AddToLoggingScope = true;
});

builder.Services.AddCors();

builder.Services.AddControllers();

builder.Services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme,
                    jwtOptions =>
                    {
                        jwtOptions.Authority = builder.Configuration.GetSection("OAuth2").GetValue<string>("AuthorityUrl");
                        jwtOptions.Audience = builder.Configuration.GetSection("OAuth2").GetValue<string>("Audience");
                    },
                    referenceOptions =>
                    {
                        referenceOptions.Authority = builder.Configuration.GetSection("OAuth2").GetValue<string>("AuthorityUrl");
                        referenceOptions.ClientId = builder.Configuration.GetSection("OAuth2").GetValue<string>("IntrospectionClient");
                        referenceOptions.ClientSecret = builder.Configuration.GetSection("OAuth2").GetValue<string>("IntrospectionSecret");
                    });

builder.Services.AddAuthorization(options => 
{
    foreach(var policy in builder.Configuration.GetSection("OAuth2").GetSection("Policies").GetChildren())
    {
        var scopes = new List<string>();

        foreach (var scope in policy.GetSection("Scopes").GetChildren())
            scopes.Add(scope.Value);

        options.AddPolicy(policy.GetValue<string>("Policy"), builder =>
        {
            builder.RequireAuthenticatedUser();
            builder.RequireClaim("scope", scopes.ToArray());
        });

    }
});

builder.Services.AddScoped<IFinanceRepository, FinanceRepository>();
builder.Services.AddScoped<IOrchestrator, Orchestrator>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen( c => 
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    {
        Title = "Accounting Service",
        Description = "",
        Version = "v1",
        Contact = new OpenApiContact 
        {
            Name = "Michael Zuniga",
            Email = "michael.zuniga727@gmail.com"
        }
    });

    c.DescribeAllParametersInCamelCase();
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme 
    {
        Description = "IDS Token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
                },
                Name= "Bearer",
                In = ParameterLocation.Header,
            },
            Array.Empty<string>()
        }
    });

#pragma warning disable CS8604 // Possible null reference argument.
    c.IncludeXmlComments(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "AccountingService.xml"));
#pragma warning restore CS8604 // Possible null reference argument.
});

AutoMapperFactory.MapperConfiguration = new MapperConfiguration(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
AutoMapperFactory.CreateMapper();

var app = builder.Build();

app.UseCorrelationId();
app.UseCors(builder => builder.AllowAnyMethod()
                              .AllowAnyHeader()
                              .SetIsOriginAllowed(origin => true)
                              .AllowCredentials()
                              .WithExposedHeaders(new string[] { "location" }));


app.UseSwagger();
app.UseSwaggerUI(options => 
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "AccountingService V1");
    options.RoutePrefix = string.Empty;
});

app.UseSerilogRequestLogging();
app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
