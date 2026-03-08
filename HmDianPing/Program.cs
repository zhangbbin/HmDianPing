using HmDianPing.Components;
using HmDianPing.Web.Data;
using HmDianPing.Web.Services;
using HmDianPing.Web.Utils;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information) // 过滤掉微软产生的大量废话日志
    .Enrich.FromLogContext()
    .WriteTo.Console() // 输出到控制台
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day) // 每天生成一个新文件
    .CreateLogger();

try
{
    Log.Information("正在启动黑马点评 Web 应用...");
    var builder = WebApplication.CreateBuilder(args);

    // 注册 Redis 连接复用器
    var redisConnection = builder.Configuration.GetConnectionString("Redis");
    builder.Services.AddSingleton<IConnectionMultiplexer>(
        ConnectionMultiplexer.Connect(redisConnection)
    );

    // 注册 DbContext
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<HmDbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    // 注册 Service
    builder.Services.AddScoped<ShopService>();
    builder.Services.AddScoped<UserService>();
    builder.Services.AddScoped<RedisIdWorker>();
    builder.Services.AddScoped<VoucherOrderService>();
    builder.Services.AddCascadingAuthenticationState();
    builder.Services.AddScoped<AuthenticationStateProvider, HmAuthStateProvider>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }
    app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
    app.UseHttpsRedirection();

    app.UseAntiforgery();

    app.MapStaticAssets();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException && ex.Source != "Microsoft.EntityFrameworkCore.Design")
{
    Log.Fatal(ex, "应用启动失败，发生未处理的异常！");
}
finally
{
    Log.CloseAndFlush();
}

