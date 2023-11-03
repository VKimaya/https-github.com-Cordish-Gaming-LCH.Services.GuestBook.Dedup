// Copyright (c) 2023 CG Shared Services, LLC
// File: LCH.Services..GuestBook.Dedup.GuestBookDupPoller.cs
// ---------------------------------------------------------------------------------------------------
// Modifications:
// Date:                                       Name:                                  Description:

using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LCH.Services.GuestBook.Dedup.DataAccess;
using LCH.Services.GuestBook.Dedup.Helpers;
using LCH.Services.Models.Configuration;
using Microsoft.Extensions.Options;
using Serilog;

namespace LCH.Services.GuestBook.Dedup.Services;

public class GuestBookDupPoller : IGuestBookDupPoller
{
    public GuestBookDupPoller(IOptionsMonitor<AppSettings> appSettings
        , IGuestBookDao guestBookDao)
    {
        this.AppSettingsOptionsMonitor = appSettings;
        this.GuestBookDao = guestBookDao;
    }

    public Timer GuestBookPollingTimer { get; set; }
    private IGuestBookDao GuestBookDao { get; }
    private IOptionsMonitor<AppSettings> AppSettingsOptionsMonitor { get; }

    public async Task StartServiceAsync()
    {
        try
        {
            await Task.Yield();

            Log.Information("Starting the InMoment polling service");

            string? pollingTime = this.AppSettingsOptionsMonitor.CurrentValue
                .Settings.FirstOrDefault(static s =>
                    s.Name.Equals("PollingTime", StringComparison.InvariantCultureIgnoreCase))
                ?.Value;

            this.GuestBookPollingTimer = new Timer(this.GuestBookPollingTimerHandlerAsync
                , null
                , GetJobRunDelay(pollingTime)
                , new TimeSpan(24, 0, 0));
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message, ex);
            Log.Error($"Exception encountered in GuestBook polling service {Constants.WindowsServiceName}.");
        }
    }

    public Task StopServiceAsync()
    {
        throw new NotImplementedException();
    }

    private async void GuestBookPollingTimerHandlerAsync(object state)
    {
        Log.Information($"The 'GuestBook' polling timer has elapsed.{DateTime.Now}");
        await Task.Yield();

        try
        {
            //get updated inmoment data
            int deletedRecords =
                this.GuestBookDao.DedupGuestBook().Result;

            if (deletedRecords > 0)
            {
                Log.Information($"{deletedRecords} duplicate records have been deleted");
                return;
            }

            Log.Information("No duplicates found.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, ex.Message);
        }
    }

    private static TimeSpan GetJobRunDelay(string pollingTime)
    {
        TimeSpan scheduledParsedTime = GetScheduledParsedTime(pollingTime);
        TimeSpan currentTimeOfTheDay = TimeSpan.Parse(DateTime.Now.TimeOfDay.ToString("hh\\:mm"));
        TimeSpan delayTime = scheduledParsedTime >= currentTimeOfTheDay
            ? scheduledParsedTime - currentTimeOfTheDay
            : new TimeSpan(24, 0, 0) - currentTimeOfTheDay + scheduledParsedTime;
        return delayTime;
    }

    private static TimeSpan GetScheduledParsedTime(string pollingTime)
    {
        string[] formats =
        {
            @"hh\:mm\:ss", "hh\\:mm"
        };

        TimeSpan.TryParseExact(pollingTime, formats, CultureInfo.InvariantCulture, out TimeSpan scheduledTimespan);
        return scheduledTimespan;
    }
}