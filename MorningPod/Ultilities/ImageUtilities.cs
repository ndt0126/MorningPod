using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MorningPod.Ultilities;
using Color = System.Drawing.Color;

namespace Ultilities
{
    public class ImageUtilities
    {
        public static string CreateResizedImage(string imgPath , int maxSize , string resizedFilePath) { return CreateResizedImage(new Uri(imgPath) , maxSize , resizedFilePath); }

        public static string CreateResizedImage(Uri imgUri , int maxSize , string resizedFilePath)
        {
            try
            {
                using (System.Drawing.Image orgImg = System.Drawing.Image.FromFile(imgUri.LocalPath))
                {
                    int orgW = orgImg.Width , orgH = orgImg.Height , newW = maxSize , newH = maxSize;

                    if (orgH >= orgW) newH = maxSize*(orgH/orgW);
                    else newW = maxSize*(orgW/orgH);

                    ImageUtilities.CreateResizedImage(new BitmapImage(imgUri) , newW , newH , 0 , resizedFilePath);

                    ImageUtilities.FindDominantColors(resizedFilePath);
                }
            }
            catch (Exception ex)
            {
                resizedFilePath = null;
                mt.Log(ex);
            }

            return resizedFilePath;
        }

        public static void CreateResizedImage(ImageSource source , int width , int height , int margin , string filePath)
        {
            var rect = new Rect(margin , margin , width - margin*2 , height - margin*2);

            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group , BitmapScalingMode.HighQuality);
            group.Children.Add(new ImageDrawing(source , rect));

            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen()) drawingContext.DrawDrawing(group);

            var resizedImage = new RenderTargetBitmap(width , height , // Resized dimensions
                96 , 96 , // Default DPI values
                PixelFormats.Default); // Default pixel format
            resizedImage.Render(drawingVisual);

            BitmapFrame bmFrame = BitmapFrame.Create(resizedImage);

            using (FileStream stream5 = new FileStream(filePath , FileMode.Create))
            {
                PngBitmapEncoder encoder5 = new PngBitmapEncoder();
                encoder5.Frames.Add(bmFrame);
                encoder5.Save(stream5);
            }
        }


        public static System.Windows.Media.Color[] FindDominantColors(string path)
        {
            Bitmap bm = new Bitmap(path);
            ColorX[] cols = new ColorX[9];

            cols[0] = new ColorX(bm.GetPixel((int) (bm.Width*0.2) , (int) (bm.Height*0.2)));
            cols[1] = new ColorX(bm.GetPixel((int) (bm.Width*0.8) , (int) (bm.Height*0.2)));
            cols[2] = new ColorX(bm.GetPixel((int) (bm.Width*0.2) , (int) (bm.Height*0.8)));
            cols[3] = new ColorX(bm.GetPixel((int) (bm.Width*0.8) , (int) (bm.Height*0.8)));

            cols[4] = new ColorX(bm.GetPixel((int) (bm.Width*0.4) , (int) (bm.Height*0.4)));
            cols[5] = new ColorX(bm.GetPixel((int) (bm.Width*0.6) , (int) (bm.Height*0.4)));
            cols[6] = new ColorX(bm.GetPixel((int) (bm.Width*0.4) , (int) (bm.Height*0.6)));
            cols[7] = new ColorX(bm.GetPixel((int) (bm.Width*0.6) , (int) (bm.Height*0.6)));

            cols[8] = new ColorX(bm.GetPixel((int) (bm.Width*0.5) , (int) (bm.Height*0.5)));

            bm.Dispose();
            
            
            //var aaa = cols.ToList();
            //aaa.Sort(SortColor);
            //return aaa;





            foreach(ColorX colA in cols)
            {
                //foreach(ColorX colB in cols)
                //{
                //    float g = ColorCompare(colA.col , colB.col)*0.001f;
                //    if(g < 0.008) { colA.count++; colB.count++; }
                //    g *= colA.col.GetHue()*colA.col.GetBrightness();
                //    g = (float) Math.Round(g);
                //    colA.grade += g; colB.grade += g;
                //}

                colA.count = (float) (colA.col.GetHue() * 0.1);
                colA.grade = /*colA.col.GetHue()*/ TotalColor(colA.col) * 0.1 * colA.col.GetBrightness() * colA.count;
                colA.grade = (float) Math.Round(colA.grade);

                colA.grade = TotalColor(colA.col);
            }

            
            //List<ColorX> LC = new List<ColorX>();

            //foreach (ColorX colA in cols)
            //{
            //    bool canAdd = true;
            //    //foreach (ColorX colL in LC) { if (Math.Abs(colA.grade - colL.grade) < 500) { canAdd = false; break; } }
            //    if (canAdd) LC.Add(colA);
            //}



            //LC.Sort(SortGrade);

            //System.Windows.Media.Color[] RS = new System.Windows.Media.Color[LC.Count];
            //for (int i = 0; i < LC.Count; i++)
            //{
            //    RS[i] = System.Windows.Media.Color.FromArgb(255 , LC[i].col.R , LC[i].col.G , LC[i].col.B);
            //}

            System.Windows.Media.Color[] RS = new System.Windows.Media.Color[cols.Length];
            for (int i = 0; i < cols.Length; i++)
            {
                RS[i] = System.Windows.Media.Color.FromArgb(255 , cols[i].col.R , cols[i].col.G , cols[i].col.B);
            }

            return RS;
        }

        private static int SortGrade(ColorX x , ColorX y) { return x.grade.CompareTo(y.grade); }
        private static int SortColor(Color colA , Color colB)
        {
            //return ColorCompare(colA, colB);
            int RS = ColorGrade(colA).CompareTo(ColorGrade(colB));
            return RS;
        }
        public static int ColorGrade(Color col)
        {
            return Math.Abs(col.R - 128) 
                 + Math.Abs(col.G - 128) 
                 + Math.Abs(col.B - 128);
        }

        public static int ColorCompare(Color colA, Color colB)
        {
            return Math.Abs(colA.R - colB.R) 
                 + Math.Abs(colA.G - colB.G) 
                 + Math.Abs(colA.B - colB.B);
        }
        public static double ColorXCompare(ColorX colA, ColorX colB)
        {
            return Math.Abs(colA.grade - colB.grade);
        }

        public static double TotalColor(Color col)
        {
            //int rs = col.R + col.G + col.B;

            double rs = 
                (col.R > 150 ? col.R * 2.5 : col.R > 70 ? col.R * 1.7 : col.R * 0.7)
                + (col.G > 150 ? col.G * 2.5 : col.G > 70 ? col.G * 1.7 : col.G * 0.7)
                + (col.B > 150 ? col.B * 2.5 : col.B > 70 ? col.B * 1.7 : col.B * 0.7)
                ;

            

            var avg = (col.R + col.G + col.B)*0.6;

            if (col.R > avg && col.G < avg && col.B < avg) return rs*-10;
            if (col.R < avg && col.G > avg && col.B < avg) return rs*10;
            if (col.R < avg && col.G < avg && col.B > avg) return rs*20;
            return rs*1;
        }
    }

    public class ColorX
    {
        public Color col;
        public double grade;
        public float count;
        public double more;
        public ColorX(Color getPixel) { col = getPixel; }
    }
}
