// Copyright (c) 2023 CG Shared Services, LLC
// File: LCH.Services..GuestBook.Dedup.HeartBeatService.cs
// ---------------------------------------------------------------------------------------------------
// Modifications:
// Date:                                       Name:                                  Description:

using System;
using System.Timers;
using LCH.Services.GuestBook.Dedup.Helpers;
using LCH.Services.Mq;
using LCH.Services.Mq.Configuration;
using LCH.Services.Mq.Models;
using Microsoft.Extensions.Options;
using Serilog;

namespace LCH.Services.GuestBook.Dedup.Services;

public class HeartBeatService : IHeartBeatService
{
    private readonly MqSettings mqSettings;
    private readonly IPublisher publisher;
    private Timer heartbeatTimer = new();

    public HeartBeatService(IOptions<MqSettings> mqSettings, IPublisher publisher)
    {
        this.mqSettings = mqSettings.Value;
        this.publisher = publisher;
    }

    public void StartHeartBeat()
    {
        try
        {
            Log.Information($"Starting heartbeat service for {Constants.WindowsServiceName}");

            HeartBeatMessage heartbeatMessage = new()
            {
                MessageType = Enums.HeartBeatMessageType.Register
                , Message = "Registering#Starting"
            };

            Log.Debug("Sending first heartbeat message");
            this.SendHeartbeatMessage(heartbeatMessage);

            this.heartbeatTimer = new Timer
            {
                AutoReset = true
                , Interval = 30000
            };

            this.heartbeatTimer.Elapsed += this.HeartbeatTimerElapsedHandler;
            this.heartbeatTimer.Enabled = true;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex.Message, ex);
            Log.Fatal(
                $"Fatal exception encountered while starting heartbeat service for {Constants.WindowsServiceName}");
        }
    }

    public void StopHeartBeat()
    {
        try
        {
            Log.Information($"Stopping heartbeat service for {Constants.WindowsServiceName}");

            HeartBeatMessage heartbeatMessage = new()
            {
                MessageType = Enums.HeartBeatMessageType.Stop
                , Message = "Stopping"
            };

            this.SendHeartbeatMessage(heartbeatMessage);
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message, ex);
            Log.Error(
                $"Exception encountered while shutting down the heartbeat service {Constants.WindowsServiceName}.");
        }
    }

    private void HeartbeatTimerElapsedHandler(object source, ElapsedEventArgs e)
    {
        HeartBeatMessage heartbeatMessage = new()
        {
            MessageType = Enums.HeartBeatMessageType.Good
            , Message = "Beating"
        };

        try
        {
            this.SendHeartbeatMessage(heartbeatMessage);
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message, ex);
        }
    }

    private void SendHeartbeatMessage(HeartBeatMessage heartbeatMessage)
    {
        MqMessage<HeartBeatMessage> heartbeatMqMessage = new()
        {
            Message = heartbeatMessage
            , Exchange = this.mqSettings.HeartbeatExchange
            , ServiceName = Constants.WindowsServiceName
        };

        this.publisher.PublishMqMessage(heartbeatMqMessage, null);
    }
}