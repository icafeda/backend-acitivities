using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Logs;
using Persistence;

namespace API.Middleware;

public class DatabaseLoggingMiddleware(RequestDelegate next)
{
   

    public async Task Invoke(HttpContext context, AppDbContext db)
    {
        var log = new Log
        {
            Level = "Information",
            Message = "Incoming request",
            Path = context.Request.Path,
            StatusCode = 0
        };

        await db.Logs.AddAsync(log);
        await db.SaveChangesAsync();

        await next(context);

        log.StatusCode = context.Response.StatusCode;
        log.Message = "Outgoing response";

        await db.SaveChangesAsync();
    }

}
