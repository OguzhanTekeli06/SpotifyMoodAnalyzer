using SpotifyApplicationLayer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ISpotifyService, SpotifyService>();


builder.Services.AddDistributedMemoryCache(); // Bellek tabanlý oturum önbelleði
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Oturum süresi
    options.Cookie.HttpOnly = true; // Çerezlerin sadece HTTP üzerinden eriþilmesini saðlar
    options.Cookie.IsEssential = true; // Çerezlerin gerekli olmasýný saðlar
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

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Spotify}/{action=Login2}/{id?}");

app.Run();
