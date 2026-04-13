using Microsoft.EntityFrameworkCore;
using PatientPortalApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Razor Pages
builder.Services.AddRazorPages();

// Entity Framework Core + SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

// Serve static files and Razor Pages
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

// Redirect root to Login
app.MapGet("/", () => Results.Redirect("/Login"));

app.Run();