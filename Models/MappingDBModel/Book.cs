using System;
using System.Collections.Generic;

namespace BookNest.Models.MappingDBModel;

public partial class Book
{
    public int Id { get; set; }

    public string? BookName { get; set; }

    public string? Isbn { get; set; }

    public string? Cover { get; set; }

    public decimal? ImportPrice { get; set; }

    public decimal? FirstPrice { get; set; }

    public decimal? SecondPrice { get; set; }

    public string? ImageUrl { get; set; }

    public string? Author { get; set; }

    public string? Description { get; set; }

    public int? YearPublished { get; set; }

    public decimal? Rating { get; set; }

    public int? Pages { get; set; }

    public int? CategoryId { get; set; }

    public int? PublisherId { get; set; }

    public int? Quantity { get; set; }

    public virtual ICollection<BookComment> BookComments { get; set; } = new List<BookComment>();

    public virtual ICollection<BookRating> BookRatings { get; set; } = new List<BookRating>();

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Publisher? Publisher { get; set; }
}
