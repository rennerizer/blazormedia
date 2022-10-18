using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using BlazorMedia.Client;
using BlazorMedia.Client.Data;
using BlazorMedia.Client.Features.Reviewers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var backendOrigin = builder.Configuration["BackendOrigin"]!;

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// gRPC-Web client with auth
builder.Services.AddReviewerDataClient((services, options) =>
{
    options.BaseUri = backendOrigin;
});

// Sets up EF Core with Sqlite
builder.Services.AddReviewerDataDbContext();

// Declare a custom element for the Reviewers
builder.RootComponents.RegisterAsCustomElement<ReviewersPage>("reviewers-grid");

await builder.Build().RunAsync();
