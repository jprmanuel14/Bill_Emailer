using System;
using BillingMail.Plugin;
using BillColl_Main.AppDbContext;
using BillColl_Main.Options;
using BillColl_Main.Securities;
using BillColl_Main.Services;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BillColl_Main
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            Configuration = configuration;
            HostingEnvironment = environment;
        }

        public IConfiguration Configuration { get; }

        public IHostEnvironment HostingEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            ValidateDatabaseConfiguration(Configuration, HostingEnvironment);

            // Authentication settings (SSO toggle for non-production environments)
            services.Configure<AuthSettings>(Configuration.GetSection("AuthSettings"));
            var authSettings = Configuration.GetSection("AuthSettings").Get<AuthSettings>() ?? new AuthSettings();

            if (authSettings.EnableSso)
            {
                // Configure Azure Entra ID (OIDC) authentication
                services.AddAuthentication(options =>
                {
                    // Default scheme for successful authentication (after OIDC redirect)
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    // Default scheme for challenging unauthenticated users (initiating OIDC flow)
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.Cookie.HttpOnly = true;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                    options.SlidingExpiration = true;
                    options.LoginPath = "/Account/Index";
                })
                .AddOpenIdConnect(options =>
                {
                    var azureAdConfig = Configuration.GetSection("AzureAd");
                    options.Authority = $"{azureAdConfig["Instance"]}{azureAdConfig["TenantId"]}/v2.0";
                    options.ClientId = azureAdConfig["ClientId"];
                    options.ClientSecret = azureAdConfig["ClientSecret"];
                    options.CallbackPath = azureAdConfig["CallbackPath"];
                    options.SignedOutCallbackPath = azureAdConfig["SignedOutCallbackPath"];

                    options.ResponseType = OpenIdConnectResponseType.Code;

                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    options.Scope.Add("offline_access");

                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.MapInboundClaims = false;

                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = options.Authority,
                        NameClaimType = "preferred_username",
                        RoleClaimType = "roles"
                    };

                    options.Events.OnTokenValidated = async context =>
                    {
                        var identity = (ClaimsIdentity)context.Principal.Identity;
                        if (identity != null && !identity.HasClaim(c => c.Type == "TenantDisplayName"))
                        {
                            identity.AddClaim(new Claim("TenantDisplayName", "Your Company Name"));
                        }
                        await Task.CompletedTask;
                    };

                    options.Events.OnRedirectToIdentityProvider = context =>
                    {
                        if (context.Properties.Items.ContainsKey("acr_values"))
                        {
                            context.ProtocolMessage.Parameters.Add("acr_values", context.Properties.Items["acr_values"]);
                        }
                        return Task.CompletedTask;
                    };

                    options.Events.OnRedirectToIdentityProviderForSignOut = context =>
                    {
                        return Task.CompletedTask;
                    };
                });
            }
            else
            {
                services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options =>
                    {
                        options.LoginPath = "/Account/Index";
                    });
            }

            // Register DbConnectionFactory for dual PostgreSQL / SQL Server database support
            services.AddSingleton<DbConnectionFactory>();

            // Always-SQL Server connection to the shared estate (PH Report Databank, FinApps, HRIS)
            services.AddSingleton<SharedSqlConnectionFactory>();

            // Configure EF Core myDBContext (PostgreSQL or SQL Server)
            var dbProvider = Configuration["Connections:Primary:Provider"];

            if (string.IsNullOrWhiteSpace(dbProvider))
            {
                dbProvider = Configuration["BillingMail:DatabaseProvider"];
            }

            if (string.IsNullOrWhiteSpace(dbProvider))
            {
                dbProvider = "PostgreSQL";
            }

            var connString = ResolveConnectionString(Configuration);

            services.AddDbContext<myDBContext>(options =>
            {
                if (string.Equals(dbProvider, "PostgreSQL", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(dbProvider, "Postgres", StringComparison.OrdinalIgnoreCase))
                {
                    options.UseNpgsql(connString, npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "dbo"));
                }
                else
                {
                    options.UseSqlServer(connString);
                }
            });

            // Register Application Repositories
            services.AddScoped<IClients, ClientsSQLRepository>();
            services.AddScoped<IContacts, ContactsSQLRepository>();
            services.AddScoped<IExceptions, ExceptionSQLRepository>();
            services.AddScoped<IGroup, GroupSQLRepository>();
            services.AddScoped<IRemarks, RemarksSQLRepository>();
            services.AddScoped<IBillingMailStatus, BillingMailStatusSQLRepository>();
            services.AddScoped<IUsers, UserSQLRepository>();
            services.AddScoped<ILogs, LogsSQLRepository>();
            services.AddScoped<IMailService, MailRepository>();
            services.AddScoped<IUserRole, UserRoleRepository>();
            services.AddScoped<IEngagementSecretary, EngagementSecretaryRepository>();
            services.AddScoped<ISharedSql, SharedSqlRepository>();
            services.AddScoped<IEmployeeDirectory, EmployeeDirectoryRepository>();
            services.AddScoped<ISettingsRepository, SettingsRepository>();
            services.AddScoped<IUserContext, UserContext>();

            // DinkToPdf HTML-to-PDF converter and the report generator that uses it
            services.AddSingleton<ITools, PdfTools>();
            services.AddSingleton<IConverter, SynchronizedConverter>();
            services.AddScoped<IReportService, ReportService>();

            // Register DPPurposeStrings for data protection
            services.AddTransient<DPPurposeStrings>();

            // Register BillingMail plugin
            AddBillingMailPlugin(services, Configuration);

            // ASP.NET Core MVC/Razor setup
            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        private static void AddBillingMailPlugin(IServiceCollection services, IConfiguration configuration)
        {
            services.AddBillingMailPlugin(configuration);
        }

        /// <summary>
        /// Fails fast with an actionable message when the environment is not fully
        /// configured, so a deployment does not come up half-working and only fail later
        /// on the first request that happens to touch a missing connection.
        /// </summary>
        private static void ValidateDatabaseConfiguration(IConfiguration configuration, IHostEnvironment environment)
        {
            var problems = new List<string>();

            if (string.IsNullOrWhiteSpace(ResolveConnectionStringOrNull(configuration)))
            {
                problems.Add("No primary connection string. Set Connections:Primary:ConnectionString " +
                             "(env var Connections__Primary__ConnectionString).");
            }

            var provider = configuration["Connections:Primary:Provider"];

            if (string.IsNullOrWhiteSpace(provider))
            {
                problems.Add("No primary provider. Set Connections:Primary:Provider to PostgreSQL or SQLServer.");
            }
            else if (!provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase) &&
                     !provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase) &&
                     !provider.Equals("SQLServer", StringComparison.OrdinalIgnoreCase) &&
                     !provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                problems.Add($"Unrecognised Connections:Primary:Provider '{provider}'. Use PostgreSQL or SQLServer.");
            }

            foreach (var key in new[] { "Bcat", "Databank", "FinApps", "Hris" })
            {
                if (string.IsNullOrWhiteSpace(configuration[$"Connections:LinkedServers:{key}"]))
                {
                    problems.Add($"Missing linked server name Connections:LinkedServers:{key}.");
                }
            }

            if (problems.Count > 0)
            {
                throw new InvalidOperationException(
                    "Database configuration is incomplete:" + Environment.NewLine +
                    "  - " + string.Join(Environment.NewLine + "  - ", problems));
            }

            var logger = LoggerFactory
                .Create(config => config.AddConsole())
                .CreateLogger("BillColl_Main.Startup");

            logger.LogInformation(
                "Database configuration resolved. Provider={Provider}, SharedSql={SharedSql}, " +
                "LinkedServers: Bcat={Bcat}, Databank={Databank}, FinApps={FinApps}, Hris={Hris}. Environment={Environment}",
                ResolveProvider(configuration),
                string.IsNullOrWhiteSpace(configuration["Connections:SharedSql:ConnectionString"]) ? "not configured" : "configured",
                configuration["Connections:LinkedServers:Bcat"],
                configuration["Connections:LinkedServers:Databank"],
                configuration["Connections:LinkedServers:FinApps"],
                configuration["Connections:LinkedServers:Hris"],
                environment.EnvironmentName);
        }

        private static string ResolveProvider(IConfiguration configuration)
        {
            var provider = configuration["Connections:Primary:Provider"];

            if (string.IsNullOrWhiteSpace(provider))
            {
                provider = configuration["BillingMail:DatabaseProvider"];
            }

            return string.IsNullOrWhiteSpace(provider) ? "PostgreSQL" : provider;
        }

        private static string ResolveConnectionStringOrNull(IConfiguration configuration)
        {
            var connString = configuration["Connections:Primary:ConnectionString"];

            if (string.IsNullOrWhiteSpace(connString))
            {
                connString = configuration.GetConnectionString("myAppDBConnection");
            }

            if (string.IsNullOrWhiteSpace(connString))
            {
                connString = configuration["BillingMail:ConnectionString"];
            }

            return connString;
        }

        private static string ResolveConnectionString(IConfiguration configuration)
        {
            var connString = ResolveConnectionStringOrNull(configuration);

            if (string.IsNullOrWhiteSpace(connString))
            {
                throw new InvalidOperationException(
                    "No database connection string configured. Set Connections:Primary:ConnectionString, ConnectionStrings:myAppDBConnection or BillingMail:ConnectionString.");
            }

            return connString;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var authSettings = Configuration.GetSection("AuthSettings").Get<AuthSettings>() ?? new AuthSettings();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                
                endpoints.MapRazorPages();
            });
        }
    }
}
