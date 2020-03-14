using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrmChartDemo.Charting
{
    public class Chart
    {
        public ChartArea ChartArea { get; set; }
        public List<Series> Series { get; set; }
        public double MaxY { get; set; }
        public double MinY { get; set; }
        private Image mBmp;
        private int mWidth;
        private int mHeight;
        private int mRelWidth;
        private int mRelHeight;
        private int mLeft = 30;
        private int mRight = 15;
        private int mTop = 15;
        private int mBottom = 30;

        public Chart(int mWidth, int mHeight)
        {
            if (mWidth < (mLeft + mRight)) mWidth = mLeft + mRight;
            if (mHeight < (mTop + mBottom)) mHeight = mTop + mBottom;

            this.mWidth = mWidth;
            this.mHeight = mHeight;
            this.mRelHeight = mHeight - mTop - mBottom;
            this.mRelWidth = mWidth - mLeft - mRight;

            MaxY = double.MaxValue;
            MinY = double.MinValue;
            this.ChartArea = new ChartArea();
            this.Series = new List<Series>();
            SaveChange();
        }

        public void SaveChange()
        {
            mBmp = new Bitmap(mWidth, mHeight);
            using (var g = Graphics.FromImage(mBmp))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.Clear(ChartArea.BackColor);
                g.Save();

                if (Series.Count > 0 && Series[0].Points.Count > 0)
                {
                    if (MaxY == double.MaxValue) MaxY = Series[0].Points.Max(a => a.Y);
                    if (MinY == double.MinValue) MinY = Series[0].Points.Min(a => a.Y);
                }
                if (MaxY == double.MaxValue) MaxY = -20;
                if (MinY == double.MinValue) MinY = -120;
                //画框线     
                Pen pen = new Pen(ChartArea.BorderColor, 1);
                int cy =GetYCoor(MaxY);
                int cx = GetXCoor(0);
                g.DrawRectangle(pen,cx, cy, mRelWidth, mRelHeight);
                g.Save();

                //画刻度
                pen = new Pen(ChartArea.AxisY.LineColor, 1);
                int perHeight = mRelHeight / ChartArea.AxisY.GridCount;
                for (int i = 1; i < ChartArea.AxisY.GridCount; i++)
                {
                    g.DrawLine(pen, mLeft, mRelHeight + mTop - i * perHeight, mLeft + mRelWidth, mRelHeight + mTop - i * perHeight);
                }
                double perY = (MaxY - MinY) / ChartArea.AxisY.GridCount;
                Font fontGrid = new Font(new FontFamily("宋体"), 8);
                for (int i = 0; i <= ChartArea.AxisY.GridCount; i++)
                {
                    string str = ((int)(MinY + perY * i)).ToString();
                    g.DrawString(str, fontGrid, Brushes.Black, 2, mRelHeight + mTop - i * perHeight - 5);
                }
                int perWidth = mRelWidth / ChartArea.AxisX.GridCount;
                pen = new Pen(ChartArea.AxisX.LineColor, 1);
                for (int j = 1; j < ChartArea.AxisX.GridCount; j++)
                {
                    g.DrawLine(pen, mLeft + perWidth * j, mRelHeight + mTop, mLeft + perWidth * j, mTop);
                }
                g.Save();

                if (Series.Count <= 0 || Series[0].Points.Count == 0) return;
                double maxX = Series[0].Points.Max(a => a.X);
                double minX = Series[0].Points.Min(a => a.X);
                double perx = (maxX - minX) / ChartArea.AxisX.GridCount;
                for (int i = 0; i <= ChartArea.AxisX.GridCount; i++)
                {
                    int value = (int)(minX + perx * i);
                    int left = mLeft + perWidth * i - 8;
                    if (i == ChartArea.AxisX.GridCount)
                    {
                        value = (int)maxX;
                        left = left - 10;
                    }
                    string str = value.ToString();

                    g.DrawString(str, fontGrid, Brushes.Black, left, mRelHeight + mTop + 5);
                }


                foreach (var ser in Series)
                {
                    Point[] pts = new Point[ser.Points.Count];
                    if (ser.ChartType == SeriesChartType.Line)
                    {
                        for (int i = 0; i < ser.Points.Count; i++)
                        {
                            double x = ser.Points[i].X;
                            double y = ser.Points[i].Y;
                            pts[i].X = GetXCoor(i);
                            pts[i].Y = GetYCoor(y);
                        }
                        pen = new Pen(ser.Color, 1);
                       if(pts.Length>1)  g.DrawLines(pen, pts);
                    }
                    if (ser.ChartType == SeriesChartType.Column)
                    {
                        int colWidth = 10;
                        Rectangle[] recs = new Rectangle[ser.Points.Count];
                        for (int i = 0; i < ser.Points.Count; i++)
                        {
                            double x = ser.Points[i].X;
                            double y = ser.Points[i].Y;
                            pts[i].X = GetXCoor(i);
                            pts[i].Y = GetYCoor(y);
                            if(i== ser.Points.Count - 1)
                            {
                                pts[i].X = pts[i].X - colWidth;
                            }
                            int height = mRelHeight - pts[i].Y + mTop;
                            if (height < 0) height = 0;
                            Rectangle r = new Rectangle(pts[i].X, pts[i].Y, colWidth, height);
                            recs[i] = r;
                        }
                        pen = new Pen(ser.Color, 1);
                        Brush brush = Brushes.Blue;
                    
                        g.FillRectangles(brush, recs);
                    }


                }
                g.Save();


            }
        }
        public int GetXCoor(int index)
        {
            int pointCount = 0;
            if (Series.Count > 0) pointCount = this.Series[0].Points.Count;
            if (pointCount<2) return mLeft;
            double pos = (double)index / (double)(pointCount - 1);
            pos = pos * mRelWidth + mLeft;
            return (int)Math.Floor(pos);
        }

        public int GetYCoor(double y)
        {
            double pos = (y - MinY) / (MaxY - MinY);
            pos = (mRelHeight - pos * mRelHeight) + mTop;
            return (int)Math.Floor(pos);
        }
        public Image ToImage()
        {
            return mBmp;
        }

        public void SaveImage(string path, System.Drawing.Imaging.ImageFormat format)
        {
            SaveChange();
            if (mBmp == null) return;
            mBmp.Save(path, format);
        }
    }
}
