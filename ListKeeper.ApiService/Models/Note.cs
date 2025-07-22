using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Numerics;

namespace ListKeeper.ApiService.Models
{
    [Table("Note")]
    public class Note
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Content { get; set; }

        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }

        [StringLength(16)]
        public string Color { get; set; }

        public Note()
        {
            Title = string.Empty;
            Content = string.Empty;
            Color = string.Empty;
        }
    }
}
