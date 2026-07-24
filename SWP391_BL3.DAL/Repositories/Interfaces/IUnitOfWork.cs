using Microsoft.EntityFrameworkCore.Storage;

namespace SWP391_BL3.DAL.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IDbContextTransaction BeginTransaction();
        int SaveChanges();
    }
}
