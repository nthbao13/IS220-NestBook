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

        // Overload để hiển thị sao với decimal (có nửa sao)
        public static string RenderStars(decimal rating)
        {
            var html = "";
            for (int i = 1; i <= 5; i++)
            {
                if (i <= Math.Floor(rating))
                {
                    // Sao đầy
                    html += "<i class='fas fa-star text-warning'></i>";
                }
                else if (i == Math.Ceiling(rating) && rating % 1 != 0)
                {
                    // Nửa sao
                    html += "<i class='fas fa-star-half-alt text-warning'></i>";
                }
                else
                {
                    // Sao trống
                    html += "<i class='far fa-star text-warning'></i>";
                }
            }
            return html;
        }
    }
}