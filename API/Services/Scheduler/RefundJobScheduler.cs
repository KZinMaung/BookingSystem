using API.Services.Booking;
using Hangfire;
using System.Collections.Concurrent;

namespace API.Services.Scheduler
{

    public class RefundJobScheduler
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private static readonly ConcurrentDictionary<int, string> _scheduledJobs = new ConcurrentDictionary<int, string>();

        public RefundJobScheduler(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
        }

        public void ScheduleRefundJob(int classId, DateTime endTime)
        {
            var jobId = $"refund-credits-class-{classId}";

            // Check if a job is already scheduled for this class
            if (_scheduledJobs.TryGetValue(classId, out var existingJobId))
            {
                // Cancel the existing job
                BackgroundJob.Delete(existingJobId);
            }

            // Schedule the new job
            var newJobId = _backgroundJobClient.Schedule<IBooking>(
                job => job.RefundWaitlistCredits(classId),
                endTime);

            // Update the dictionary with the new job ID
            _scheduledJobs[classId] = newJobId;
        }
    }

}
