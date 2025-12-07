namespace Homework_portal.Models.ViewModels
{
    public class CourseWithPendingSubmissionsVM
    {
        public Course Course { get; set; } = null!;
        public int PendingSubmissionCount { get; set; }
    }
}
