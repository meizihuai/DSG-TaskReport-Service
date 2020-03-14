using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrmChartDemo.Charting
{
    public class Axis
    {
        public Color MajorGridColor { get; set; }
        public Color LineColor { get; set; }
        public int GridCount { get; set; }
        public Axis()
        {
            MajorGridColor = Color.Black;
            LineColor = Color.Gray;
            GridCount = 5;
        }
    }
}
