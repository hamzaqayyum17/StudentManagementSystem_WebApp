var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<StudentManagementSystem.Services.GoogleSheetsService>();
builder.Services.AddScoped<StudentManagementSystem.Services.EmailService>();
builder.Services.AddControllersWithViews();

builder.Services.AddSession();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ ADD THIS (must be BEFORE MapControllerRoute)
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Student}/{action=SignIn}/{id?}"); 
app.Run();