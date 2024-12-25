using SpotifyApplicationLayer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ISpotifyService, SpotifyService>();


builder.Services.AddDistributedMemoryCache(); // Bellek tabanl� oturum �nbelle�i
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Oturum s�resi
    options.Cookie.HttpOnly = true; // �erezlerin sadece HTTP �zerinden eri�ilmesini sa�lar
    options.Cookie.IsEssential = true; // �erezlerin gerekli olmas�n� sa�lar
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
