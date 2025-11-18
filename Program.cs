using Homework_portal.Data;
    using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// services ...
var app = builder.Build();

// Apply pending migrations (development only)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}

app.Run();