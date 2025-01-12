using Backend.Models.Communs;
using Backend.Models.Entities.AI;

namespace Backend.Models.Entities.AI
{
    public class AIMessage : AuditableEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Sender { get; set; } = default!; // "user" or "ai"
        public string Message { get; set; } = default!;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public string AIConversationId { get; set; } = default!;
        public AIConversation AIConversation { get; set; } = default!;
    }


}
