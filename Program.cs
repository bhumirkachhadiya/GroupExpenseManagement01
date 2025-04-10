//using GroupExpenseManagement01.Helpers;
using GroupExpenseManagement01.Services;
using GroupExpenseManagement01;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IEmailSender, EmailSender>();
// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();


// Bind EncryptionSettings from appsettings.json
builder.Services.Configure<EncryptionSettings>(builder.Configuration.GetSection("EncryptionSettings"));

// Register EncryptionService as a service
builder.Services.AddSingleton<IEncryptionService>(provider =>
{
    var encryptionSettings = provider.GetRequiredService<IOptions<EncryptionSettings>>().Value;
    return new EncryptionService(encryptionSettings.Key); // Inject the key from appsettings.json
});

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

// Enable session middleware before authentication and authorization
app.UseSession();

/*app.UseAuthentication();*/
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
