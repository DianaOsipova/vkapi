using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DrawingCore;
using System.DrawingCore.Text;
namespace vkapi
{
    public class DrawingChart
    {
        public static int Width { get; set; } = 500;
        public static int Height { get; set; } = 300;
        public static int Margin { get; set; } = 20;

        /// <summary>
        /// grawing chart
        /// </summary>
        /// <returns>pathToImage</returns>
        public static string DrawChart(Dictionary<string, string> probabilities, string groupId)
        {
            Console.WriteLine("Рисую");
            float scaleX = (float)(Width - Margin * 2) / probabilities.Count;
            float scaleY = (float)(Height - Margin * 2) / 100; 

            Bitmap bmp = new Bitmap(Width, Height);

            Graphics g = Graphics.FromImage(bmp);
            g.PageUnit = GraphicsUnit.Pixel;
            g.Clear(Color.White);
            Pen blackPen = new Pen(Color.Black, 0.5f);

            //внешняя рамочка
            g.DrawLine(new Pen(Color.Black, 4f), new Point(0, 0), new Point(0, Height));
            g.DrawLine(new Pen(Color.Black, 4f), new Point(0, 0), new Point(Width, 0));
            g.DrawLine(new Pen(Color.Black, 4f), new Point(Width, 0), new Point(Width, Height));
            g.DrawLine(new Pen(Color.Black, 4f), new Point(0, Height), new Point(Width, Height));


            //внутренняя рамочка
            g.DrawLine(new Pen(Color.Black, 1f), new Point(Margin, Margin), new Point(Margin, Height - Margin));
            g.DrawLine(new Pen(Color.Black, 1f), new Point(Margin, Margin), new Point(Width - Margin, Margin));
            g.DrawLine(new Pen(Color.Black, 1f), new Point(Width - Margin, Margin), new Point(Width - Margin, Height - Margin));
            g.DrawLine(new Pen(Color.Black, 1f), new Point(Margin, Height - Margin), new Point(Width - Margin, Height - Margin));

            //рисуем деления
            //по y
            for (int i = 0; i <= 100; i += 5)
            {
                g.DrawString($"{i / 100f}", new Font(new FontFamily(GenericFontFamilies.SansSerif), 5f),
                   Brushes.Black, new PointF(1, ys(scaleY, (float)i)));
            }
            //по x
            for (int i = 0; i <= probabilities.Count; i += 5)
            {
                g.DrawString($"{i}", new Font(new FontFamily(GenericFontFamilies.SansSerif), 5f),
                   Brushes.Black, new PointF(xs(scaleX, (float)i), Height - 10));
            }

            PointF[] points = new PointF[probabilities.Count];
            
            for (int i = 0; i < probabilities.Count; i++)
            {
                var value = probabilities.ElementAt(i);
                points[i] =new PointF(xs(scaleX, float.Parse(value.Key)), ys(scaleY, float.Parse(value.Value) * 100));
            
            }
            g.DrawLines(new Pen(Color.Goldenrod, 2f), points);


            Directory.CreateDirectory(@".\pict");
            bmp.Save(@$".\pict\{groupId}.png");
            Console.WriteLine("Всё успешно нарисовано");
            return @".\pict";
        }

        public static float xs(float scale, float x)
        {
            return Margin + (float)((x - 2) * scale);
        }

        public static float ys(float scale, float y)
        {
            return (Height - Margin) - (float)(y * scale);
        }
    }
}
