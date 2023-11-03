// Copyright (c) 2023 CG Shared Services, LLC
// File: LCH.Services..GuestBook.Dedup.GuestBookDao.cs
// ---------------------------------------------------------------------------------------------------
// Modifications:
// Date:                                       Name:                                  Description:

using System;
using System.Data;
using System.Threading.Tasks;
using LCH.Services.GuestBook.Dedup.Helpers;
using LCH.Services.Models.Configuration;
using LCH.Services.Models.Session;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Serilog;

namespace LCH.Services.GuestBook.Dedup.DataAccess;

public class GuestBookDao : BaseDao, IGuestBookDao
{
    public GuestBookDao(IOptions<ConnectionStrings> options, IRequestMetadata requestMetadata)
        : base(options.Value, requestMetadata)
    {
    }

    public async Task<int> DedupGuestBook()
    {
        SqlTransaction sqlTransaction;

        await using SqlConnection con = new(this.ConnectionStrings.Guest);
        await con.OpenAsync();

        sqlTransaction = con.BeginTransaction();
        await using SqlCommand cmd = new(Constants.ProcDedupGuestRecords, con)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.Add("@DeletedRecords", SqlDbType.Int);
        cmd.Parameters["@DeletedRecords"].Direction = ParameterDirection.Output;

        cmd.Transaction = sqlTransaction;
        try
        {
            _ = await cmd.ExecuteNonQueryAsync();

            int deleted = Convert.ToInt32(cmd.Parameters["@DeletedRecords"].Value);
            sqlTransaction.Commit();

            return deleted;
        }
        catch (Exception e)
        {
            sqlTransaction.Rollback();
            Log.Error(e, e.Message);
            throw;
        }
    }
}