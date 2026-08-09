using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MediatR;
using Persistence;
using Domain;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Application.Activities.Commands;

public class EditActivity
{
    public class Command : IRequest
    {
        public required Activity Activity { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var activity = await context.Activities.FirstOrDefaultAsync(x => x.Id == request.Activity.Id, cancellationToken) 
            ?? throw new Exception("can not find activity");

            // Mapping từ request sang entity đã được track
            mapper.Map(request.Activity, activity);

            // Đảm bảo Id không bị đánh dấu là Modified
            context.Entry(activity).Property(x => x.Id).IsModified = false;

            await context.SaveChangesAsync(cancellationToken);
        
        }
    }
}
