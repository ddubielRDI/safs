using Microsoft.EntityFrameworkCore;
using Sasquatch.Core.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Compose section controllers via AddApplicationPart
builder.Services.AddControllersWithViews()
    .AddApplicationPart(typeof(Sasquatch.Collection.CollectionMarker).Assembly)
    .AddApplicationPart(typeof(Sasquatch.Calculation.CalculationMarker).Assembly)
    .AddApplicationPart(typeof(Sasquatch.Reporting.ReportingMarker).Assembly);

// Configure Entity Framework with SQL Server
builder.Services.AddDbContext<SasquatchDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Area routes must be registered before the default route
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
