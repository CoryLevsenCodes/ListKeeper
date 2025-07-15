using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Numerics;

namespace ListKeeper.ApiService.Models
{
    [Table("Notes")]
    public class Note
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(500)]
        public string Content { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public Boolean IsCompleted { get; set; }

        [Required]
        [StringLength(16)]
        public string Color { get; set; }

        public Note() {
            Title = string.Empty;
            Content = string.Empty;
            Color = string.Empty;
        }
    }
}
