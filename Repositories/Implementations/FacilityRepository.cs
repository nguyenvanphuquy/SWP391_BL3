using Microsoft.Build.Construction;
using Microsoft.EntityFrameworkCore;
using SWP391_BL3.Data;
using SWP391_BL3.Models.DTOs.Response;
using SWP391_BL3.Models.Entities;
using SWP391_BL3.Repositories.Interfaces;

namespace SWP391_BL3.Repositories.Implementations
{
    public class FacilityRepository : IFacilityRepository
    {
        private readonly FptBookingContext _context;
        public FacilityRepository(FptBookingContext context)
        {
            _context = context;
        }
        public IEnumerable<Facility> GetAll()
        {
            return _context.Facilities.ToList();
        }
        public Facility? GetById(int id)
        {
            return _context.Facilities.Find(id);
        }
        public Facility? GetByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            return _context.Facilities.FirstOrDefault(f => f.FacilityCode == code);
        }
        public void Create(Facility facility)
        {
            _context.Facilities.Add(facility);
            _context.SaveChanges();
        }
        public void Update(Facility facility)
        {
            _context.Facilities.Update(facility);
            _context.SaveChanges();
        }
        public void Delete(Facility facility)
        {
            _context.Facilities.Remove(facility);
            _context.SaveChanges();
        }
        public List<FacilityListResponse> GetFacilityList()
        {
            var list = (from f in _context.Facilities
                        join c in _context.Campuses on f.CampusId equals c.CampusId
                        join t in _context.FacilityTypes on f.TypeId equals t.TypeId
                        orderby f.CreateAt descending
                        select new FacilityListResponse
                        {
                            FacilityId = f.FacilityId,
                            FacilityCode = f.FacilityCode,
                            Capacity = f.Capacity,
                            Floors = f.Floor,
                            Equipment = f.Equipment,
                            Status = f.Status,
                            CampusName = c.CampusName,
                            TypeName = t.TypeName
                        }).ToList();
            return list;
        }
        public List<FacilityListResponse> GetAllList()
        {
            var list = (from f in _context.Facilities
                        join c in _context.Campuses on f.CampusId equals c.CampusId
                        join t in _context.FacilityTypes on f.TypeId equals t.TypeId
                        select new FacilityListResponse
                        {
                            FacilityId = f.FacilityId,
                            FacilityCode = f.FacilityCode,
                            Capacity = f.Capacity,
                            Floors = f.Floor,
                            Equipment = f.Equipment,
                            Status = f.Status,
                            CampusName = c.CampusName,
                            TypeName = t.TypeName
                        }).ToList();
            return list;
        }
        public FacilityDetailResponse GetFacilityDetail(int facilityId)
        {
            var result = _context.Facilities
                .Where(f => f.FacilityId == facilityId)
                .Select(f => new FacilityDetailResponse
                {
                    // ===== Facility =====
                    FacilityId = f.FacilityId,
                    FacilityCode = f.FacilityCode,
                    Capacity = f.Capacity,
                    Floor = f.Floor,
                    Equipment = f.Equipment,
                    Status = f.Status,

                    // ===== Campus =====
                    CampusId = f.Campus.CampusId,
                    CampusName = f.Campus.CampusName,
                    CampusAddress = f.Campus.Address,

                    // ===== Facility Type =====
                    TypeId = f.Type.TypeId,
                    TypeName = f.Type.TypeName,
                    TypeDescription = f.Type.Description,

                    Slots = _context.Slots
                            .FromSqlRaw(@"
                                SELECT s.* 
                                FROM Slot s  -- Đổi từ Slots thành Slot
                                INNER JOIN Facility_Slot fs ON s.SlotId = fs.SlotId
                                WHERE fs.FacilityId = {0}
                            ", facilityId)
                            .OrderBy(s => s.SlotNumber)
                            .Select(s => new SlotItem
                            {
                                SlotId = s.SlotId,
                                SlotNumber = s.SlotNumber,
                                StartTime = s.StartTime,
                                EndTime = s.EndTime
                            })
                            .ToList(),

                    // ===== Feedback Summary =====
                    AverageRating = f.Feedbacks.Any()
                                    ? Convert.ToDecimal(Math.Round(f.Feedbacks.Average(x => (double?)x.Rating) ?? 0, 2))
                                    : null,
                    TotalFeedback = f.Feedbacks.Count(),

                    RecentFeedback = f.Feedbacks
                        .OrderByDescending(x => x.CreateAt)
                        .Take(5)
                        .Select(x => new FeedbackItem
                        {
                            UserName = x.User.FullName,
                            Rating = x.Rating,
                            Comment = x.Comment,
                            CreateAt = x.CreateAt
                        })
                        .ToList()
                })
                .FirstOrDefault();

            return result;
        }
    }
}
