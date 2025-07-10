using System;
using System.Collections.Generic;

namespace EbookStore.API.Models;

public partial class BlogPost
{
    public int PostId { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public string? Image { get; set; }

    public DateTime? CreatedAt { get; set; }
}
