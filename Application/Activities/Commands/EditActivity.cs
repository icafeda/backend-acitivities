using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MediatR;
using Persistence;
using Domain;
using AutoMapper;

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
            var activity = await context.Activities.FindAsync(request.Activity.Id, cancellationToken) ?? throw new Exception("can not find activity");

            // activity.Title = request.Activity.Title;
            mapper.Map(request.Activity, activity);
            
            await context.SaveChangesAsync(cancellationToken);
        
        }
    }
}
