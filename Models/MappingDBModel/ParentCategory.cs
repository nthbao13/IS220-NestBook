using System;
using System.Collections.Generic;

namespace BookNest.Models.MappingDBModel;

public partial class ParentCategory
{
    public int Id { get; set; }

    public string? Type { get; set; }

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}
