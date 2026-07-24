using Microsoft.EntityFrameworkCore.Storage;
using SWP391_BL3.DAL.Data;
using SWP391_BL3.DAL.Repositories.Interfaces;

namespace SWP391_BL3.DAL.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FptBookingContext _context;

        public UnitOfWork(FptBookingContext context)
        {
            _context = context;
        }

        public IDbContextTransaction BeginTransaction()
        {
            return _context.Database.BeginTransaction();
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            // DbContext is owned by DI; do not dispose here
        }
    }
}
