using MassIntentionSystem.Data;
using MassIntentionSystem.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Connection String Setup
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure())); // Resiliency para sa transient LocalDB connection hiccups

// 2. Add Session Services (Pang-fix sa Session Error)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 3. Add MVC Services
builder.Services.AddControllersWithViews();

// 4. Dependency Injection para sa Services
builder.Services.AddScoped<DocumentService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<NotificationService>();

var app = builder.Build();

// Auto-create / migrate the database on startup so the app is self-healing
// kahit wala pang MassIntentionDb sa LocalDB (dating cause ng startup crash).
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Nabigo ang database migration sa startup.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 5. Enable Session Middleware (Dapat bago mag UseAuthorization)
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();