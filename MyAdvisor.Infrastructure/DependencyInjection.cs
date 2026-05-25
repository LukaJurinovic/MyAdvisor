using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyAdvisor.Application.Interfaces.Contracts;
using MyAdvisor.Application.Interfaces.Repositories;
using MyAdvisor.Application.Interfaces.Services.Domain;
using MyAdvisor.Application.Interfaces.Services.App;
using MyAdvisor.Application.Interfaces.Services.Auth;
using MyAdvisor.Application.Interfaces.Services.AI;
using MyAdvisor.Application.Mappers;
using MyAdvisor.Infrastructure.AI;
using MyAdvisor.Infrastructure.Auth;
using MyAdvisor.Infrastructure.Identity;
using MyAdvisor.Infrastructure.Persistence;
using MyAdvisor.Infrastructure.Repositories;
using MyAdvisor.Application.Services;
using MyAdvisor.Infrastructure.Services.AI;
using MyAdvisor.Infrastructure.Services.Background;
using MyAdvisor.Infrastructure.Services.Auth;
using UnitOfWork = MyAdvisor.Infrastructure.Persistence.UnitOfWork;

namespace MyAdvisor.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
                ?? throw new InvalidOperationException("JwtSettings are not configured.");

            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

            services.AddDbContext<AppDbContext>(options =>
                options.UseMySQL(connectionString));

            services.AddIdentity<ApplicationUser, IdentityRole<int>>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParametersFactory(
                    Microsoft.Extensions.Options.Options.Create(jwtSettings)).Create();
            });

            // Core
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSingleton<JwtTokenGenerator>();

            // Mappers (stateless — singleton is correct)
            services.AddSingleton<UserMapper>();
            services.AddSingleton<CategoryMapper>();
            services.AddSingleton<FinancialDiaryMapper>();
            services.AddSingleton<TransactionMapper>();
            services.AddSingleton<RecurringTransactionMapper>();

            // Identity services
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();

            // Domain services
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IFinancialDiaryService, FinancialDiaryService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<ITransactionAiLogService, TransactionAiLogService>();
            services.AddScoped<IRecurringTransactionService, RecurringTransactionService>();
            services.AddScoped<ISpendingStatisticService, SpendingStatisticService>();

            // App services
            services.AddScoped<IDiaryTransactionService, DiaryTransactionService>();
            services.AddScoped<IAiTransactionImportService, AiTransactionImportService>();
            services.AddScoped<IFinancialChatService, FinancialChatService>();
            services.AddScoped<ISpendingStatisticComputeService, SpendingStatisticComputeService>();

            // AI / Gemini
            services.Configure<GeminiSettings>(configuration.GetSection("GeminiSettings"));
            services.AddHttpClient<IGeminiService, GeminiService>((sp, client) =>
            {
                var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<GeminiSettings>>().Value;
                client.BaseAddress = new Uri(settings.BaseUrl);
                client.DefaultRequestHeaders.Add("x-goog-api-key", settings.ApiKey);
            });

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IFinancialDiaryRepository, FinancialDiaryRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IRecurringTransactionRepository, RecurringTransactionRepository>();
            services.AddScoped<ITransactionAiLogRepository, TransactionAiLogRepository>();
            services.AddScoped<ISpendingStatisticRepository, SpendingStatisticRepository>();

            // Background services
            services.AddHostedService<RefreshTokenCleanupService>();
            services.AddHostedService<RecurringTransactionProcessorService>();
            services.AddHostedService<SpendingStatisticProcessorService>();

            return services;
        }
    }
}
