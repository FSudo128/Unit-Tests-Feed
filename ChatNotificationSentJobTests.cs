using Hangfire;
using Web.Backend.Core.Components;
using Web.Backend.Core.Jobs;
using Web4you.Web.Core;

namespace Tests.Core;

public class ChatNotificationSentJobTests : BaseTest
{
    private ChatNotificationSentJob _job;
    
    [SetUp]
    public void Init()
    {
        BackgroundJob.Enqueue<ChatNotificationSentJob>(job => job.DoWork(CancellationToken.None, null, null));
    }
}
