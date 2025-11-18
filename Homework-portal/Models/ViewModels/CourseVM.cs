using Homework_portal.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Homework_portal.Models.ViewModels
{
    public class CourseVM
    {
        public Course Course { get; set; }
        public SelectList? TeacherList { get; set; }
    }
}
