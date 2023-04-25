using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Web4you.Web.Core;
using Web4you.Web.Requests;

namespace Web.Users
{
    public class UserChatGroup : EntityBase, IEntityOwner
	{
        public Guid ChatGroupId { get; set; }
        public virtual ChatGroup ChatGroup { get; set; }
        
        public Guid UserId { get; set; }
		public virtual User User { get; set; }
    }
    
    public class UserChatGroupMap : EntityBaseMap<UserChatGroup>
    {
        public override void Configure(EntityTypeBuilder<UserChatGroup> b)
        {
            b.HasOne(i => i.User).WithMany().HasForeignKey(i => i.UserId).OnDelete(DeleteBehavior.NoAction);

            base.Configure(b);
        }
    }
}
