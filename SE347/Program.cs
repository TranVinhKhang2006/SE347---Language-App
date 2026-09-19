var builder = WebApplication.CreateBuilder(args);

// 1. Kích hoạt tính năng đọc và render file .cshtml (SSR)
builder.Services.AddRazorPages();

var app = builder.Build();

// 2. Cho phép Server truy cập và chạy các file tĩnh như CSS, JS, hình ảnh
app.UseStaticFiles();

app.UseRouting();

// 3. Tự động ánh xạ đường dẫn URL tới các file trong thư mục Pages
app.MapRazorPages();

app.MapGet("/", async context =>
{
    context.Response.Redirect("/Index.cshtml");
    await Task.CompletedTask;
});

// Kích hoạt Server chạy
app.Run();
