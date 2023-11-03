// Copyright (c) 2023 CG Shared Services, LLC
// File: LCH.Services..GuestBook.Dedup.BaseDao.cs
// ---------------------------------------------------------------------------------------------------
// Modifications:
// Date:                                       Name:                                  Description:

using LCH.Services.Models.Configuration;
using LCH.Services.Models.Session;

namespace LCH.Services.GuestBook.Dedup.DataAccess;

public class BaseDao
{
    protected BaseDao(IConnectionStrings connectionStrings, IRequestMetadata requestMetadata)
    {
        this.ConnectionStrings = connectionStrings;
        this.RequestMetadata = requestMetadata;
    }

    protected IRequestMetadata RequestMetadata { get; }

    protected IConnectionStrings ConnectionStrings { get; }
}