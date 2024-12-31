// Models/Todo.cs
using System.ComponentModel.DataAnnotations;

namespace Backend.Data{

    public class Todo
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }
        public Todo()
            {
                Title = string.Empty;
            }
    }
}