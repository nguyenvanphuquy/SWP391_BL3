namespace SWP391_BL3.Models.DTOs.Response
{
    public class ListTypeResponse
    {
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public string TypeDescription { get; set; }
        public int FacilitiCount { get; set; }
        public DateTime? CreateAt { get; set; }

    }
}
