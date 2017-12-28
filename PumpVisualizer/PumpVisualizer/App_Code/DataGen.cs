using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using PumpDb;

namespace PumpVisualizer
{
    public class DataGen
    {
        public void GenerateData(string Identity, DateTime start, double electric, double water, int countMax)
        {
            ExternalRepository repogen=new ExternalRepository(ConfigurationManager.AppSettings["dbPath"]);
            Random rnd=new Random(DateTime.Now.Second);
            for (int i = 0; i < countMax; i++)
            {
                repogen.InsertNewRow(start.AddSeconds(i*5), Identity, electric + i, rnd.NextDouble(),rnd.NextDouble(),rnd.NextDouble(),
                    rnd.NextDouble()*10,rnd.NextDouble()*10,rnd.NextDouble()*10, 3, water + i,50,1367,78, "");
            }
        }
    }
}