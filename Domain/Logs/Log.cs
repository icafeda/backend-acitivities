using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Logs;

public class Log
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Level { get; set; } = string.Empty;      // Info, Warning, Error
    public string Message { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;       // /api/activities
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
