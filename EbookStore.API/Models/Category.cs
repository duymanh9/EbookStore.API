using System;
using System.Collections.Generic;

namespace EbookStore.API.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public virtual ICollection<Ebook> Ebooks { get; set; } = new List<Ebook>();
}
