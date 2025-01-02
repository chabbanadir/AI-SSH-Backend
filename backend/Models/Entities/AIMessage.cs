using Backend.Models.Communs;
using Backend.Models.Entities;

namespace Backend.Models.Entities
{
    public class AIMessage : AuditableEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Sender { get; set; } = default!;
        public string Message { get; set; } = default!; // Renamed from MessageText
        public DateTime SentAt { get; set; } // Renamed from Timestamp
        public string AIConversationId { get; set; } = default!;
        public AIConversation AIConversation { get; set; } = default!;
    }


}
