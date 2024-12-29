using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using BE_AuctionAOT.Common.Utility;
using BE_AuctionAOT.Controllers.Personal_Account_Management;
using BE_AuctionAOT.DAO.AccountManagement.Account;
using BE_AuctionAOT.DAO.AccountManagement.Account_Details;
using BE_AuctionAOT.DAO.AuctionForCustomer.JoinTheAuction;
using BE_AuctionAOT.DAO.AuctionManagement.Auction;
using BE_AuctionAOT.DAO.AuctionManagement.AuctionImage;
using BE_AuctionAOT.DAO.AuctionManagement.AuctionInvitation;
using BE_AuctionAOT.DAO.AuctionManagement.AuctionRequest;
using BE_AuctionAOT.DAO.AuctionManagement.Dispute;
using BE_AuctionAOT.DAO.AuctionManagement.EvidenceFile;
using BE_AuctionAOT.DAO.AuctionManagement.ListAuction;
using BE_AuctionAOT.DAO.AuctionManagement.Payment;
using BE_AuctionAOT.DAO.AuctionReviewDao;
using BE_AuctionAOT.DAO.CategoryManagement;
using BE_AuctionAOT.DAO.Chats;
using BE_AuctionAOT.DAO.Common.Category;
using BE_AuctionAOT.DAO.Common.Notifications;
using BE_AuctionAOT.DAO.Common.SystemConfiguration;
using BE_AuctionAOT.DAO.Common.SystemMessages;
using BE_AuctionAOT.DAO.Common.User;
using BE_AuctionAOT.DAO.Personal_Account_Management.Personal_Account;
using BE_AuctionAOT.DAO.PostManagement.Post;
using BE_AuctionAOT.DAO.Search;
using BE_AuctionAOT.Models;
using BE_AuctionAOT.RabbitMQ.BidQueue.Publishers;
using BE_AuctionAOT.RabbitMQ.BidQueue.Services;
using BE_AuctionAOT.Realtime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/auctionAOT.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// Add services to the container.

//builder.Services.AddControllers(options =>
//{
//    options.ReturnHttpNotAcceptable = true;
//}).AddNewtonsoftJson()
//.AddXmlDataContractSerializerFormatters();

builder.Services.AddDbContext<DB_AuctionAOTContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Mapping configuration
builder.Services.AddAutoMapper(typeof(PersonalConversion), typeof(DisputeConversion), typeof(PaymentConversion), typeof(SearchProfile), typeof(AuctionReviewProfile));

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt =>
{
    // For development only
    opt.RequireHttpsMetadata = false;
    opt.SaveToken = true;
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Authentication:SecretForKey"])),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Authentication:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Authentication:Audience"],
    };
});

builder.Services.AddHealthChecks();
builder.Services.AddSwaggerGen(setupAction =>
{
    setupAction.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AuctionAOT",
        Version = "v1"
    });
    setupAction.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer jhfdkj.jkdsakjdsa.jkdsajk\"",
    });
    setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }, new string[] {}
        }
    });
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("corsapp", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});


var keyVaultUrl = builder.Configuration["AzureKeyVault:VaultUrl"];
builder.Services.AddSingleton(new SecretClient(new Uri(keyVaultUrl), new ClientSecretCredential(
	builder.Configuration["AzureKeyVault:TenantId"],
	builder.Configuration["AzureKeyVault:ClientId"],
	builder.Configuration["AzureKeyVault:ClientSecret"]
)));

// Tăng giới hạn kích thước Request Body
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100MB
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
});

// builder.Services.AddControllers(options =>
// {
//     options.MaxModelBindingCollectionSize = 1024;
// }).AddJsonOptions(options =>
// {
//     options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
// });

// Tăng thời gian xử lý request
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(2);
});

builder.Services.AddMemoryCache();

builder.Services.AddScoped<UserDao>();
builder.Services.AddScoped<AccountDao>();
builder.Services.AddScoped<AuctionDao>();
builder.Services.AddScoped<PersonalDao>();
builder.Services.AddScoped<PaymentDao>();
builder.Services.AddScoped<EvidenceFileDao>();
builder.Services.AddScoped<DisputeDao>();
builder.Services.AddScoped<ListAuctionDao>();
builder.Services.AddScoped<PostDao>();
builder.Services.AddScoped<CategoryDao>();
builder.Services.AddScoped<AccountDetailDao>();
builder.Services.AddScoped<AuctionImageDao>();
builder.Services.AddScoped<AuctionRequestDao>();
builder.Services.AddScoped<AuctionInvitationDao>();
builder.Services.AddScoped<SystemConfigurationDao>();
builder.Services.AddScoped<NotificationsDao>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<CategoryManagementDao>();
builder.Services.AddScoped<SearchDao>();
builder.Services.AddScoped<AuctionReviewDao>();

builder.Services.AddScoped<Mail>();
builder.Services.AddScoped<Number>();
builder.Services.AddScoped<AuthUtility>();
builder.Services.AddScoped<ImageEncryptionService>();
builder.Services.AddScoped<Ekyc>();

builder.Services.AddSingleton<FileExtensionContentTypeProvider>();

//Service - Dai
builder.Services.AddSignalR();

builder.Services.AddHostedService<AuctionBidService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IBidPublisher, BidPublisher>();
builder.Services.AddScoped<JoinTheAuctionDao>();
builder.Services.AddScoped<ChatDao>();


builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddApiVersioning(setupAction =>
{
    setupAction.AssumeDefaultVersionWhenUnspecified = true;
    setupAction.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    setupAction.ReportApiVersions = true;
});

builder.Services.AddControllers();

builder.Services.AddSingleton(_ => {
    string connectionString = builder.Configuration["ConnectionStrings:Store"];
    return new BlobServiceClient(connectionString);
});

// Cấu hình dịch vụ JsonSerializerOptions
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    options.JsonSerializerOptions.MaxDepth = 64; // Tùy chọn này có thể thay đổi nếu cần
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("corsapp");

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapHub<BidRealtimeHub>("/bidRealtimeHub");
app.MapHub<NotifyHub>("/notifyHub");
app.MapHub<CommentHub>("/commentHub");
app.MapHub<ChatHub>("/chatHub");

app.MapControllers();

app.Run();
