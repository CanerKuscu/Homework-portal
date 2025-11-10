using Homework_portal.Data;
using Homework_portal.Models;

namespace Homework_portal.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;

        public IRepository<ApplicationUser> User { get; private set; }
        public IRepository<Ders> Ders { get; private set; }
        public IRepository<Odev> Odev { get; private set; }
        public IRepository<Teslim> Teslim { get; private set; }
        public IRepository<DersKayit> DersKayit { get; private set; }

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            User = new Repository<ApplicationUser>(_db);
            Ders = new Repository<Ders>(_db);
            Odev = new Repository<Odev>(_db);
            Teslim = new Repository<Teslim>(_db);
            DersKayit = new Repository<DersKayit>(_db);
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