// A02:2025 - Security Misconfiguration
// Уязвимая конфигурация ASP.NET Core (учебный пример, не код Juice Shop).
//
// Дефект не в бизнес-логике: обработчики написаны корректно.
// Дефект в том, ЧТО попало в продуктовую сборку и КАК оно сконфигурировано.

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMetrics(); // метрики включены безусловно

var app = builder.Build();

// 1. Диагностика включена всегда, независимо от окружения.
//    В проде это отдаёт наружу стек-трейсы, пути и имена внутренних сборок.
app.UseDeveloperExceptionPage();

// 2. Служебные эндпоинты опубликованы без ограничений и без авторизации.
app.MapMetrics(); // /metrics
app.MapGet("/health/details", () => new
{
    db = "connected",
    version = typeof(Program).Assembly.GetName().Version,
    env = app.Environment.EnvironmentName
});

// 3. Статика раздаётся из каталога, в который при сборке попали
//    инфраструктурные артефакты (IaC, конфиги, скрипты деплоя).
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "deploy")),
    RequestPath = "/deploy"
});

// 4. Устаревший интерфейс оставлен "на всякий случай" и не выключен.
app.MapPost("/api/v1/b2b/import", (HttpRequest request) =>
{
    // legacy-ветка, которая умеет разбирать XML
    return Results.Ok();
});

// 5. CORS открыт полностью, вместе с credentials.
app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials());

app.MapControllers();
app.Run();
