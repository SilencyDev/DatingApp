var builder = WebApplication.CreateBuilder(args);

// Add services in the container
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy =>
	policy.AllowAnyHeader()
    .AllowAnyMethod()
	.AllowCredentials()
	.WithOrigins("https://localhost:4200"));
});
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPIv5", Version = "v1" });
});

// Configure https requests pipeline

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/message");
app.MapFallbackToController("Index", "Fallback");


using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try {
    var context = services.GetRequiredService<DataContext>();
	var userManager = services.GetRequiredService<UserManager<AppUser>>();
	var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
    await context.Database.MigrateAsync();
    await Seed.SeedUsers(userManager, roleManager);
} catch (Exception e) {
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(e, "An error occured during migration of fixtures");
}

await app.RunAsync();
