using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts;

public class NotificationCreatedResponse
{
    public string Id { get; set; } = default!;
    public string MatchId { get; set; } = default!;
}