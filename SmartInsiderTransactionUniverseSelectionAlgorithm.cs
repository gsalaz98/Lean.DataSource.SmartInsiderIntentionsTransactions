/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/

using System;
using System.Linq;
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.DataSource;

namespace QuantConnect.Algorithm.CSharp
{
    public class SmartInsiderTransactionUniverseAlgorithm : QCAlgorithm
    {
        public override void Initialize()
        {
            // Data ADDED via universe selection is added with Daily resolution.
            UniverseSettings.Resolution = Resolution.Daily;

            SetStartDate(2022, 2, 14);
            SetEndDate(2022, 2, 18);
            SetCash(100000);

            // add a custom universe data source (defaults to usa-equity)
            var universe = AddUniverse<SmartInsiderTransactionUniverse>(data =>
            {
                foreach (SmartInsiderTransactionUniverse datum in data)
                {
                    Log($"{datum.Symbol},{datum.Amount},{datum.MinimumExecutionPrice},{datum.MaximumExecutionPrice},{datum.USDValue},{datum.BuybackPercentage},{datum.VolumePercentage},{datum.USDMarketCap}");
                }

                // define our selection criteria
                return from SmartInsiderTransactionUniverse d in data
                       where d.BuybackPercentage > 0.005m && d.USDMarketCap > 100000000m
                       select d.Symbol;
            });

            var history = History(universe, 10).ToList();
            if (history.Count != 10)
            {
                throw new System.Exception($"Unexpected historical data count!");
            }
            foreach (var dataForDate in history)
            {
                var coarseData = dataForDate.ToList();
                if (coarseData.Count < 1)
                {
                    throw new System.Exception($"Unexpected historical universe data!");
                }
            }
        }

        public override void OnSecuritiesChanged(SecurityChanges changes)
        {
            Log(changes.ToString());
        }
    }
}