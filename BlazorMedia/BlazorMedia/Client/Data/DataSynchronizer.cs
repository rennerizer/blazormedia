using System.Data.Common;
using System.Runtime.InteropServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

using BlazorMedia.Client;

namespace BlazorMedia.Client.Data
{
    public class DataSynchronizer
    {
        public const string SqliteDbFilename = "app.db";
        private readonly Task firstTimeSetupTask;
        private readonly IDbContextFactory<ClientSideDbContext> dbContextFactory;
        private readonly ReviewerData.ReviewerDataClient reviewerData;
        private bool isSynchronizing;

        public DataSynchronizer(IJSRuntime js, IDbContextFactory<ClientSideDbContext> dbContextFactory, ReviewerData.ReviewerDataClient reviewerData)
        {
            this.dbContextFactory = dbContextFactory;
            this.reviewerData = reviewerData;
            firstTimeSetupTask = FirstTimeSetupAsync(js);
        }

        public int SyncCompleted { get; private set; }
        public int SyncTotal { get; private set; }

        public async Task<ClientSideDbContext> GetPreparedDbContextAsync()
        {
            await firstTimeSetupTask;
            return await dbContextFactory.CreateDbContextAsync();
        }

        public void SynchronizeInBackground()
        {
            _ = EnsureSynchronizingAsync();
        }

        public event Action? OnUpdate;
        public event Action<Exception>? OnError;

        private async Task FirstTimeSetupAsync(IJSRuntime js)
        {
            var module = await js.InvokeAsync<IJSObjectReference>("import", "./dbstorage.js");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("browser")))
            {
                await module.InvokeVoidAsync("synchronizeFileWithIndexedDb", SqliteDbFilename);
            }

            using var db = await dbContextFactory.CreateDbContextAsync();
            await db.Database.EnsureCreatedAsync();
        }

        private async Task EnsureSynchronizingAsync()
        {
            // Don't run multiple syncs in parallel. This simple logic is adequate because of single-threadedness.
            if (isSynchronizing)
            {
                return;
            }

            try
            {
                isSynchronizing = true;
                SyncCompleted = 0;
                SyncTotal = 0;

                // Get a DB context
                using var db = await GetPreparedDbContextAsync();
                db.ChangeTracker.AutoDetectChangesEnabled = false;
                db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                // Begin fetching any updates to the dataset from the backend server
                var mostRecentUpdate = db.Reviewers.OrderByDescending(p => p.ModifiedTicks).FirstOrDefault()?.ModifiedTicks;

                var connection = db.Database.GetDbConnection();
                connection.Open();

                while (true)
                {
                    var request = new ReviewersRequest { MaxCount = 5000, ModifiedSinceTicks = mostRecentUpdate ?? -1 };
                    var response = await reviewerData.GetReviewersAsync(request);
                    var syncRemaining = response.ModifiedCount - response.Reviewers.Count;
                    SyncCompleted += response.Reviewers.Count;
                    SyncTotal = SyncCompleted + syncRemaining;

                    if (response.Reviewers.Count == 0)
                    {
                        break;
                    }
                    else
                    {
                        mostRecentUpdate = response.Reviewers.Last().ModifiedTicks;
                        BulkInsert(connection, response.Reviewers);
                        OnUpdate?.Invoke();
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: use logger also
                OnError?.Invoke(ex);
            }
            finally
            {
                isSynchronizing = false;
            }
        }

        private void BulkInsert(DbConnection connection, IEnumerable<Reviewer> reviewers)
        {
            // Since we're inserting so much data, we can save a huge amount of time by dropping down below EF Core and
            // using the fastest bulk insertion technique for Sqlite.
            using (var transaction = connection.BeginTransaction())
            {
                var command = connection.CreateCommand();
                var reviewerId = AddNamedParameter(command, "$ReviewerId");
                var platform = AddNamedParameter(command, "$Platform");
                var genre = AddNamedParameter(command, "$Genre");
                var name = AddNamedParameter(command, "$Name");
                var location = AddNamedParameter(command, "$Location");
                var reviews = AddNamedParameter(command, "$Reviews");
                var revenue = AddNamedParameter(command, "$Revenue");
                var modifiedTicks = AddNamedParameter(command, "$ModifiedTicks");

                command.CommandText =
                    $"INSERT OR REPLACE INTO Reviewers (ReviewerId, Platform, Genre, Name, Location, Reviews, Revenue, ModifiedTicks) " +
                    $"VALUES ({reviewerId.ParameterName}, {platform.ParameterName}, {genre.ParameterName}, {name.ParameterName}, {location.ParameterName}, {reviews.ParameterName}, {revenue.ParameterName}, {modifiedTicks.ParameterName})";

                foreach (var reviewer in reviewers)
                {
                    reviewerId.Value = reviewer.ReviewerId;
                    platform.Value = reviewer.Platform;
                    genre.Value = reviewer.Genre;
                    name.Value = reviewer.Name;
                    location.Value = reviewer.Location;
                    reviews.Value = reviewer.Reviews;
                    revenue.Value = reviewer.Revenue;
                    modifiedTicks.Value = reviewer.ModifiedTicks;
                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }

            static DbParameter AddNamedParameter(DbCommand command, string name)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = name;
                command.Parameters.Add(parameter);
                return parameter;
            }
        }
    }
}
