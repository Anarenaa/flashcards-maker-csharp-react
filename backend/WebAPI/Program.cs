using System.ComponentModel.Design;
using System.Data.Common;
using System.Text;
using App.Configuration;
using Core.Context;
using Core.DTOs;
using Core.Exceptions;
using Core.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Repositories;
using Repositories.Interfaces;
using Services;
using Services.Interfaces;
using Services.Practice;

using IDictionaryService = Services.Interfaces.IDictionaryService;

// Дозволяємо .NET працювати з датами Postgres без проблем із часовими поясами
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Flashcards Maker API",
        Version = "v1",
        Description = "REST API для управління флеш-картками."
    });
});

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;              // Обов'язково хоча б одна цифра
    options.Password.RequiredLength = 6;               // Мінімальна довжина пароля 
    options.Password.RequireNonAlphanumeric = false;   // Чи обов'язкові спецсимволи (!, @, #, $)?
    options.Password.RequireUppercase = false;         // Чи обов'язкова велика літера?
    options.Password.RequireLowercase = false;         // Чи обов'язкова мала літера?
    options.Password.RequiredUniqueChars = 1;          // Кількість унікальних символів
})
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders()
.AddRoles<IdentityRole<int>>();

builder.Services.AddMemoryCache();

static bool IsTransient(HttpResponseMessage? response) =>
    response is null ||
    (int)response.StatusCode >= 500 ||
    response.StatusCode == System.Net.HttpStatusCode.RequestTimeout ||
    response.StatusCode == System.Net.HttpStatusCode.TooManyRequests;

var geminiSection = builder.Configuration.GetRequiredSection("Gemini");
var geminiOpts = geminiSection.Get<ApiClientOptions>()!;

builder.Services.Configure<ApiClientOptions>("Gemini", geminiSection);

var geminiApiKey = builder.Configuration["Gemini:ApiKey"]
                   ?? throw new Exception("Gemini API Key is missing!");

// Реєструємо Typed HttpClient з конвеєром Polly
var httpClientBuilder = builder.Services.AddHttpClient<IGeminiService, GeminiService>(client =>
{
    client.BaseAddress = new Uri(geminiOpts.BaseUrl);
});
httpClientBuilder.AddResilienceHandler("gemini-pipeline", pipelineBuilder =>
{
    pipelineBuilder.AddTimeout(TimeSpan.FromSeconds(geminiOpts.TimeoutSeconds * 2));

    pipelineBuilder.AddRetry(new HttpRetryStrategyOptions
    {
        MaxRetryAttempts = geminiOpts.MaxRetryAttempts,
        Delay = TimeSpan.FromSeconds(1),
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true,
        ShouldHandle = args => ValueTask.FromResult(
            args.Outcome.Exception is not null || IsTransient(args.Outcome.Result))
    });

    pipelineBuilder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
    {
        MinimumThroughput = 5,
        FailureRatio = 0.5,
        SamplingDuration = TimeSpan.FromSeconds(30),
        BreakDuration = TimeSpan.FromSeconds(geminiOpts.BreakDurationSeconds),
        ShouldHandle = args => ValueTask.FromResult(
            args.Outcome.Exception is not null || IsTransient(args.Outcome.Result))
    });

    pipelineBuilder.AddTimeout(TimeSpan.FromSeconds(geminiOpts.TimeoutSeconds));
});

httpClientBuilder.AddTypedClient<IGeminiService>((httpClient, sp) =>
    new GeminiService(httpClient, geminiApiKey, sp.GetRequiredService<ILogger<GeminiService>>(), sp.GetRequiredService<IUnitOfWork>()));

// --- НАЛАШТУВАННЯ WIKIPEDIA ---
var wikiConfig = builder.Configuration.GetSection("Wikipedia");
var wikiUserAgent = wikiConfig["UserAgent"];
var wikiTimeout = double.Parse(wikiConfig["TimeoutSeconds"]);
var wikiMaxRetries = int.Parse(wikiConfig["MaxRetryAttempts"]);
var wikiBreakDuration = double.Parse(wikiConfig["BreakDurationSeconds"]);

var wikiBuilder = builder.Services.AddHttpClient<IWikipediaService, WikipediaService>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", wikiUserAgent);
    client.Timeout = TimeSpan.FromSeconds(wikiTimeout * 2);
});

wikiBuilder.AddResilienceHandler("wikipedia-pipeline", pipelineBuilder =>
{
    pipelineBuilder.AddRetry(new HttpRetryStrategyOptions
    {
        MaxRetryAttempts = wikiMaxRetries,
        Delay = TimeSpan.FromSeconds(1),
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true,
        ShouldHandle = args => ValueTask.FromResult(
            args.Outcome.Exception is not null || IsTransient(args.Outcome.Result))
    });

    pipelineBuilder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
    {
        MinimumThroughput = 5,
        FailureRatio = 0.5,
        SamplingDuration = TimeSpan.FromSeconds(30),
        BreakDuration = TimeSpan.FromSeconds(wikiBreakDuration),
        ShouldHandle = args => ValueTask.FromResult(
            args.Outcome.Exception is not null || IsTransient(args.Outcome.Result))
    });

    pipelineBuilder.AddTimeout(TimeSpan.FromSeconds(wikiTimeout));
});

// --- НАЛАШТУВАННЯ MYMEMORY ---
var myMemoryConfig = builder.Configuration.GetSection("MyMemory");
var myMemoryBaseUrl = myMemoryConfig["BaseUrl"];
var myMemoryTimeout = double.Parse(myMemoryConfig["TimeoutSeconds"]);
var myMemoryMaxRetries = int.Parse(myMemoryConfig["MaxRetryAttempts"]);
var myMemoryBreakDuration = double.Parse(myMemoryConfig["BreakDurationSeconds"]);

var myMemoryBuilder = builder.Services.AddHttpClient<IDictionaryService, DictionaryService>(client =>
{
    client.BaseAddress = new Uri(myMemoryBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(myMemoryTimeout * 2);
});

myMemoryBuilder.AddResilienceHandler("mymemory-pipeline", pipelineBuilder =>
{
    pipelineBuilder.AddRetry(new HttpRetryStrategyOptions
    {
        MaxRetryAttempts = myMemoryMaxRetries,
        Delay = TimeSpan.FromSeconds(1),
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true,
        ShouldHandle = args => ValueTask.FromResult(
            args.Outcome.Exception is not null || IsTransient(args.Outcome.Result))
    });

    pipelineBuilder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
    {
        MinimumThroughput = 5,
        FailureRatio = 0.5,
        SamplingDuration = TimeSpan.FromSeconds(30),
        BreakDuration = TimeSpan.FromSeconds(myMemoryBreakDuration),
        ShouldHandle = args => ValueTask.FromResult(
            args.Outcome.Exception is not null || IsTransient(args.Outcome.Result))
    });

    pipelineBuilder.AddTimeout(TimeSpan.FromSeconds(myMemoryTimeout));
});

// АУТЕНТИФІКАЦІЯ І АВТОРИЗАЦІЯ
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        RoleClaimType = System.Security.Claims.ClaimTypes.Role
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Cookies["AuthToken"];
            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            try
            {
                var userManager = context.HttpContext.RequestServices
                    .GetRequiredService<UserManager<User>>();

                var userIdClaim = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    context.Fail("Unauthorized");
                    return;
                }

                var user = await userManager.FindByIdAsync(userIdClaim.Value);

                if (user == null || user.IsBanned || (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow))
                {
                    context.Fail("User is banned");
                    context.Response.Cookies.Delete("AuthToken");
                }
            }
            catch (DbException ex)
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                logger.LogError(ex, "Database error during token validation for user.");

                context.Fail("Authentication failed due to a database error.");

                context.Response.Cookies.Delete("AuthToken");
                context.Response.Redirect("/Home/ServiceUnavailable");
            }
        },
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.Redirect("/login");
            return Task.CompletedTask;
        },
        OnForbidden = context =>
        {
            context.Response.Redirect("/access-denied");
            return Task.CompletedTask;
        }
    };
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
    {
        OnRemoteFailure = context =>
        {
            context.Response.Redirect("/Home/ServiceUnavailable");
            context.HandleResponse();
            return Task.CompletedTask;
        }
    };
});

// РЕЄСТРАЦІЯ СЕРВІСІВ І РЕПОЗИТОРІЇВ
builder.Services.Configure<EmailSettingsDTO>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<SetService>();
builder.Services.AddScoped<FlashcardService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<CollectionService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<IPracticeRepository, PracticeRepository>();
builder.Services.AddScoped<IAnswerService, AnswerService>();
builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IPracticeService, PracticeService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddTransient<IEmailService, EmailService>();

builder.Services.AddScoped<IWikipediaService, WikipediaService>();
builder.Services.AddScoped<IDictionaryService, DictionaryService>();
builder.Services.AddScoped<IHintService, HintService>();

var app = builder.Build();

// Автоматичний запуск міграцій при старті
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<DataContext>();
        context.Database.Migrate();
        Console.WriteLine("----> Database Migration Successful");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"----> Migration Error: {ex.Message}");
    }
}

// Зчитування HTTPS заголовків від проксі-сервера Render
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                       Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (KeyNotFoundException)
    {
        context.Response.StatusCode = 404;
        context.Response.Redirect($"/Home/NotFoundPage/404");
    }
    catch (DbException)
    {
        if (!context.Request.Path.Value!.StartsWith("/api/"))
        {
            context.Response.Redirect("/Home/ServiceUnavailable");
        }
        else
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsJsonAsync(new { error = "Database connection error." });
        }
    }
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Flashcards Maker API v1");
    });
}

app.UseHttpsRedirection();
app.UseStatusCodePagesWithReExecute("/Home/NotFoundPage/{0}");
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();