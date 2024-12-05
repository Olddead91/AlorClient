﻿using BonadSharp.OpenApi.Core.Data;
using BonadSharp.OpenApi.Core.Instruments;
using BondSharp.OpenApi.Alor.Common;
using BondSharp.OpenApi.Alor.Data;
using BondSharp.OpenApi.Core.AbstractServices;
using Microsoft.AspNetCore.Http.Extensions;

namespace BondSharp.OpenApi.Alor.Deals;
internal class DealsProvider(ApiClient apiClient) : IDealsProvider
{
    public async IAsyncEnumerable<IDeal> GetPast(IInstrument instrument, DateTimeOffset? from, DateTimeOffset? to, int batchSize)
    {
        var page = 0;
        while (true)
        {
            var queryBuilder = new QueryBuilder();            
            queryBuilder.Add("format", "Slim");
            queryBuilder.Add("offset", (batchSize * page).ToString());
            queryBuilder.Add("limit",batchSize.ToString());
            if (from.HasValue)
            {
                queryBuilder.Add("from", from.Value.ToUnixTimeSeconds().ToString());
            }
            if (to.HasValue)
            {
                queryBuilder.Add("to", to.Value.ToUnixTimeSeconds().ToString());
            }
            var dealhistory = await apiClient.Get<DealsHistory>($"md/v2/Securities/MOEX/{instrument.Symbol}/alltrades/history", queryBuilder);
            foreach (var deal in dealhistory.List)
            {
                yield return deal;
            }

            if(dealhistory.List.Count < batchSize)
            {
                break;
            }
            page++;
        }
    }

    public async IAsyncEnumerable<IDeal> GetToday(IInstrument instrument, IDeal? withDeal, bool descending, int batchSize)
    {
        int page = 0;
        while (true)
        {
            var queryBuilder = new QueryBuilder();
            queryBuilder.Add("format", "Slim");
            if (withDeal != null)
            {
                queryBuilder.Add("fromId", withDeal.Id.ToString());
            }
            queryBuilder.Add("take", batchSize.ToString());
            queryBuilder.Add("offset", (batchSize * page).ToString());
            queryBuilder.Add("descending", descending.ToString());
            var counter = 0;
            await foreach (var deal in apiClient.GetValues<Deal>($"md/v2/Securities/MOEX/{instrument.Symbol}/alltrades", queryBuilder))
            {
                counter++;
                yield return deal;
            }

            if (counter < batchSize)
            {
                break;
            }
            page++;
        }

    }
}
