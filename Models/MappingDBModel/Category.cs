using System;
using System.Collections.Generic;

namespace BookNest.Models.MappingDBModel;

public partial class Category
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? ParentCategoryId { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();

    public virtual ParentCategory? ParentCategory { get; set; }
}
