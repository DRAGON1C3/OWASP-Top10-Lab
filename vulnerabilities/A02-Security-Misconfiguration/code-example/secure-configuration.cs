// A02:2025 - Security Misconfiguration
// Защищённая конфигурация ASP.NET Core (учебный пример, не код Juice Shop).
//
// Логика приложения та же. Отличие - в составе сборки и в условиях,
// при которых включается каждая возможность.

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

if (builder.Environment.IsDevelopment())
{
    // Метрики и подробная диагностика нужны только разработчикам.
    builder.Services.AddMetrics();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // Наружу - только код ошибки и correlation id, без деталей.
    app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "internal_error",
            traceId = context.TraceIdentifier
        });
    }));
    app.UseHsts();
}

// Служебные эндпоинты: только для доверенного контура и только с авторизацией.
if (app.Environment.IsDevelopment())
{
    app.MapMetrics().RequireAuthorization("ops");
}

// Статика ограничена каталогом публичных ассетов.
// Служебные файлы в артефакт сборки не попадают - это задача CI.
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot")),
    RequestPath = ""
});

// Устаревший интерфейс не скрыт, а удалён из сборки.
// Если он ещё нужен - отдельный сервис, отдельный контур, отдельный жизненный цикл.

// CORS: только известные источники; AllowCredentials() несовместим с AllowAnyOrigin().
app.UseCors(policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod());

app.MapControllers();
app.Run();
