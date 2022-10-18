using Microsoft.EntityFrameworkCore;

using Grpc.Core;

using BlazorMedia.Server.Data;

namespace BlazorMedia.Server
{
    public class ReviewersService : ReviewerData.ReviewerDataBase
    {
        private readonly ApplicationDbContext db;

        public ReviewersService(ApplicationDbContext db)
        {
            this.db = db;
        }

        public override async Task<ReviewersReply> GetReviewers(ReviewersRequest request, ServerCallContext context)
        {
            //Temp
            //request.MaxCount = 10;

            var modifiedReviewers = db.Reviewers
                .OrderBy(p => p.ModifiedTicks)
                .Where(p => p.ModifiedTicks > request.ModifiedSinceTicks);

            var reply = new ReviewersReply();
            reply.ModifiedCount = await modifiedReviewers.CountAsync();
            reply.Reviewers.AddRange(await modifiedReviewers.Take(request.MaxCount).ToListAsync());

            return reply;
        }
    }
}
