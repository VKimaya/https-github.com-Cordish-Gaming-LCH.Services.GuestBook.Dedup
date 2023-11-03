// Copyright (c) 2023 CG Shared Services, LLC
// File: LCH.Services..GuestBook.Dedup.IHeartBeatService.cs
// ---------------------------------------------------------------------------------------------------
// Modifications:
// Date:                                       Name:                                  Description:

namespace LCH.Services.GuestBook.Dedup.Services;

public interface IHeartBeatService
{
    void StartHeartBeat();

    void StopHeartBeat();
}