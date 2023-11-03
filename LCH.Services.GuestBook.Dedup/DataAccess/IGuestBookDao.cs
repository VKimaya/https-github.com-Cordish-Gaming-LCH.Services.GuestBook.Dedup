// Copyright (c) 2023 CG Shared Services, LLC
// File: LCH.Services..GuestBook.Dedup.IGuestBookDao.cs
// ---------------------------------------------------------------------------------------------------
// Modifications:
// Date:                                       Name:                                  Description:

using System.Threading.Tasks;

namespace LCH.Services.GuestBook.Dedup.DataAccess;

public interface IGuestBookDao
{
    Task<int> DedupGuestBook();
}