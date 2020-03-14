using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrmChartDemo.Charting
{
    public class Series
    {
        public List<ChartPoint> Points { get; set; }
        public SeriesChartType ChartType { get; set; }
        public Color Color { get; set; }
        public Series()
        {
            this.Points = new List<ChartPoint>();
            this.Color = Color.Black;
        }
       
    }
}
