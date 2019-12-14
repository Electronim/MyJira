using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyJira.Models
{
    public class Team
    {
        [Key]
        public int TeamId { get; set; }

        [Required(ErrorMessage = "Name of the Team is mandatory")]
        [StringLength(30, ErrorMessage = "Team name cannot have more than 30 characters")]
        public string TeamName { get; set; }
        //public virtual ICollection<ApplicationUser> Devs { set; get; }
    }
}