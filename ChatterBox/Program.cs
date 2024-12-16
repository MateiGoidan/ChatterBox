using ChatterBox.Data;
using ChatterBox.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        SeeData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(name: "HomePage", pattern: "", defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(name: "UsersList", pattern: "Users/List", defaults: new { controller = "Users", action = "List" });
app.MapControllerRoute(name: "UsersProfile", pattern: "Users/Show/{id}", defaults: new { controller = "Users", action = "Show" });
app.MapControllerRoute(name: "UsersPromote", pattern: "Users/Promote/{_ID}", defaults: new { controller = "Users", action = "Promote" });
app.MapControllerRoute(name: "UsersDemote", pattern: "Users/Demote/{_ID}", defaults: new { controller = "Users", action = "Demote" });

app.MapRazorPages();

app.Run();
