using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

namespace Homework_portal.Models.ViewModels
{
    public class AssignmentVM
    {
        public Assignment Assignment { get; set; }
        public IEnumerable<SelectListItem>? CourseList { get; set; }
        public string[]? SelectedClasses { get; set; }
        public string[]? SelectedBranches { get; set; }
        public IEnumerable<string>? AllClasses { get; set; }
        public IEnumerable<string>? AllBranches { get; set; }
    }
}
