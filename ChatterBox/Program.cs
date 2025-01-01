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

app.MapControllerRoute(name: "UsersList", pattern: "Users/List/{_Id?}", defaults: new { controller = "Users", action = "List" });
app.MapControllerRoute(name: "UsersProfile", pattern: "Users/Show/{_Id}", defaults: new { controller = "Users", action = "Show" });
app.MapControllerRoute(name: "UsersPromote", pattern: "Users/Promote/{_Id}", defaults: new { controller = "Users", action = "Promote" });
app.MapControllerRoute(name: "UsersDemote", pattern: "Users/Demote/{_Id}", defaults: new { controller = "Users", action = "Demote" });

app.MapControllerRoute(name: "CategoriesList", pattern: "Categories/List", defaults: new { controller = "Categories", action = "List" });
app.MapControllerRoute(name: "CategoriesNew", pattern: "Categories/New", defaults: new { controller = "Categories", action = "New" });
app.MapControllerRoute(name: "CategoriesEdit", pattern: "Categories/Edit/{_Id}", defaults: new { controller = "Categories", action = "Edit" });
app.MapControllerRoute(name: "CategoriesDelete", pattern: "Categories/Delete/{_Id}", defaults: new { controller = "Categories", action = "Delete" });

app.MapControllerRoute(name: "ChannelsList", pattern: "Channels/List", defaults: new { controller = "Channels", action = "List" });
app.MapControllerRoute(name: "ChannelsShow", pattern: "Channels/Show/{_Id}", defaults: new { controller = "Channels", action = "Show" });
app.MapControllerRoute(name: "ChannelsDisplay", pattern: "Channels/Info/{_Id}", defaults: new { controller = "Channels", action = "Display" });
app.MapControllerRoute(name: "ChannelsNew", pattern: "Channels/New", defaults: new { controller = "Channels", action = "New" });

app.MapControllerRoute(name: "MessagesNew", pattern: "Messages/New", defaults: new { controller = "Messages", action = "New" });
app.MapControllerRoute(name: "MessagesEdit", pattern: "Messages/Edit/{_Id}", defaults: new { controller = "Messages", action = "Edit" });
app.MapControllerRoute(name: "MessagesDelete", pattern: "Messages/Delete/{_Id}", defaults: new { controller = "Messages", action = "Delete" });

app.MapRazorPages();

app.Run();
