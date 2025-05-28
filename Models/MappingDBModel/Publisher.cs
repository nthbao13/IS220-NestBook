using System;
using System.Collections.Generic;

namespace BookNest.Models.MappingDBModel;

public partial class Publisher
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
