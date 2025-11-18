using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;

namespace Homework_portal.Models.ViewModels
{
    public class AssignmentsVM
    {
        public Course Course { get; set; }
        public IEnumerable<Assignment> Assignments { get; set; } = new List<Assignment>();
        [ValidateNever]
        public IEnumerable<Submission> StudentSubmissions { get; set; } = new List<Submission>();
    }
}
