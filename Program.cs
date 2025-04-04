using library_management.Models;
using library_management.repository.classes;
using library_management.repository.internalinterface;
using Microsoft.EntityFrameworkCore;
using library_management.Repositories.Classes;
using library_management.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<dbConnect>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DbContext")));
builder.Services.AddScoped<libraryInterface , librarymainClass>();
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddSingleton<EmailSenderInterface, EmailSenderClass>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<loginInterface, loginClass>();
builder.Services.AddScoped<ISidebarRepository , SidebarRepository>();
builder.Services.AddScoped<MemberMasterInterface , MemberMasterClass>();
builder.Services.AddScoped<AdminInterface, AdminMasterClass>();
builder.Services.AddScoped<MembershipInterface, MembershipClass>();
builder.Services.AddScoped<BookServiceInterface, BookServiceClass>();
builder.Services.AddScoped<BorrowInterface, BorrowClass>();
builder.Services.AddScoped<FineInterface,FineClass>();
builder.Services.AddScoped<ReportsInterface,ReportsClasses>();
builder.Services.AddScoped<PermisionHelperInterface,PermisionHelperClass>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true; // Ensures session cookie is accessible only via HTTP
    options.Cookie.IsEssential = true; // Ensures cookie is essential
});

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
    pattern: "{controller=library}/{action=login}/{id?}");
app.Run();
