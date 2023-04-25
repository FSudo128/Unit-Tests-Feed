using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Web4you.Web.Core;
using Web4you.Web.Delivery;
using Web4you.Web.Requests;

namespace Web.Users
{
    public class ChatGroup : EntityBase, IEntityOwner
	{
        public string Name { get; set; }
        
        public virtual ICollection<UserChatGroup> Members { get; set; }
        public virtual ICollection<ChatMessage> Messages { get; set; }
        
        public Guid? RequestId { get; set; }
        public virtual Request Request { get; set; }
        
        public Guid? QuoteId { get; set; }
        public virtual Quote Quote { get; set; }
        
        public Guid? WorkOrderId { get; set; }
        public virtual WorkOrder WorkOrder { get; set; }
        
        public Guid? InvoiceId { get; set; }
        public virtual Invoice Invoice { get; set; }

        public Guid UserId { get; set; }
		public virtual User User { get; set; }
    }

    public class ChatGroupMap : EntityBaseMap<ChatGroup>
    {
        public override void Configure(EntityTypeBuilder<ChatGroup> b)
        {
            b.HasOne(e => e.Request).WithMany().HasForeignKey(e => e.RequestId).OnDelete(DeleteBehavior.SetNull);
            b.HasOne(e => e.Quote).WithMany().HasForeignKey(e => e.QuoteId).OnDelete(DeleteBehavior.SetNull);
            b.HasOne(e => e.WorkOrder).WithMany().HasForeignKey(e => e.WorkOrderId).OnDelete(DeleteBehavior.SetNull);
            b.HasOne(e => e.Invoice).WithMany().HasForeignKey(e => e.InvoiceId).OnDelete(DeleteBehavior.SetNull);

            base.Configure(b);
        }
    }
}
