using System.Text;
using App.Configuration;
using Core.Context;
using Core.DTOs;
using Core.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Repositories;
using Repositories.Interfaces;
using Services;
using Services.Interfaces;
using Services.Practice;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ─── Swagger / OpenAPI ────────────────────────────────────────────────────────
// Swashbuckle автоматично сканує [ApiController]-и та [ProducesResponseType]-атрибути
// і генерує інтерактивну документацію на /swagger
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
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole<int>>()
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders()
.AddRoles<IdentityRole<int>>();

builder.Services.AddMemoryCache();
static bool IsTransient(HttpResponseMessage? response) =>
    response is null ||
    (int)response.StatusCode >= 500 ||           // 500, 502, 503, 504 — серверні помилки
    response.StatusCode == System.Net.HttpStatusCode.RequestTimeout; // 408

var geminiSection = builder.Configuration.GetRequiredSection("Gemini");
var geminiOpts = geminiSection.Get<ApiClientOptions>()!;

builder.Services.Configure<ApiClientOptions>("Gemini", geminiSection);

var geminiApiKey = builder.Configuration["ExternalApis:Gemini:ApiKey"]
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
    new GeminiService(httpClient, geminiApiKey));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
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
        },
        OnChallenge = context =>
        {
            // 401
            context.HandleResponse();
            context.Response.Redirect("/login");
            return Task.CompletedTask;
        },
        OnForbidden = context =>
        {
            // 403
            context.Response.Redirect("/access-denied");
            return Task.CompletedTask;
        }
    };
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
});


builder.Services.Configure<EmailSettingsDTO>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<SetService>();
builder.Services.AddScoped<FlashcardService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<CollectionService>();
builder.Services.AddScoped<ReportService>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<PracticeService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<IAnswerService, AnswerService>();
builder.Services.AddTransient<EmailService>();
builder.Services.AddTransient<IEmailService, EmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
