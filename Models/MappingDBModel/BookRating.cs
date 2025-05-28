using System;
using System.Collections.Generic;

namespace BookNest.Models.MappingDBModel;

public partial class BookRating
{
    public int Id { get; set; }

    public int? BookId { get; set; }

    public int? UserId { get; set; }

    public int? Rating { get; set; }

    public DateTime? CreateAt { get; set; }

    public virtual Book? Book { get; set; }
}
