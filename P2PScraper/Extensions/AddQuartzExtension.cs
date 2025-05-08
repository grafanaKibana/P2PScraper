using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using P2PScraper.Jobs;
using Quartz;

namespace P2PScraper.Extensions;

using P2PScraper.Models;

public static class AddQuartzExtension
{
    public static IServiceCollection AddQuartz(this IServiceCollection services, IConfigurationSection config)
    {
        services.AddQuartz(q =>
        {
            q.ScheduleJob<SendTelegramMessageJob>(trigger =>
            {
                var jobInterval = config.GetValue<int>(nameof(AppConfig.NotificationIntervalInMinutes));

                trigger
                    .WithIdentity("JobTrigger")
                    //.StartNow()
                    .WithSimpleSchedule(b => b.WithIntervalInMinutes(jobInterval).RepeatForever());
            });
        });
        services.AddQuartzHostedService(opt =>
        {
            opt.WaitForJobsToComplete = true;
        });

        return services;
    }
}