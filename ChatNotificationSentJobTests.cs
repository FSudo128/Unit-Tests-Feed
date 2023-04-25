using Hangfire;
using Hangfire.Server;
using Moq;
using Web.Backend.Core.Components;
using Web.Backend.Core.Jobs;
using Web.Users;
using Web4you.Web.Core;

namespace Tests.Core;

public class ChatNotificationSentJobTests : BaseTest
{
    private ChatNotificationSentJob _job;
    
    [SetUp]
    public void Init()
    {
        
    }
    
    [Test]
    public void TestExecuteWithMultipleNotifications()
    {
        var newChat = new ChatGroup(){Name = "Test 1 Chat Group", UserId = Admin.Id};
        var memberAdmin = new UserChatGroup(){ChatGroupId = newChat.Id, UserId = Admin.Id};
        var memberServiceProvider = new UserChatGroup(){ChatGroupId = newChat.Id, UserId = ServiceProvider.Id};
        var newMessage = new ChatMessage()
        {
            Message = "Test new unread messages from admin.",
            ChatGroupId = newChat.Id,
            FromUserId = Admin.Id,
            ToUserId = ServiceProvider.Id
        };
        
        Context.ChatGroup.Add(newChat);
        Context.UserChatGroup.Add(memberAdmin);
        Context.UserChatGroup.Add(memberServiceProvider);
        Context.ChatMessage.Add(newMessage);
        Context.SaveChanges();

        _job = new ChatNotificationSentJob(Context, EmailHelperMock.Object, TestCtx.Services.GetService<CommonHelper>());
        _job.DoWork(new CancellationToken(), null, "");
        
        EmailHelperMock.Verify(i => i.SendAsync(
            It.Is<List<string>>(i => i.Contains(ServiceProvider.Email)), 
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            null, null, new CancellationToken()), Times.Once);
    }
}
