using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MusicApp2017.Models
{
    public class Genre
    {
        public int GenreID { get; set; }
        [Required(ErrorMessage = "Genre name is required")]
        [StringLength(20, ErrorMessage = "Genre name can't be more than 20 characters")]
        public string Name { get; set; }
    }
}