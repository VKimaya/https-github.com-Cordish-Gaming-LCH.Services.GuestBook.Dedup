// Copyright (c) 2023 CG Shared Services, LLC
// File: LCH.Services..GuestBook.Dedup.Constants.cs
// ---------------------------------------------------------------------------------------------------
// Modifications:
// Date:                                       Name:                                  Description:

namespace LCH.Services.GuestBook.Dedup.Helpers;

public static class Constants
{
    public const string WindowsServiceName = "LCH.Services.GuestBook.Dedup";

    /* Sql and Procedures */
    public const string ProcDedupGuestRecords = "[dbo].[usp_DedupGuestRecords]";
}