using System.Collections.Generic;
using Homework_portal.Models;

namespace Homework_portal.Models.ViewModels
{
    public class AdminCourseAssignmentsVM
    {
        public Course Course { get; set; } = null!;
        public IList<Assignment> Assignments { get; set; } = new List<Assignment>();
    }

    public class AdminAssignmentSubmissionsVM
    {
        public Assignment Assignment { get; set; } = null!;
        public IList<Submission> Submissions { get; set; } = new List<Submission>();
    }
}
