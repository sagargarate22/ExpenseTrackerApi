using ExpenseTrackerApi.Configuration;
using ExpenseTrackerApi.Entities;
using ExpenseTrackerApi.Services.BlobStorage;
using ExpenseTrackerApi.Services.Email;
using ExpenseTrackerApi.Services.Email.Interface;
using ExpenseTrackerApi.Services.JWTAuthentication;
using ExpenseTrackerApi.Services.PasswordEncrypter;
using ExpenseTrackerApi.Services.Repository;
using ExpenseTrackerApi.Services.Repository.IRepository;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
}).AddRazorRuntimeCompilation();
builder.Services.AddRazorPages();
// adding custom services
builder.Services.AddScoped(typeof(ICommonService<>), typeof(CommonRepo<>));
builder.Services.AddScoped<INonReversiblePasswordHasher, NonReversiblePasswordHasher>();
builder.Services.AddScoped<IUserServices, UserRepository>();
builder.Services.AddScoped<IRoleServices, RoleRepo>();
builder.Services.AddScoped<IEmailServices, EmailServices>();
builder.Services.AddScoped<IExpenseService, ExpensesRepo>();
builder.Services.AddScoped<ITokenManager, TokenManager>();
builder.Services.AddScoped<IEmailServices, EmailServices>();
builder.Services.AddScoped<IEmailServiceProvider, EmailServiceProvider>();
builder.Services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<IUserRoleMappingServices, UserRoleMappingRepo>();
builder.Services.AddScoped<ICache, MemoryCacheStorage>();
builder.Services.AddAutoMapper(typeof(AutoMapperConfig));
builder.Services.Configure<BlobStorageOptions>(builder.Configuration.GetSection("BlobStorage"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("TestCors", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// jwt authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!)),
        ClockSkew = TimeSpan.Zero,
    };
});
var jwt = new JwtSettings();
builder.Configuration.Bind("JwtSettings", jwt);
builder.Services.AddSingleton(jwt);


builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DatabaseConnectionString")));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHangfire(opt =>
{
    opt.UseSqlServerStorage(builder.Configuration.GetConnectionString("DatabaseConnectionString"))
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings();
});

builder.Services.AddHangfireServer();

builder.Services.AddSwaggerGen(options =>
{
 
    // Adding swagger authorization ui 
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Description = "jwt Authorization header using bearer scheme enter the bearer [space] add token in the text input",
        Name = "Authorization",
        In = ParameterLocation.Header
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme()
            {
                Reference = new OpenApiReference()
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme,
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<String>()
        },
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseRouting();
app.UseCors("TestCors");
app.UseHttpsRedirection();
app.UseHangfireDashboard();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    //Authorization = new[]
    //{
    //    new HangfireCustomBasicAuthenticationFilter
    //    {
    //        User = app.Configuration.GetSection("HangfireSettings:Username").Value,
    //        Pass = app.Configuration.GetSection("HangfireSettings:Password").Value
    //    }
    //}
});

//RecurringJob.AddOrUpdate(Guid.NewGuid().ToString(),
//    x => x.Execute(), Cron.Hourly);

//RecurringJob.AddOrUpdate(
//           "SendDailyEmails",
//           () => Console.WriteLine("Daily Email Job Executed"),
//           Cron.Minutely);

//RecurringJob.AddOrUpdate(() => Console.WriteLine("Hello from Hangfire!"), Cron.Minutely);
TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
RecurringJob.AddOrUpdate<IEmailServiceProvider>("send-daily-expense-report", x => x.SendDailyReport(), Cron.Daily(21, 0), timeZone: timeZoneInfo);
RecurringJob.AddOrUpdate<IEmailServiceProvider>("send-monthly-expense-report", x => x.SendMonthlyReport(), Cron.Monthly(1, 10), timeZone: timeZoneInfo);


app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
