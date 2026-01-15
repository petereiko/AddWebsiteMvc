using AddWebsiteMvc;
using AddWebsiteMvc.Business;
using AddWebsiteMvc.Business.Configurations;
using AddWebsiteMvc.Business.Entities.Identity;
using AddWebsiteMvc.Business.HttpClientWrapper;
using AddWebsiteMvc.Business.Interfaces;
using AddWebsiteMvc.Business.Persistence;
using AddWebsiteMvc.Business.Services;
using AddWebsiteMvc.Business.Services.Auth;
using AddWebsiteMvc.Business.Services.Election;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http;
using System.Configuration;
using VoteApp.Application.Services.Election;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<VoteDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSignalR();

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    //Configure Identity Options
    options.SignIn.RequireConfirmedAccount = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(1);
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 4;
})
    .AddEntityFrameworkStores<VoteDbContext>()
    .AddUserManager<UserManager<ApplicationUser>>()
    .AddRoleManager<RoleManager<ApplicationRole>>()
    .AddUserStore<UserStore<ApplicationUser, ApplicationRole, VoteDbContext, Guid, IdentityUserClaim<Guid>, ApplicationUserRole, IdentityUserLogin<Guid>, IdentityUserToken<Guid>, IdentityRoleClaim<Guid>>>()
.AddRoleStore<RoleStore<ApplicationRole, VoteDbContext, Guid, ApplicationUserRole, IdentityRoleClaim<Guid>>>()
.AddDefaultTokenProviders();



builder.Services.AddHttpClient();
builder.Services.Configure<HttpClientFactoryOptions>(options =>
{
    options.HttpClientActions.Add(client =>
        client.Timeout = TimeSpan.FromMinutes(5));
});


builder.Services.AddTransient<ICandidateService, CandidateService>();

builder.Services.AddTransient<IElectionService, ElectionService>();

builder.Services.AddTransient<IAuthUser, AuthUser>();

builder.Services.AddTransient<IUserManagementService, UserManagementService>();

builder.Services.AddTransient<IVoteService, VoteService>();

builder.Services.AddTransient<IVotePriceService, VotePriceService>();


builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.AddHttpContextAccessor();

// Configure Cookie Authentication (without Identity)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Account/Login";
        options.LogoutPath = "/Admin/Account/Logout";
        options.AccessDeniedPath = "/Admin/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomClaimsPrincipalFactory>();

builder.Services.AddHttpClient<IHttpClientWrapperService, HttpClientWrapperService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(120);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddAuthentication();

builder.Services.AddAuthorization();

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
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
  );

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");




app.Run();
