using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using BE_AuctionOT_Cronjob.Modelss;
using BE_AuctionOT_Cronjob.RabbitMQ.BidQueue.Publishers;
using BE_AuctionOT_Cronjob.Job;
using BE_AuctionOT_Cronjob.Common.Utility;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddDebug();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<DB_AuctionAOTContext>(options =>
            options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IBidPublisher, BidPublisher>(); 
        services.AddScoped<Mail>();

        // Đăng ký IMemoryCache
        services.AddMemoryCache();

        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();

            var jobKey = new JobKey("AutoBidJob");
            q.AddJob<AutoBidJob>(opts => opts.WithIdentity(jobKey));
            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("AutoBidJob-Trigger")
                .WithCronSchedule("0/10 * * * * ?"));

            var updateStatusAucJobKey = new JobKey("UpdateStatusAuc");
            q.AddJob<UpdateStatusAucJob>(opts => opts.WithIdentity(updateStatusAucJobKey));
            q.AddTrigger(opts => opts
                .ForJob(updateStatusAucJobKey)
                .WithIdentity("UpdateStatusAuc-Trigger")
                .WithCronSchedule("0/5 * * * * ?"));
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });
    })
    .Build();
await host.RunAsync();
