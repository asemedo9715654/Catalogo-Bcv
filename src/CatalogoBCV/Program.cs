using CatalogoBCV.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

using CatalogoBCV.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ApplicationName", "CataloqueBcv")
    .Enrich.WithProperty("Environment", "Desenvolvimento")
    .Enrich.WithProperty("ServerName", Environment.MachineName)
    .Enrich.WithProperty("AppVersion", "1.0.0")
    .WriteTo.Seq("http://bcv_appt2012:5341/")
    .WriteTo.Console());

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewCatalog", policy =>
        policy.RequireClaim("permission", "CanViewCatalog"));
    options.AddPolicy("CanEditCatalog", policy =>
        policy.RequireClaim("permission", "CanEditCatalog"));
    options.AddPolicy("CanCreateCatalog", policy =>
        policy.RequireClaim("permission", "CanCreateCatalog"));
    options.AddPolicy("CanViewAudit", policy =>
        policy.RequireClaim("permission", "CanViewAudit"));
    options.AddPolicy("CanManageUsers", policy =>
        policy.RequireClaim("permission", "CanManageUsers"));
    options.AddPolicy("CanManageRoles", policy =>
        policy.RequireClaim("permission", "CanManageRoles"));
    options.AddPolicy("CanManageSettings", policy =>
        policy.RequireClaim("permission", "CanManageSettings"));
});

builder.Services.AddScoped<IMetadataService, SqlServerMetadataService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddDbContext<CatalogContext>(options =>
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

app.UseAuthentication();
app.UseAuthorization();

app.UseSerilogRequestLogging();

app.Use(async (context, next) =>
{
    var username = context.User?.Identity?.IsAuthenticated == true ? context.User.Identity.Name : "Anonymous";
    using (Serilog.Context.LogContext.PushProperty("User", username))
    {
        await next();
    }
});


app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
