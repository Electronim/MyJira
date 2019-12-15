using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyJira.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The name is mandatory")]
        public string Name { get; set; }

        [Required(ErrorMessage = "The description is mandatory")]
        public string Description { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<Task> Tasks { get; set; }
    }
}