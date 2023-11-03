// Copyright (c) 2023 CG Shared Services, LLC
// File: LCH.Services..GuestBook.Dedup.WindowsService.cs
// ---------------------------------------------------------------------------------------------------
// Modifications:
// Date:                                       Name:                                  Description:

using System.Threading;
using System.Threading.Tasks;
using LCH.Services.GuestBook.Dedup.Helpers;
using LCH.Services.GuestBook.Dedup.Services;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace LCH.Services.GuestBook.Dedup;

public class WindowsService : BackgroundService
{
    private readonly IGuestBookDupPoller guestBookDupPoller;
    private readonly IHeartBeatService heartBeatService;

    public WindowsService(IGuestBookDupPoller guestBookDupPoller,
        IHeartBeatService heartBeatService)
    {
        this.guestBookDupPoller = guestBookDupPoller;
        this.heartBeatService = heartBeatService;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);
        Log.Information($"{Constants.WindowsServiceName} Starting");
        await this.guestBookDupPoller.StartServiceAsync();
        this.heartBeatService.StartHeartBeat();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        Log.Information($"{Constants.WindowsServiceName} Shutting down");
        await this.guestBookDupPoller.StopServiceAsync();
        this.heartBeatService.StopHeartBeat();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Keep following boiler plate
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Run(() =>
                {
                }
                , stoppingToken);
            await Task.Delay(250, stoppingToken);
        }
    }
}