using EquipmentBorrowingManagementSystem.Web.Options;
using EquipmentBorrowingManagementSystem.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApiOptions>(options =>
{
    options.BaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5171";
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "ebms.web.session";
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpClient<EbmsApiClient>();
builder.Services.AddScoped<AuthSessionService>();
builder.Services.AddScoped<BorrowCartService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");
app.UseSession();
app.UseAuthorization();

app.MapRazorPages();
app.MapGet("/", () => Results.Redirect("/Categories"));

app.Run();
