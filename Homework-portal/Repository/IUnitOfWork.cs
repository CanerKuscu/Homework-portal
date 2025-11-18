using Homework_portal.Models;

namespace Homework_portal.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<ApplicationUser> User { get; }
        IRepository<Course> Course { get; }
        IRepository<Assignment> Assignment { get; }
        IRepository<Submission> Submission { get; }
        IRepository<CourseEnrollment> CourseEnrollment { get; }

        void Save();
    }
}