using Homework_portal.Data;
using Homework_portal.Models;

namespace Homework_portal.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;

        public IRepository<ApplicationUser> User { get; private set; }
        public IRepository<Course> Course { get; private set; }
        public IRepository<Assignment> Assignment { get; private set; }
        public IRepository<Submission> Submission { get; private set; }
        public IRepository<CourseEnrollment> CourseEnrollment { get; private set; }

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            User = new Repository<ApplicationUser>(_db);
            Course = new Repository<Course>(_db);
            Assignment = new Repository<Assignment>(_db);
            Submission = new Repository<Submission>(_db);
            CourseEnrollment = new Repository<CourseEnrollment>(_db);
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}