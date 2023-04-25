using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using CurrieTechnologies.Razor.SweetAlert2;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Syncfusion.Blazor;
using Web.Backend.Core;
using Web.Backend.Core.Helpers;
using Web.Backend.Requests.Entities;
using Web4you.Data;
using Web4you.Web.Areas.Identity;
using Web4you.Web.Configuration;
using Web4you.Web.Core;
using Web4you.Web.Delivery;
using Web4you.Web.Requests;
using Web.Users;
using static Web4you.Web.Requests.RequestQuestion;
using TestContext = Bunit.TestContext;
using User = Web.Users.User;
using AngleSharp.Dom;
using Microsoft.Extensions.Configuration;

namespace Tests;

public class BaseTest
{
    private DbContextOptions<RepositoryContext> _options;
    protected RepositoryContext Context;
    protected TestContext TestCtx;
    protected Mock<IPublishEndpoint> PublishEndpointMock;
    protected Mock<IEmailHelper> EmailHelperMock;
    protected Mock<ISmsHelper> SmsHelperMock;

    protected User Admin;
    protected User Client;
    protected User Client2;
    protected User ServiceProvider;
    protected User ServiceProvider2;
    protected User OfficeManager;
    protected User Manager;
    protected User Manager2;
    protected Country Country1;
    protected Province Province1;
    protected City City1;
    protected City City2;

    protected Category Category1 = new Category
    {
        Name = "Roof Inspection",
        Group = "Roofing",
        Price = 3
    };
    protected Category Category2 = new Category
    {
        Name = "Category1",
        Group = "Common",
        Price = 10
    };

    public void SetEntities()
    {
        Admin = new()
        {
            Email = "admin@web4you.ca",
            LastActivityDate = DateTime.Now,
        };

        Client = new()
        {
            Email = "client@web4you.ca",
            LastActivityDate = DateTime.Now,
            PermissionPreset = DefaultPresets.Client
        };

        Client2 = new()
        {
            Email = "client2@web4you.ca",
            LastActivityDate = DateTime.Now,
            PermissionPreset = DefaultPresets.Client
        };

        ServiceProvider = new()
        {
            Email = "service@web4you.ca",
            LastActivityDate = DateTime.Now,
            PermissionPreset = DefaultPresets.ServiceProvider
        };

        ServiceProvider2 = new()
        {
            Email = "service2@web4you.ca",
            LastActivityDate = DateTime.Now,
            PermissionPreset = DefaultPresets.ServiceProvider
        };

        OfficeManager = new()
        {
            Email = "office@web4you.ca",
            FullName = "Test Office Manager"
        };
        
        Manager = new()
        {
            Email = "manager@web4you.ca",
            LastActivityDate = DateTime.Now,
        };

        Manager2 = new()
        {
            Email = "manager2@web4you.ca",
            LastActivityDate = DateTime.Now,
        };

        Country1 = new Country() { Name = "Canada", };
        Province1 = new Province() { Name = "Alberta", CountryId = Country1.Id };
        City1 = new City() { Name = "Calgary", ProvinceId = Province1.Id };
        City2 = new City() { Name = "Calgary 2", ProvinceId = Province1.Id };

        Category1 = new Category
        {
            Name = "Roof Inspection",
            Group = "Roofing",
            Price = 3
        };
        Category2 = new Category
        {
            Name = "Category1",
            Group = "Common",
            Price = 10
        };
    }
    
    [SetUp]
    public void Init()
    {
        _options = new DbContextOptionsBuilder<RepositoryContext>()
            .UseInMemoryDatabase(databaseName: "servicedeck")
            .Options;

        TestCtx = new TestContext();
        TestCtx.JSInterop.Mode = JSRuntimeMode.Loose;

        AddServices();
        SetEntities();
        AddTestData();
    }

    [TearDown]
    public void Cleanup()
    {
        Context.ChangeTracker.Clear();
        Context.Database.EnsureDeleted();
    }

    protected virtual void AddTestData()
    {
        Context.Users.Add(Admin);
        Context.Users.Add(Client);
        Context.Users.Add(Client2);
        Context.Users.Add(ServiceProvider);
        Context.Users.Add(ServiceProvider2);
        Context.Users.Add(Manager);
        Context.Users.Add(Manager2);
        Context.Country.Add(Country1);
        Context.Province.Add(Province1);
        Context.City.Add(City1);

        var country = new Country() { Name = "United States", };
        Context.Country.Add(country);

        var province = new Province() { Name = "Washington", CountryId = country.Id };
        Context.Province.Add(province);

        var city = new City() { Name = "Anacortes", ProvinceId = province.Id };
        Context.City.Add(city);

        Context.Users.Add(new User()
        {
            Email = "client1@web4you.ca",
            LastActivityDate = DateTime.Now,
            PermissionPreset = DefaultPresets.Client,
            Properties = new List<Property>
            {
                new()
                {
                    AddressLine1 = "Anacortes Ferry Terminal",
                    AddressLine2 = "",
                    PostalCode = "98221",
                    ProvinceId = province.Id,
                    CountryId = country.Id,
                    CityId = city.Id,
                }
            }
        });

        Context.Property.Add(new Property
                {
                    AddressLine1 = "Anacortes Ferry Terminal",
                    AddressLine2 = "",
                    PostalCode = "98221",
                    ProvinceId = province.Id,
                    CountryId = country.Id,
                    CityId = city.Id,
                    UserId = Client.Id
                });            

        Context.Category.Add(Category1);
        Context.Category.Add(Category2);

        Context.SaveChanges();
    }

    private void AddServices()
    {
        TestCtx.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<User>>();
        TestCtx.Services.AddScoped<Config>();
        TestCtx.Services.AddScoped<CaptchaVerificationService>();
        TestCtx.Services.AddScoped<AuthHelper>();
        TestCtx.Services.AddScoped<UploadHelper>();
        TestCtx.Services.AddScoped<CommonHelper>();
        TestCtx.Services.AddScoped<UserOptions>();
        TestCtx.Services.AddScoped<UiUpdatesService>();
        TestCtx.Services.AddScoped<StateContainer>();
        TestCtx.Services.AddScoped<IAuthorizationHandler, EditAuthorizationHandler>();
        TestCtx.Services.AddSyncfusionBlazor();
        TestCtx.Services.AddSweetAlert2();
        TestCtx.Services.AddRazorPages();
        TestCtx.Services.AddCoreServices();
        TestCtx.Services.AddScoped<ProtectedLocalStorage>();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, false)
            .AddEnvironmentVariables()
            .Build();
        
        TestCtx.Services.Configure<CaptchaConfig>(configuration.GetSection(CaptchaConfig.Position));
        TestCtx.Services.Configure<CryptoConfig>(configuration.GetSection(CryptoConfig.Position));
        TestCtx.Services.Configure<PathsConfig>(configuration.GetSection(PathsConfig.Position));
        TestCtx.Services.Configure<CommonConfig>(configuration.GetSection(CommonConfig.Position));
        TestCtx.Services.Configure<SmtpConfig>(configuration.GetSection(SmtpConfig.Position));
        TestCtx.Services.Configure<StripeConfig>(configuration.GetSection(StripeConfig.Position));
        TestCtx.Services.Configure<GoogleConfig>(configuration.GetSection(GoogleConfig.Position));
        TestCtx.Services.Configure<PlansConfig>(configuration.GetSection(PlansConfig.Position));
        TestCtx.Services.Configure<QuickBookConfig>(configuration.GetSection(QuickBookConfig.Position));
        TestCtx.Services.Configure<CDyneConfig>(configuration.GetSection(CDyneConfig.Position));
        TestCtx.Services.Configure<DemoClientsConfig>(configuration.GetSection(DemoClientsConfig.Position));
        
        
        PublishEndpointMock = new Mock<IPublishEndpoint>();
        TestCtx.Services.AddSingleton(PublishEndpointMock.Object);

        EmailHelperMock = new Mock<IEmailHelper>();
        TestCtx.Services.AddSingleton(EmailHelperMock.Object);

        SmsHelperMock = new Mock<ISmsHelper>();
        TestCtx.Services.AddSingleton(SmsHelperMock.Object);

        Context = new RepositoryContext(_options, TestCtx.Services);
        TestCtx.Services.AddSingleton(Context);

        var store = new UserStore<User, Role, RepositoryContext, Guid>(Context);
        var um = new UserManager<User>(store, null, new PasswordHasher<User>(), null, null, null, null, null, null);
        TestCtx.Services.AddSingleton(um);
    }

    protected virtual void LoginAs(User user, string role = UserRoles.User, List<Claim> claims = null)
    {
        var authContext = TestCtx.AddTestAuthorization();
        TestCtx.Services.AddScoped<IAuthorizationService, DefaultAuthorizationService>();
        
        authContext.SetAuthorized(role);
        authContext.SetRoles(role);

        var allClaims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };
        
        if (claims != null)
            allClaims.AddRange(claims);

        authContext.SetClaims(allClaims.ToArray());
    }

    protected Request CreatedRequest(User client, Category category, User serviceProvider = null, bool isArchived = false, bool approved = true, RequestStatus status = RequestStatus.New)
    {
        var random = new Random();

        var approvalStatus = approved ? RequestApprovalStatus.Approved : RequestApprovalStatus.Pending;

        return new Request()
        {
            Number = "RQ" + random.Next(10000, 19999),
            UserId = serviceProvider?.Id ?? client.Id,
            ClientId = client.Id,
            PropertyId = Context.Property
                .Where(i => i.UserId == client.Id)
                .Select(i => i.Id)
                .FirstOrDefault(),
            CategoryId = category.Id,
            PreferredAnyTime = true,
            Title = "Nulla porttitor accumsan tincidunt.",
            Description = "Nulla quis lorem ut libero malesuada feugiat. Pellentesque in ipsum id orci porta dapibus. Donec sollicitudin molestie malesuada.",
            StartDate1 = DateTime.Now,
            StartDate2 = DateTime.Now.AddDays(1),
            Budget = Math.Round((double)random.Next(1000, 10000), 2).ToString(),
            NumberOfEstimates = 1,
            ShowOnMarket = true,
            ApprovalStatus = approvalStatus,
            IsArchived = isArchived,
            Status = status,
            Attachments = new List<Attachment>()
            {
                new()
                {
                    Files = new List<AttachmentFile>
                    {
                        new()
                        {
                            File = "c:/Some/test/file",
                            FileName = "some file name",
                        }
                    }
                }
            }
        };
    }

    protected WorkOrder CreatedWorkOrder(User user, User client, bool isArchived = false, Property property = null)
    {
        var random = new Random();

        return new WorkOrder()
        {
            Number = "RQ" + random.Next(10000, 19999),
            UserId = user.Id,
            IsArchived = isArchived,
            ClientId = client.Id,
            PropertyId = property != null
                ? property.Id
                : Context.Property
                    .Where(i => i.UserId == client.Id)
                    .Select(i => i.Id)
                    .FirstOrDefault(),
            Title = "Nulla porttitor accumsan tincidunt.",
            Instructions = "Nulla quis lorem ut libero malesuada feugiat. Pellentesque in ipsum id orci porta dapibus. Donec sollicitudin molestie malesuada.",
            Status = WorkOrderStatus.Scheduled,
            Lines = new List<ServiceLine>(),
            Attachments = new List<Attachment>(),
            Visits = new List<WorkOrderVisit>()
        };
    }

    protected WorkOrderVisit CreatedWorkOrderVisit(WorkOrder workOrder, Service service = null)
    {
        var random = new Random();

        return new WorkOrderVisit()
        {
            Title = "Visit nulla porttitor accumsan tincidunt.",
            Status = VisitStatus.Pending,
            SchedulerId = random.Next(10000, 19999),
            ServiceId = service?.Id,
            Instructions = "Visit inst nulla porttitor accumsan tincidunt.",
            EndDate = DateTime.Now.AddHours(1),
            StartDate = DateTime.Now.AddHours(-1),
            Workers = new List<WorkOrderVisitWorker>(),
            WorkOrderId = workOrder.Id,
            UserId = workOrder.UserId
        };
    }

    protected Invoice CreatedInvoice(User user, User client, bool isArchived = false, Property property = null)
    {
        var random = new Random();

        return new Invoice()
        {
            Number = "IN" + random.Next(10000, 19999),
            UserId = user.Id,
            IsArchived = isArchived,
            ClientId = client.Id,
            PropertyId = property != null ? property.Id : Context.Property.FirstOrDefault(i => i.UserId == client.Id)?.Id,
            Subtotal = 1000,
            Total = 1000,
            IssuedDate = DateTime.Now,
            PaymentDueDate = DateTime.Now.AddDays(30),
            Status = InvoiceStatus.Draft,
            Title = "New Invoice for Service"
        };
    }

    protected Service CreatedService(User user)
    {
        return new Service()
        {
            UserId = user.Id,
            Cost = 10,
            Name = "Roof Cleaning",
            Colour = "#fbc02dff",
            Markup = 0,
            UnitPrice = 10,
            Description = "Description of service"
        };
    }
    
    protected Inventory CreatedProduct(User user)
    {
        return new Inventory
        {
            Name = "Proin eget tortor risus.",
            Description = "Praesent sapien massa, convallis a pellentesque nec, egestas non nisi.",
            Sku = "123456",
            Qty = 10,
            UnitPrice = 100,
            Markup = 1,
            Cost = 100,
            Image = null,
            UserId = user.Id,
        };
    }

    protected User CreateWorker(User serviceProvider)
    {
        var worker = new User();

        worker.CreatedByUserId = serviceProvider.Id;
        worker.CompanyName = serviceProvider.CompanyName;
        worker.CompanyLogo = serviceProvider.CompanyLogo;
        worker.PermissionPreset = DefaultPresets.Worker;

        return worker;
    }

    protected RequestOpen CreatedRequestOpen(Request request, User serviceProvider, bool IsInvitation = false, bool IsArchived = false )
    {
        return new RequestOpen()
        {
            RequestId = request.Id,
            UserId = serviceProvider.Id,
            IsInvitation = IsInvitation,
            IsArchived = IsArchived
        };
    }

    protected Quote CreatedQuote(User client, User professional, QuoteStatus status, bool isArchived = false)
    {
        var random = new Random();

        return new Quote()
        {
            Number = "WO" + random.Next(10000, 19999),
            UserId = professional.Id,
            ClientId = client.Id,
            PropertyId = Context.Property
                .Where(i => i.UserId == client.Id)
                .Select(i => i.Id)
                .FirstOrDefault(),
            Title = "Suspendisse non semper nulla",
            Status = status,
            Total = (decimal)Math.Round((double)random.Next(1000, 10000)),
            IsArchived = isArchived
        };
    }

    protected Category CreatedCategory(string name, string group, int price = 1)
    {
        return new Category()
        {
            Name = name,
            Group = group,
            Price = price
        };
    } 

    protected UserCategory CreatedUserCategory(User user, Category category)
    {
        return new UserCategory()
        {
            User = user,
            Category = category
        };
    }

    protected RequestQuestion CreatedRequestQuestion(Category category, string title, QuestionType type, int order, bool required = false)
    {
        return new RequestQuestion()
        {
            CategoryId = category.Id,
            Title = title,
            Type = type,
            Order = order,
            Required = required
        };
    }

    protected RequestQuestionOption CreatedRequestQuestionOption(RequestQuestion question, string title)
    {
        return new RequestQuestionOption()
        {
            RequestQuestionId = question.Id,
            Title = title,
        };
    }

    protected Lead CreateLead(string name, string email, Guid categoryId, Guid userId)
    {
        return new Lead
        {
            FullName = name,
            Email = email,
            CategoryId = categoryId,
            UserId = userId
        };
    }

    protected void LoginAsProfessional(User user)
    {
        var authContext = TestCtx.AddTestAuthorization();        

        authContext.SetClaims(new (ClaimTypes.NameIdentifier, user.Id.ToString()),
            new ($"{PermissionType.Create}:{ModuleType.Quotes}", "1"),
            new ($"{PermissionType.View}:{ModuleType.Quotes}", "1"),
            new ($"{PermissionType.Create}:{ModuleType.Quotes}", "1"),
            new ($"{PermissionType.Create}:{ModuleType.Jobs}", "1"),
            new ($"{PermissionType.View}:{ModuleType.Jobs}", "1"),
            new ($"{PermissionType.Create}:{ModuleType.Jobs}", "1"),
            new ($"{PermissionType.Edit}:{ModuleType.Jobs}", "1"),
            new ($"{PermissionType.Edit}:{ModuleType.Requests}", "1"),
            new ($"{PermissionType.Create}:{ModuleType.Requests}", "1"),
            new ($"{PermissionType.View}:{ModuleType.Requests}", "1"),
            new ($"{PermissionType.Edit}:{ModuleType.Company}", "1"),
            new ($"{PermissionType.Create}:{ModuleType.Invoices}", "1"),
            new ($"{PermissionType.Edit}:{ModuleType.Invoices}", "1"),
            new ($"{PermissionType.View}:{ModuleType.Invoices}", "1")
            );
    }

    protected void LoginAsClient(User user)
    {
        var authContext = TestCtx.AddTestAuthorization();
        authContext.SetClaims(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim($"{PermissionType.View}:{ModuleType.Jobs}", "1"),
            new Claim($"{PermissionType.View}:{ModuleType.Requests}", "1"),
            new Claim($"{PermissionType.View}:{ModuleType.Quotes}", "1")
            );
    }

    protected void SelectDdlItem(IElement ddl, string selectedItem)
    {
        var ulEle = ddl.QuerySelector("ul");
        Assert.True(ulEle.ClassName.Contains("e-ul"));
        var liCollection = ulEle.QuerySelectorAll("li.e-list-item");
        Assert.True(liCollection.Any(i => i.InnerHtml.Contains(selectedItem)));

        liCollection.First(i => i.InnerHtml.Contains(selectedItem)).Click();
    }
}
