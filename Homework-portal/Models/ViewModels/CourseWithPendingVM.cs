using Homework_portal.Models;

namespace Homework_portal.Models.ViewModels
{
    public class CourseWithPendingVM
    {
        public Course Course { get; set; } = null!;
        public int PendingAssignmentCount { get; set; }
    }
}
