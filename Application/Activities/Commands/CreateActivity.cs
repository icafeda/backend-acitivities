using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Activities.Queries;
using Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Activities.Commands;

public class CreateActivity
{
    public class Command : IRequest<string>
    {
        public required Activity Activity { get; set; }
    }

    public class Handler(AppDbContext context, ILogger<CreateActivity> logger) : IRequestHandler<Command, string>
    {
        public async Task<string> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                //1. LogInformation - log thông tin bình thường
                logger.LogInformation("Editing activity started");

                //2. LogWarning - log cảnh báo dữ liệu bất thường
                if (string.IsNullOrWhiteSpace(request.Activity.Title))
                {
                    logger.LogWarning("Activity title is empty");
                }
                context.Activities.Add(request.Activity);
                await context.SaveChangesAsync(cancellationToken);

                // LogInformation — thành công
                logger.LogInformation($"Activity created successfully with Id = {request.Activity.Id}");

                return request.Activity.Id;
            }
            catch (System.Exception ex)
            {

                // 3) LogError — lỗi nghiêm trọng
                logger.LogError(ex, "Error occurred while creating new activity");

                throw; // vẫn throw để API trả lỗi đúng
            }
            
        }
    }
}
