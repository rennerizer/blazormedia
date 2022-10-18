using Microsoft.EntityFrameworkCore;

using Grpc.Net.Client.Web;
using Grpc.Net.Client;

using BlazorMedia.Client;

namespace BlazorMedia.Client.Data
{
    public static class ReviewerDataServices
    {
        public static void AddReviewerDataClient(this IServiceCollection serviceCollection, Action<IServiceProvider, ReviewerDataClientOptions> configure)
        {
            serviceCollection.AddScoped(services =>
            {
                //var options = new ReviewerDataClientOptions();
                //configure(services, options);
                //var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb));
                //var channel = GrpcChannel.ForAddress(options.BaseUri!, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = null });
                //return new ReviewerData.ReviewerDataClient(channel);

                var channel = GrpcChannel.ForAddress("https://localhost:7170/", new GrpcChannelOptions
                {
                    HttpHandler = new GrpcWebHandler(new HttpClientHandler())
                });

                return new ReviewerData.ReviewerDataClient(channel);
            });
        }

        public static void AddReviewerDataDbContext(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddDbContextFactory<ClientSideDbContext>(
                options => options.UseSqlite($"Filename={DataSynchronizer.SqliteDbFilename}"));
            serviceCollection.AddScoped<DataSynchronizer>();
        }
    }
}

public class ReviewerDataClientOptions
{
    public string? BaseUri { get; set; }
}
