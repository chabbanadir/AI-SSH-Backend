using Backend.Models.Communs;
using Backend.Models.Entities;

namespace Backend.Models.Entities
{
    public class AIMessage : AuditableEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Sender { get; set; } = default!;
        public string MessageText { get; set; } = default!;
        public DateTime Timestamp { get; set; }
        public string AIConversationId { get; set; } = default!;
        public AIConversation AIConversation { get; set; } = default!;
    }
}
