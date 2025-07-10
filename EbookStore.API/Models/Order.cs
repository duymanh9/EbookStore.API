using System;
using System.Collections.Generic;

namespace EbookStore.API.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? UserId { get; set; }

    public int? EbookId { get; set; }

    public string? PaymentMethod { get; set; }

    public string? PaymentStatus { get; set; }

    public DateTime? PaidAt { get; set; }

    public virtual Ebook? Ebook { get; set; }

    public virtual User? User { get; set; }
}
