using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Constants;
using Fashia.Infrastructure.Data;
using Fashia.Infrastructure.Data.Interceptors;
using Fashia.Infrastructure.Email;
using Fashia.Infrastructure.Identity;
using Fashia.Infrastructure.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString(Services.Database);
        Guard.Against.Null(
            connectionString,
            message: $"Connection string '{Services.Database}' not found."
        );

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>(
            (sp, options) =>
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                options.UseNpgsql(connectionString);
                options.ConfigureWarnings(warnings =>
                    warnings.Ignore(RelationalEventId.PendingModelChangesWarning)
                );
            }
        );

        builder.Services.Configure<SmtpOptions>(
            builder.Configuration.GetSection(SmtpOptions.SectionName)
        );

        builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
        builder.Services.AddSingleton<IEmailQueue, ChannelEmailQueue>();
        builder.Services.AddHostedService<QueuedEmailSenderService>();

        builder.EnrichNpgsqlDbContext<ApplicationDbContext>();

        builder.Services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>()
        );

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        builder.Services.AddScoped<IFileStorageService, CloudinaryStorageService>();

        builder
            .Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        builder
            .Services.AddAuthorizationBuilder()
            .AddPolicy(
                Policies.CanManageCategories,
                policy => policy.RequireRole(Roles.Administrator)
            )
            .AddPolicy(
                Policies.CanManageProducts,
                policy => policy.RequireRole(Roles.Administrator)
            )
            .AddPolicy(
                Policies.CanManageBranches,
                policy => policy.RequireRole(Roles.Administrator)
            )
            .AddPolicy(
                Policies.CanManageBranchInventories,
                policy => policy.RequireRole(Roles.Administrator, Roles.BranchManager)
            );

        builder
            .Services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders()
            .AddApiEndpoints();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddTransient<IIdentityService, IdentityService>();
    }
}
