using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkProgramming.Models
{
    internal class HistoryData
    {
        public List<HistoryModels> data { get; set; }
        public Int64 timestamp { get; set; }
    }
    /*Модель-оболочка от сайта https://api.coincap.io/v2/assets/bitcoin/history?interval=d1
     * {
     *      data: [... HistoryModel ...],
     *      timestamp: 1669852800000
     */
    internal class HistoryModels
    {
        public String priceUsd { get; set; }
        public Int64 time { get; set; }
        public String date { get; set; }

        public Double price => Convert.ToDouble(priceUsd,
            CultureInfo.InvariantCulture); // на "наших" ОС вместо "." берет ","
    }
}
/* Модель для данных 
* {
      "priceUsd": "17068.7558958508773389",
      "time": 1669852800000,
      "date": "2022-12-01T00:00:00.000Z"
    },
*/