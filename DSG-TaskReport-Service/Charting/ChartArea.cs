using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrmChartDemo.Charting
{
   public class ChartArea
    {
        public Color BorderColor { get; set; }
        public Color BackColor { get; set; }
        public Axis AxisX { get; set; }
        public Axis AxisY { get; set; }
        public ChartArea()
        {
            this.BorderColor = Color.Black;
            this.BackColor = Color.White;
            AxisX = new Axis();
            AxisY = new Axis();
        }
    }
}
