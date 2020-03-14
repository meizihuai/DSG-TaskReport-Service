using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrmChartDemo.Charting
{
  public  class ChartPoint
    {
        public ChartPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
        public ChartPoint() { }


        public double X { get; set; }
        public double Y { get; set; }

    }
}
