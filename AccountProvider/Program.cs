using Data.Contexts;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddDbContext<DataContext>(x => x.UseSqlServer(context.Configuration.GetConnectionString("SqlServer")));
        services.AddDefaultIdentity<UserAddress>(x =>
        {
            x.SignIn.RequireConfirmedAccount = true;
            x.User.RequireUniqueEmail = true;
            x.Password.RequiredLength = 8;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<DataContext>();

        services.AddAuthorization(x =>
        {
            x.AddPolicy("SuperAdmins", policy => policy.RequireRole("SuperAdmin"));
            x.AddPolicy("CIO", policy => policy.RequireRole("SuperAdmin", "CIO"));
            x.AddPolicy("Admins", policy => policy.RequireRole("SuperAdmin", "CIO", "Admin"));
            x.AddPolicy("Users", policy => policy.RequireRole("SuperAdmin", "CIO", "Admin", "User"));
        });
    })
    .Build();


using (var scope = host.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = ["SuperAdmin", "CIO", "Admin", "User"];
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}



host.Run();
