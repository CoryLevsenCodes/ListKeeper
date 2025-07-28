using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ListKeeper.ApiService.Models
{
    [Table("NoteCategory")]
    public class NoteCategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NoteCategoryId { get; set; }

        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public ICollection<Note>? Notes { get; set; }    

        //public NoteCategoryViewModel()
        //{
        //    Name = string.Empty;
        //    Description = string.Empty;
        //}
    }
}
