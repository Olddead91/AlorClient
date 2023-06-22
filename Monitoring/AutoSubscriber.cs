﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlorClient;
using Common;

namespace Monitoring
{
    internal class AutoSubscriber
    {
        private readonly ISubscriber subscriber;
        private readonly ISecurities securities;
        private readonly Configuration configuration;

        public AutoSubscriber(ISubscriber subscriber, ISecurities securities, Configuration configuration)
        {
            this.subscriber = subscriber;
            this.securities = securities;
            this.configuration = configuration;
        }

        public void Subscribe()
        {
            foreach (var security in GetSecurities())
            {
                subscriber.Subscribe(new DealsSubscription(security));
                subscriber.Subscribe(new DealsSubscription(security));
            }
        }

        private ISecurity[] GetSecurities()
        {
            var symbols = configuration
                .Symbols
                .Select(securities.GetAsync)
                .Select(async x => await x)
                .Select(x => x.Result);
            var futures = configuration.Futures
                .ToAsyncEnumerable()
                .SelectMany(securities.GetFuturesAsync)
                .ToEnumerable();
            var options = configuration.Options
                .ToAsyncEnumerable()
                .SelectMany(securities.GetOptionsAsync)
                .ToEnumerable();
                


            return symbols.Concat(futures).Concat(options).ToArray();
        }

    }
}