using Collection.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => { options.LoginPath = "/login"; });

builder.Services.AddDbContext<UsersDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("CourseProject")));
builder.Services.AddDbContext<CollectionsDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("CourseProject")));
builder.Services.AddDbContext<ItemsDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("CourseProject")));
builder.Services.AddDbContext<LikesDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("CourseProject")));
builder.Services.AddDbContext<CommentsDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("CourseProject")));
builder.Services.AddDbContext<TagsDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("CourseProject")));
builder.Services.AddDbContext<CollectionsPropDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("CourseProject")));
builder.Services.AddDbContext<ItemsPropDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("CourseProject")));

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
