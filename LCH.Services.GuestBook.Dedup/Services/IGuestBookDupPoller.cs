// Copyright (c) 2023 CG Shared Services, LLC
// File: LCH.Services..GuestBook.Dedup.IGuestBookDupPoller.cs
// ---------------------------------------------------------------------------------------------------
// Modifications:
// Date:                                       Name:                                  Description:

using System.Threading.Tasks;

namespace LCH.Services.GuestBook.Dedup.Services;

public interface IGuestBookDupPoller
{
    Task StartServiceAsync();

    Task StopServiceAsync();
}