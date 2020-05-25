using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Laba5
{
    public partial class Form1 : Form
    {
        private Bitmap bitmap;
        private Graphics graphics;
        private Rectangle clipRec;
        private Rectangle rec;

        public Form1()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private List<int> Parse()
        {
            List<int> config = new List<int>();
            foreach (string part in textBox1.Text.Replace('\n', ' ').
                Replace('\r', ' ').Replace('\t', ' ').Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                int value;
                if (!int.TryParse(part, out value))
                {
                    MessageBox.Show("Unable to parse " + part + " as integer!", "Error!");
                    return null;
                }

                config.Add(value);
            }

            return config;
        }

        private Rectangle ParseRec(List<int> config)
        {
            int x1 = config[config.Count - 4];
            int y1 = config[config.Count - 3];
            int x2 = config[config.Count - 2];
            int y2 = config[config.Count - 1];

            x1 = Math.Min(x1, x2);
            y1 = Math.Min(y1, y2);
            return new Rectangle(x1, y1, x2 - x1, y2 - y1);
        }


        public void Start(Rectangle clippingRect)
        {
            this.clipRec = clippingRect;
            rec = new Rectangle(clippingRect.X - this.clipRec.Width / 2,
                clippingRect.Y - this.clipRec.Height / 2,
                this.clipRec.Width * 2, this.clipRec.Height * 2);

            bitmap = new Bitmap(rec.Width, rec.Height);
            graphics = Graphics.FromImage(bitmap);

            graphics.FillRectangle(new SolidBrush(Color.WhiteSmoke), new Rectangle(0, 0, rec.Width, rec.Height));
            int gridXStep = rec.Width / 10;
            int gridYStep = rec.Height / 10;

            for (int x = 0; x < rec.Width; x += gridXStep)
            {
                graphics.DrawLine(new Pen(Color.FromArgb(100, 100, 100)), x, 0, x, rec.Height);
                if (x != 0)
                {
                    graphics.DrawString(string.Format("{0}", x + rec.X), new Font("Arial", 16), new SolidBrush(Color.Black), x, 0);
                }
            }

            for (int y = 0; y < rec.Height; y += gridYStep)
            {
                graphics.DrawLine(new Pen(Color.FromArgb(100, 100, 100)), 0, y, rec.Width, y);
                if (y != 0)
                {
                    graphics.DrawString(string.Format("{0}", y + rec.Y), new Font("Arial", 16), new SolidBrush(Color.Black), 0, y);
                }
            }

            graphics.DrawRectangle(new Pen(Color.Blue, 3), new Rectangle(clippingRect.X - rec.X,
                clippingRect.Y - rec.Y, clippingRect.Width, clippingRect.Height));
        }

        public void DrawLine(int x0, int y0, int x1, int y1)
        {
            graphics.DrawLine(new Pen(Color.Red, 2), x0 - rec.X, y0 - rec.Y,
                x1 - rec.X, y1 - rec.Y);
            MedianLine(x0, y0, x1, y1);
        }

        private void MedianLine(float x0, float y0, float x1, float y1)
        {
            float dx = Math.Abs(x1 - x0);
            float dy = Math.Abs(y1 - y0);

            if (Math.Sqrt(Math.Pow(x1 - x0, 2) + Math.Pow(y1 - y0, 2)) <= 1.0f)
            {
                return;
            }

            byte code0 = Place(x0, y0);
            byte code1 = Place(x1, y1);

            if ((code0 & code1) != 0)
            {
                return;
            }

            if ((code0 | code1) == 0)
            {
                graphics.DrawLine(new Pen(Color.Green, 2), x0 - rec.X, y0 - rec.Y,
                    x1 - rec.X, y1 - rec.Y);
                return;
            }

            float mx = x0 + (x1 - x0) / 2;
            float my = y0 + (y1 - y0) / 2;

            MedianLine(x0, y0, mx, my);
            MedianLine(mx, my, x1, y1);
        }

        private byte Place(float x, float y)
        {
            byte code = 0;
            if (y > clipRec.Y + clipRec.Height) code |= 1;
            if (x > clipRec.X + clipRec.Width) code |= 1 << 1;
            if (y < clipRec.Y) code |= 1 << 2;
            if (x < clipRec.X) code |= 1 << 3;
            return code;
        }

      

        private void button1_Click(object sender, EventArgs e)
        {
            List<int> config = Parse();
            if (config == null)
            {
                return;
            }

            if (config.Count % 4 != 0 || config.Count < 8)
            {
                MessageBox.Show(@"Неверынй формат входных данных, пример входных данных: 
X1_1 Y1_1 X2_1 Y2_1
X1_2 Y1_2 X2_2 Y2_2
…
X1_n Y1_n X2_n Y2_n *координаты отрезков*
Xmin Ymin Xmax Ymax *координаты отсекающего прямоугольного окна* ");
                return;
            }

            Start(ParseRec(config));
            int lineStartIndex = 0;

            while (lineStartIndex < config.Count - 4)
            {
                DrawLine(config[lineStartIndex], config[lineStartIndex + 1],
                    config[lineStartIndex + 2], config[lineStartIndex + 3]);
                lineStartIndex += 4;
            }

            graphics.Dispose();
            pictureBox1.Image = bitmap;
            pictureBox1.Invalidate();
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;

            string filename = openFileDialog1.FileName;

            string fileText = System.IO.File.ReadAllText(filename);
            textBox1.Text = fileText;
        }
    }
}
