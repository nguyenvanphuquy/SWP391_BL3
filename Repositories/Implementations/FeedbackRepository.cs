using Microsoft.EntityFrameworkCore;
using SWP391_BL3.Data;
using SWP391_BL3.Models.Entities;
using SWP391_BL3.Repositories.Interfaces;

namespace SWP391_BL3.Repositories.Implementations
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly FptBookingContext _context;
        public FeedbackRepository(FptBookingContext context)
        {
            _context = context;
        }
        public Feedback Create(Feedback fb)
        {
            _context.Feedbacks.Add(fb);
            _context.SaveChanges();
            return fb;
        }

        public Feedback Update(Feedback fb)
        {
            _context.Feedbacks.Update(fb);
            _context.SaveChanges();
            return fb;
        }

        public bool Delete(int id)
        {
            var fb = _context.Feedbacks.Find(id);
            if (fb == null) return false;

            _context.Feedbacks.Remove(fb);
            _context.SaveChanges();
            return true;
        }

        public Feedback GetById(int id)
        {
            return _context.Feedbacks
                .Include(x => x.User)
                .Include(x => x.Facility)
                .FirstOrDefault(x => x.FeedbackId == id);
        }

        public IEnumerable<Feedback> GetAll()
        {
            return _context.Feedbacks
                .Include(x => x.User)
                .Include(x => x.Facility)
                .ToList();
        }

        public IEnumerable<Feedback> GetByFacility(int facilityId)
        {
            return _context.Feedbacks
                .Include(x => x.User)
                .Include(x => x.Facility)
                .Where(x => x.FacilityId == facilityId)
                .ToList();
        }
    }
}
