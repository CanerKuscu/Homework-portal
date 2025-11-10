using Homework_portal.Models;

namespace Homework_portal.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<ApplicationUser> User { get; }
        IRepository<Ders> Ders { get; }
        IRepository<Odev> Odev { get; }
        IRepository<Teslim> Teslim { get; }
        IRepository<DersKayit> DersKayit { get; }

        void Save();
    }
}