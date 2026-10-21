using Microsoft.EntityFrameworkCore;
using ProcurementApp.Data;
using ProcurementApp.Services;
using ProcurementApp.Strategies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// --- Database (EF Core + SQLite) ---
builder.Services.AddDbContext<ProcurementDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=procurement.db"));

// --- Shared foundation: repositories (Repository pattern) ---
// Every module depends on these interfaces, never on EF directly.
builder.Services.AddScoped<IPurchaseOrderRepository, EfPurchaseOrderRepository>();
builder.Services.AddScoped<IReferenceDataRepository, EfReferenceDataRepository>();
builder.Services.AddScoped<IDeliveryRepository, EfDeliveryRepository>();

// Unique sequential PO reference — single shared counter (Singleton). Used by Module 1.
builder.Services.AddSingleton<IReferenceNumberGenerator, SequentialReferenceGenerator>();

// --- Module 4: Notifications (Observer pattern) ---
// The hub is the Subject; observers below subscribe to it. Other modules just call Notify().
builder.Services.AddSingleton<InAppInboxObserver>();              // keeps an in-app inbox
builder.Services.AddSingleton<INotificationService>(sp =>
{
    var hub = new NotificationHub(sp.GetRequiredService<ILogger<NotificationHub>>());
    hub.Subscribe(sp.GetRequiredService<InAppInboxObserver>());   // show notifications in the UI
    hub.Subscribe(new ConsoleLogObserver());                      // also log them
    return hub;
});

// --- Module 2: Approval (Strategy + Factory) ---
// The active approval policy is chosen from configuration by the factory, then injected
// everywhere as IApprovalPolicy. Change "Approval:Policy" in appsettings.json to swap the rule.
builder.Services.AddSingleton<IApprovalPolicy>(_ =>
{
    var policyName = builder.Configuration["Approval:Policy"] ?? "Threshold";
    var threshold = builder.Configuration.GetValue<decimal?>("Approval:ThresholdLkr") ?? 100000m;
    return ApprovalPolicyFactory.Create(policyName, threshold);
});

// --- Module services ---
builder.Services.AddScoped<IApprovalService, ApprovalService>();        // Module 2
builder.Services.AddScoped<IOrderCreationService, OrderCreationService>(); // Module 1
builder.Services.AddScoped<IDeliveryService, DeliveryService>();        // Module 3
builder.Services.AddScoped<IEnquiryService, EnquiryService>();          // Module 4

var app = builder.Build();

// Create and seed the SQLite database on startup.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProcurementDbContext>();
    DbSeeder.Initialise(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();      // serves wwwroot (bootstrap, css, js)
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
