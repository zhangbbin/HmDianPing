using HmDianPing.Components;
using HmDianPing.Web.Data;
using HmDianPing.Web.Security;
using HmDianPing.Web.Services;
using HmDianPing.Web.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;
using System.Text;

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
    builder.Services.AddControllers();
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    var jwtKey = builder.Configuration["Jwt:Key"] ?? "HmDianPing-Super-Secret-Key-For-Dev-Only-Please-Change";
    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "HmDianPing";
    var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "HmDianPing.Client";

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(PolicyNames.CanManageShops, policy =>
            policy.RequireRole(RoleConstants.Merchant, RoleConstants.Admin, RoleConstants.SuperAdmin));

        options.AddPolicy(PolicyNames.CanEditShopResource, policy =>
            policy.Requirements.Add(new CanEditShopRequirement()));
    });

    // 注册 Service
    builder.Services.AddScoped<ShopService>();
    builder.Services.AddScoped<UserService>();
    builder.Services.AddScoped<JwtTokenService>();
    builder.Services.AddScoped<RedisIdWorker>();
    builder.Services.AddScoped<VoucherOrderService>();
    builder.Services.AddScoped<IAuthorizationHandler, ShopEditAuthorizationHandler>();
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

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseAntiforgery();

    app.MapStaticAssets();
    app.MapControllers();
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

