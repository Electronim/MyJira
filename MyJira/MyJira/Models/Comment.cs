using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyJira.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The comment should not be empty")]
        public string Content { get; set; }

        [DataType(DataType.DateTime, ErrorMessage = "Date and Time is required")]
        public DateTime Date { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public int TaskId { get; set; }
        public virtual Task Task { get; set; }
    }
}