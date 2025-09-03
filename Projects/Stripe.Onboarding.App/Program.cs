
using Azure.Identity;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

//var appSecretsId = "5334ac05-3583-4823-9d44-97410596f81b";
builder.Configuration.AddUserSecrets<Program>();
if (builder.Environment.IsProduction())
{
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
        new DefaultAzureCredential());
}
// enable CORS
var stripeCheckout = "stripeCheckout";
var localFrontendDevServer = "localFrontendDevServer";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: localFrontendDevServer,
        policy =>
        {
            policy.WithOrigins("*", "https://127.0.0.1:5500")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });

    options.AddPolicy(name: stripeCheckout,
        policy =>
        {
            policy.WithOrigins(
                "https://checkout.stripe.com",
                "https://*.stripe.com",
                "https://api.stripe.com",
                "https://maps.googleapis.com",
                "https://js.stripe.com"
                )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });

});

// Add services to the container.
builder.Services.AddRazorPages();
// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Foundations
Stripe.Onboarding.Foundations.Integrations.Stripe.Startup.ConfigureServices(builder.Services, builder.Configuration);
Stripe.Onboarding.Foundations.Products.Startup.ConfigureServices(builder.Services, builder.Configuration);
Stripe.Onboarding.Foundations.Authentication.Startup.ConfigureServices(builder.Services, builder.Configuration);
Stripe.Onboarding.Foundations.Orders.Startup.ConfigureServices(builder.Services, builder.Configuration);

// Features
Stripe.Onboarding.Features.Cart.Startup.ConfigureServices(builder.Services, builder.Configuration);

var isProduction = builder.Environment.IsProduction();

// Add Database Exception filter
// provides helpful error information in the development environment for EF migrations errors.
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// enable MVC
builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
{
});
builder.Services.AddRazorPages();

// enable sessionState
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(120);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();


app.UseSession();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseCors(localFrontendDevServer);
}

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapRazorPages();
app.MapControllers();

app.MapDefaultControllerRoute();


app.Run();


