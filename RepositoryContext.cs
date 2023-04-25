using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Web.Backend.Core;
using Web.Backend.Core.Entities;
using Web.Backend.Requests.Entities;
using Web.Backend.Reviews.Entities;
using Web4you.Web.Configuration;
using Web.Users;
using Web4you.Web.Core;
using Web4you.Web.Delivery;
using Web4you.Web.Requests;
using Web.Backend.Delivery;

namespace Web4you.Data;

public class RepositoryContext : IdentityDbContext<User,
    Role,
    Guid,
    IdentityUserClaim<Guid>,
    UserRole,
    IdentityUserLogin<Guid>,
    IdentityRoleClaim<Guid>,
    IdentityUserToken<Guid>>
{
    private readonly IServiceProvider _serviceProvider;
    public DbSet<Increment> Increments { get; set; }
    public DbSet<BinaryResource> BinaryResources { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Province> Province { get; set; }
    public DbSet<Country> Country { get; set; }
    public DbSet<City> City { get; set; }
    public DbSet<Notification> Notification { get; set; }
    public DbSet<Category> Category { get; set; }
    public DbSet<ErrorLog> ErrorLog { get; set; }
    public DbSet<EmailLog> EmailLog { get; set; }

    public DbSet<Inventory> Inventory { get; set; }
    public DbSet<Service> Service { get; set; }

    public DbSet<ConfigEntry> ConfigEntries { get; set; }

    public DbSet<Request> Request { get; set; }
    public DbSet<RequestOpen> RequestOpen { get; set; }
    public DbSet<RequestEmailSent> RequestEmailSent { get; set; }
    public DbSet<Attachment> Attachment { get; set; }
    public DbSet<AttachmentFile> AttachmentFile { get; set; }
    public DbSet<Folder> Folder { get; set; }
    public DbSet<EditableFile> EditableFile { get; set; }
    public DbSet<EditableFileFolder> EditableFileFolder { get; set; }
    public DbSet<Quote> Quote { get; set; }
    public DbSet<ServiceLine> QuoteLine { get; set; }
    public DbSet<WorkOrder> WorkOrder { get; set; }
    public DbSet<WorkOrderVisit> WorkOrderVisit { get; set; }
    public DbSet<WorkOrderVisitTravelLog> WorkOrderVisitTravelLog {get; set;}
    public DbSet<WorkOrderInvoice> WorkOrderInvoice { get; set; }
    public DbSet<RequestQuestion> RequestQuestion { get; set; }
    public DbSet<RequestQuestionOption> RequestQuestionOption { get; set; }
    public DbSet<RequestQuestionAnswer> RequestQuestionAnswer { get; set; }
    public DbSet<ServiceLine> ServiceLine { get; set; }
    public DbSet<WorkOrderTimeSheet> WorkOrderTimeSheet { get; set; }
    public DbSet<WorkOrderVisitWorker>  WorkOrderVisitWorker { get; set; }
    public DbSet<Invoice> Invoice { get; set; }
    public DbSet<InvoiceDeposit> InvoiceDeposit { get; set; }
    public DbSet<InvoicePayment> InvoicePayment { get; set; }
    public DbSet<Tax> Tax { get; set; }
    public DbSet<TaxGroup> TaxGroup { get; set; }
    public DbSet<Template> Template { get; set; }    
    public DbSet<UserTeamMember> UserTeamMember { get; set; }
    public DbSet<UserClient> UserClient { get; set; }
    public DbSet<Property> Property { get; set; }
    public DbSet<Phone> Phone { get; set; }
    public DbSet<UserCategory> UserCategory { get; set; }
    public DbSet<ManagedCategory> ManagedCategory { get; set; }
    public DbSet<ChatMessage> ChatMessage { get; set; }
    public DbSet<ChatGroup> ChatGroup { get; set; }
    public DbSet<UserChatGroup> UserChatGroup { get; set; }
    public DbSet<PermissionPreset> PermissionPreset { get; set; }
    public DbSet<Language> Language { get; set; }
    public DbSet<Review> Review { get; set; }
    public DbSet<UserDashboard> UserDashboard { get; set; }
    public DbSet<DashboardWidget> DashboardWidget { get; set; }
    public DbSet<Widget> Widget { get; set; }
    public DbSet<Lead> Lead { get; set; }
    public DbSet<Integration> Integration { get; set; }
    public DbSet<WorkerTimeOffRequest> WorkerTimeOffRequest { get; set; }
    public DbSet<TeamMemberTag> TeamMemberTag { get; set; }
    public DbSet<TeamMemberService> TeamMemberService { get; set; }
    public DbSet<T> GetDbSet<T>() where T : class, IEntityBase
    {
        var prop = GetType().GetProperties().FirstOrDefault(c => c.PropertyType == typeof(DbSet<T>) && c.PropertyType.GenericTypeArguments[0] == typeof(T));
        if (prop == null)
            throw new Exception($"{nameof(DbSet<T>)} for {typeof(T).Name} not found on database context");

        return prop.GetValue(this, null) as DbSet<T>;
    }

    public RepositoryContext(DbContextOptions<RepositoryContext> options, IServiceProvider serviceProvider) : base(options)
    {
        _serviceProvider = serviceProvider;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(UserMap).Assembly);
    }

    public IQueryable<T> Query<T>() where T : class, IEntityBase
    {
        if (typeof(IManagedEntity).IsAssignableFrom(typeof(T)))
            return ApplyGlobalFilters(GetDbSet<T>().Include(i => (i as IManagedEntity).Property).AsQueryable());
        else if (typeof(IEntityOwner).IsAssignableFrom(typeof(T)))
            return ApplyGlobalFilters(GetDbSet<T>().Include(i => (i as IEntityOwner).User).AsQueryable());
        else
            return ApplyGlobalFilters(GetDbSet<T>().AsQueryable());
    }

    public IQueryable<T> ApplyGlobalFilters<T>(IQueryable<T> query) where T : class, IEntityBase
    {
        var filter = _serviceProvider.GetService<BaseQueryFilter<T>>();
        return filter != null ? filter.ApplyGlobalFilters(query) : query;
    }

    public void DetachAllEntities()
    {
        var changedEntriesCopy = this.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added ||
                        e.State == EntityState.Modified ||
                        e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in changedEntriesCopy)
            entry.State = EntityState.Detached;
    }

    public T ExcludeNavigationProperties<T>(T entity) where T : IEntityBase, new()
    {
        var navProps = Model
            .GetEntityTypes()
            .First(i => i.Name == typeof(T).FullName)
            .GetNavigations().Select(x => x.PropertyInfo)
            .ToList();

        var copy = new T {
            Id = entity.Id
        };

        var props = typeof(T).GetProperties()
            .ExceptBy(navProps.Select(k => k.Name), k => k.Name)
            .Where(p => p.CanWrite);

        foreach (var p in props)
            p.SetValue(copy, p.GetValue(entity, null), null);

        return copy;
    }

    public void UpdateCollection<T>(ICollection<T> original, ICollection<T> changed) where T : class, IEntityBase, new()
    {
        var toDelete = original.ExceptBy(changed.Select(i => i.Id), i => i.Id)
            .Select(ExcludeNavigationProperties)
            .ToList();
        GetDbSet<T>().RemoveRange(toDelete);

        var toAdd = changed.ExceptBy(original.Select(i => i.Id), i => i.Id)
            .Select(ExcludeNavigationProperties)
            .ToList();
        GetDbSet<T>().AddRange(toAdd);

        var toUpdate = changed.IntersectBy(original.Select(i => i.Id), i => i.Id)
            .Select(ExcludeNavigationProperties)
            .ToList();
        GetDbSet<T>().UpdateRange(toUpdate);
    }
}
