using Hangfire.Server;
using Web4you.Data;
using Web4you.Web.Core;

namespace Web.Backend.Core.Jobs;

public class ChatNotificationSentJob : BaseJob<string>
{
    private readonly IEmailHelper _emailHelper;
    private readonly CommonHelper _commonHelper;
    
    public ChatNotificationSentJob(RepositoryContext ctx, IEmailHelper emailHelper, CommonHelper commonHelper) : base(ctx)
    {
        _emailHelper = emailHelper;
        _commonHelper = commonHelper;
    }

    public override Task DoWork(CancellationToken stoppingToken, PerformContext context, string options)
    {
        var notSendMessagesExist = Context.ChatMessage.Where(i => !i.NotificationSent && i.DateCreated >= DateTime.Now.AddHours(-4)).ToList();

        if (!notSendMessagesExist.Any()) return Task.CompletedTask;
        
        var usersToNotify = notSendMessagesExist.Select(i => i.ToUserId).Distinct();

        foreach (var userId in usersToNotify)
        {
            var unreadMessages = Context.ChatMessage.Where(i => i.ToUserId == userId && !i.Read).ToList();
            var user = Context.Users.FirstOrDefault(i => i.Id == userId);
            user.NumberOfUnreadMessages = unreadMessages.Count;
            
            var (emailBody, emailSubject, _) = _commonHelper.GetTemplatesWithMappingByName(user, EmailTemplateType.SystemNumberofUnreadMessages);

            _emailHelper.SendAsync(new List<string>(){user.Email}, emailSubject, emailBody);
            
            foreach (var message in unreadMessages)
            {
                message.NotificationSent = true;
            }

            Context.SaveChanges();
        }
        
        return Task.CompletedTask;
    }
}
