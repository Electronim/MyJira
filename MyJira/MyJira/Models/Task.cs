using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace MyJira.Models
{
    public enum TaskStatus
    {
        Todo,
        InProgress,
        Done
    }

    public class Task
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The title is mandatory")]
        public string Title { get; set; }

        [Required(ErrorMessage = "The description is mandatory")]
        public string Description { get; set; }

        public TaskStatus Status { get; set; }

        [DataType(DataType.DateTime, ErrorMessage = "Date and Time is required")]
        public DateTime StartTime { get; set; }

        [DataType(DataType.DateTime, ErrorMessage = "Date and Time is required")]
        public DateTime FinishTime { get; set; }

        public string AssigneeId { get; set; }
        public virtual ApplicationUser Assignee { get; set; }

        [Required]
        public string ReporterId { get; set; }
        public virtual ApplicationUser Reporter { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
    }
}