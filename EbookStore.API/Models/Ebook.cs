using System;
using System.Collections.Generic;

namespace EbookStore.API.Models;

public partial class Ebook
{
    public int EbookId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Author { get; set; }

    public string? FileUrl { get; set; }

    public string? FileType { get; set; }

    public decimal? Price { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CategoryId { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
