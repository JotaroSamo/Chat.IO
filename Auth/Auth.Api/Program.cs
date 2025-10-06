using Auth.Api.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor();

builder.Services
    .AddAuthDatabase(builder.Configuration)
    .AddAuthIdentity()
    .AddJwtAuthentication(builder.Configuration)
    .AddAuthLogic();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

