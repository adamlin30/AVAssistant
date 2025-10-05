using System.Diagnostics;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Static frontend
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// Asynchronously launch browser
async Task OpenBrowserAsync(string url)
{
    await Task.Delay(1000); // Wait for the WebServer to start
    Process.Start(new ProcessStartInfo
    {
        FileName = url,
        UseShellExecute = true
    });
}

// Call and wait
await OpenBrowserAsync("http://localhost:5000");

// Start Web API
app.Run("http://localhost:5000");
