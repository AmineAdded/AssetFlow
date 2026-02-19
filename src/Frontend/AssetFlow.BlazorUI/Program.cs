// ============================================================
// AssetFlow.BlazorUI / Program.cs
// Point d'entrée du frontend Blazor WebAssembly
// ============================================================

using AssetFlow.BlazorUI;
using AssetFlow.BlazorUI.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ==========================================
// URL du backend API
// En hors-ligne, tout tourne localement
// ==========================================
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5164/") // URL de l'API ASP.NET
});

// ==========================================
// LocalStorage pour stocker le token JWT
// ==========================================
builder.Services.AddBlazoredLocalStorage();

// ==========================================
// Enregistrement du service d'authentification
// ==========================================
builder.Services.AddScoped<AuthService>();

await builder.Build().RunAsync();