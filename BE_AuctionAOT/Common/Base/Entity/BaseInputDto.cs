namespace BE_AuctionAOT.Common.Base.Entity
{
    public class BaseInputDto
    {
        public Pagination? DisplayCount { get; set; }
    }

    public class Pagination
    {
        public int? DisplayCount { get; set; }
        public int? PageCount { get; set; }
        public List<SortSetting>? SortSettings { get; set; }
    }
    public class SortSetting 
    {
        public string? Key { get; set; }
        public string? Direction { get; set; }
    }
}
