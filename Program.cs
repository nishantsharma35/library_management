using library_management.Models;
using library_management.repository.classes;
using library_management.repository.internalinterface;
using Microsoft.EntityFrameworkCore;
using library_management.Repositories.Classes;
using library_management.Controllers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<dbConnect>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DbContext")));
builder.Services.AddScoped<libraryInterface, librarymainClass>();
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddSingleton<EmailSenderInterface, EmailSenderClass>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<loginInterface, loginClass>();
builder.Services.AddScoped<ISidebarRepository, SidebarRepository>();
builder.Services.AddScoped<MemberMasterInterface, MemberMasterClass>();
builder.Services.AddScoped<AdminInterface, AdminMasterClass>();
builder.Services.AddScoped<MembershipInterface, MembershipClass>();
builder.Services.AddScoped<BookServiceInterface, BookServiceClass>();
builder.Services.AddScoped<BorrowInterface, BorrowClass>();
builder.Services.AddScoped<FineInterface, FineClass>();
builder.Services.AddScoped<ReportsInterface, ReportsClasses>();
builder.Services.AddScoped<PermisionHelperInterface, PermisionHelperClass>();
builder.Services.AddScoped<PaymentInterface, PaymentClass>();
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true; // Ensures session cookie is accessible only via HTTP
    options.Cookie.IsEssential = true; // Ensures cookie is essential
});
builder.Services.AddHttpClient();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = "451272492291-evpt01qddi8oqkdnkjfkr6d897c0p2an.apps.googleusercontent.com";
    options.ClientSecret = "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";
    options.CallbackPath = "/signin-google"; // or whatever you’ve set
});
builder.Services.Configure<RazorpaySettingClass>(builder.Configuration.GetSection("Razorpay"));
builder.Services.AddSingleton(resolver =>
    resolver.GetRequiredService<IOptions<RazorpaySettingClass>>().Value);
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
var app = builder.Build();


app.UseSession();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}



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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
