using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Web4you.Web.Core;
using Web4you.Web.Delivery;
using Web4you.Web.Requests;

namespace Web.Users
{
    public class ChatMessage : EntityBase
	{
        public string Message { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        
        public bool Read { get; set; }
        public bool NotificationSent { get; set; }
        
        public Guid? ReplyToChatMessageId { get; set; }
        public virtual ChatMessage ReplyToChatMessage { get; set; }
        
        public Guid? ToUserId { get; set; }
        public virtual User ToUser { get; set; }
        
        public Guid FromUserId { get; set; }
		public virtual User FromUser { get; set; }
        
		public Guid? RequestId { get; set; }
		public virtual Request Request { get; set; }
        
        public Guid? QuoteId { get; set; }
        public virtual Quote Quote { get; set; }
        
        public Guid? WorkOrderId { get; set; }
        public virtual WorkOrder WorkOrder { get; set; }
        
        public Guid? InvoiceId { get; set; }
        public virtual Invoice Invoice { get; set; }
        
        public Guid? ChatGroupId { get; set; }
        public virtual ChatGroup ChatGroup { get; set; }
    }
    
    public class ChatMessageMap : EntityBaseMap<ChatMessage>
    {
        public override void Configure(EntityTypeBuilder<ChatMessage> b)
        {
            b.HasOne(i => i.FromUser).WithMany().HasForeignKey(i => i.FromUserId).OnDelete(DeleteBehavior.NoAction);
            b.HasOne(e => e.Request).WithMany().HasForeignKey(e => e.RequestId).OnDelete(DeleteBehavior.SetNull);
            b.HasOne(e => e.Quote).WithMany().HasForeignKey(e => e.QuoteId).OnDelete(DeleteBehavior.SetNull);
            b.HasOne(e => e.WorkOrder).WithMany().HasForeignKey(e => e.WorkOrderId).OnDelete(DeleteBehavior.SetNull);
            b.HasOne(e => e.Invoice).WithMany().HasForeignKey(e => e.InvoiceId).OnDelete(DeleteBehavior.SetNull);

            base.Configure(b);
        }
    }
}
