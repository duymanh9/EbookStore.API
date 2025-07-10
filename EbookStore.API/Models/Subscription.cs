using System;
using System.Collections.Generic;

namespace EbookStore.API.Models;

public partial class Subscription
{
    public int SubscriptionId { get; set; }

    public string? Email { get; set; }

    public DateTime? SubscribedAt { get; set; }
}
