using System.Text.Json.Serialization;
using Backend.Extensions;
using Backend.Handlers;
using Backend.Hubs;
using Backend.Middlewares;
using Backend.Models.Configurations;
using Serilog;

const string corsPolicy = "CrewQuizPolicy";
var builder = WebApplication.CreateBuilder(args);

// Configure Serilog early in the pipeline
builder.Services.ConfigureSerilog(builder.Configuration);
builder.Host.UseSerilog();
builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithAuth();
builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddHttpContextAccessor();
builder.Services.AddNpgsqlDbContext(builder.Configuration.GetConnectionString("CrewQuiz"));
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddRepositories();
builder.Services.AddServices();
builder.Services.AddSignalR();
builder.Services.ConfigureCors(builder.Configuration, corsPolicy);
builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRequestLogging();
app.UseMiddleware<DatabaseTransactionMiddleware>();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors(corsPolicy);
app.UseMiddleware<TokenExpirationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<GameHub>("/crew-quiz");
app.MapControllers();
app.Run();