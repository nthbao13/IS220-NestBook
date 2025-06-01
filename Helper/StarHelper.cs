namespace BookNest.Helper
{
    public static class StarHelper
    {
        public static string RenderStars(int rating)
        {
            var html = "";
            for (int i = 1; i <= 5; i++)
            {
                if (i <= rating)
                {
                    html += "<i class='fas fa-star text-warning'></i>";
                }
                else
                {
                    html += "<i class='far fa-star text-warning'></i>";
                }
            }
            return html;
        }
    }
}
