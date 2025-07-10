﻿using System;
using System.Collections.Generic;

namespace EbookStore.API.Models;

public partial class Review
{
    public int ReviewId { get; set; }

    public int? UserId { get; set; }

    public int? EbookId { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Ebook? Ebook { get; set; }

    public virtual User? User { get; set; }
}
