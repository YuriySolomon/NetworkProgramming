using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkProgramming.Models
{
    internal class AssetData
    {
        public List<AssetModel> data { get; set; }
        public Int64 timestamp { get; set; }
    }
    /*Модель-оболочка от сайта https://api.coincap.io/v2/assets/bitcoin/history?interval=d1
     * {
     *      data: [... AssetModel ...],
     *      timestamp: 1669852800000
     */
    public class AssetModel
    {
        public string id { get; set; }
        public string rank { get; set; }
        public string symbol { get; set; }
        public string name { get; set; }
        public string supply { get; set; }
        public string maxSupply { get; set; }
        public string marketCapUsd { get; set; }
        public string volumeUsd24Hr { get; set; }
        public string priceUsd { get; set; }
        public string changePercent24Hr { get; set; }
        public string vwap24Hr { get; set; }
        public string explorer { get; set; }
    }
}
