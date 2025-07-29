using ListKeeperWebApi.WebApi.Models;
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

        public int? NoteCategoryId { get; set; }
        public NoteCategory? NoteCategory { get; set; }

        /// <summary> 
        /// Gets or sets the ID of the user who owns this note 
        /// </summary> 
        [Required]
        public int UserId { get; set; }

        /// <summary> 
        /// Navigation property to the User who owns this note 
        /// </summary> 
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public Note()
        {
            Title = string.Empty;
            Content = string.Empty;
            Color = string.Empty;
        }
    }
}
