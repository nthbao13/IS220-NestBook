namespace BookNest.Models.ViewModel
{
    public class RatingStatistics
    {
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }
        public int TotalCount { get; set; }

        public double FiveStarPercentage => TotalCount > 0 ? (double)FiveStarCount / TotalCount * 100 : 0;
        public double FourStarPercentage => TotalCount > 0 ? (double)FourStarCount / TotalCount * 100 : 0;
        public double ThreeStarPercentage => TotalCount > 0 ? (double)ThreeStarCount / TotalCount * 100 : 0;
        public double TwoStarPercentage => TotalCount > 0 ? (double)TwoStarCount / TotalCount * 100 : 0;
        public double OneStarPercentage => TotalCount > 0 ? (double)OneStarCount / TotalCount * 100 : 0;
    }
}
