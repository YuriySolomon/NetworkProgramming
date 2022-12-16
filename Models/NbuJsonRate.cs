using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkProgramming.Models
{
    // модель для JSON данных курсов валют от НБУ (https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?json)
    internal class NbuJsonRate
    {
        public Int16 r030 { get; set; } //sort
        public String  txt { get; set; }
        public Single rate { get; set; } // float
        public String cc { get; set; }
        public String exchangedate { get; set; }
    }
}
/*
   {
    "r030":36,							 
    "txt":"Австралійський долар",		 
    "rate":24.9416,						
    "cc":"AUD",							
    "exchangedate":"05.12.2022"			
    },	
 */
