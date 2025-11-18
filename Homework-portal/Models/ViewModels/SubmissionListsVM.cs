using System.Collections.Generic;
using Homework_portal.Models;

namespace Homework_portal.Models.ViewModels
{
    public class NonSubmissionItemVM
    {
        public Assignment Assignment { get; set; } = null!;
        public ApplicationUser Student { get; set; } = null!;
    }

    public class SubmissionListsVM
    {
        public IList<Submission> Submitted { get; set; } = new List<Submission>();
        public IList<NonSubmissionItemVM> NotSubmitted { get; set; } = new List<NonSubmissionItemVM>();
    }
}
