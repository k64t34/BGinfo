using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
namespace BGInfo
{
    public  class Info
    {
        public static String hostName;
        public static int ScreenHeight, ScreenWidth;
    }
    public static class Image
    {
        private static ImageCodecInfo GetEncoder(ImageFormat format)//https://docs.microsoft.com/ru-ru/dotnet/framework/winforms/advanced/how-to-set-jpeg-compression-level
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
        public static bool Create(string ImageFile, Color BGColor)
        {
            bool result = true;
            Bitmap Img;
            Graphics graphics;
            //TODO:Try
            Img = new Bitmap(Info.ScreenWidth, Info.ScreenHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            //for (int w=0;w!=ScreenWidth;w++) for (int h = 0; h != ScreenHeight; h++) Img.SetPixel(w,h,Color.DarkBlue);
            graphics = Graphics.FromImage(Img);
            graphics.Clear(BGColor);
            if (BGImage(graphics)) result = Save(ImageFile, Img);            
            Img.Dispose();
            graphics.Dispose();
            return result;
        }
        public static bool Edit(string ImageFile)
        {
            bool result = true;
            if (!File.Exists(ImageFile)) { result = false; /*ErrorTxt = "Исходный файл не найден\n" + ImageFile; */};
            return result;
        }
        public static bool Save(string ImageFile, Bitmap Img)
        {
            bool result = true;
            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg); //https://docs.microsoft.com/ru-ru/dotnet/framework/winforms/advanced/how-to-set-jpeg-compression-level
            System.Drawing.Imaging.Encoder myEncoder =
                System.Drawing.Imaging.Encoder.Quality;
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 100L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            try { Img.Save(ImageFile, jpgEncoder, myEncoderParameters); } catch (Exception e) { /*ErrorTxt = e.ToString();*/ result = false; }
            return result;
        }
        public static bool Copy(string FileFrom, string FileTo)
        {
            bool result = true;
            if (String.Compare(FileFrom, FileTo, true) == 0) return (Edit(FileFrom));
            if (!File.Exists(FileFrom)) { result = false; /*ErrorTxt = "Исходный файл не найден\n" + FileFrom; */return result; };
            if (File.Exists(FileTo)) { try { File.Delete(FileTo); } catch (Exception e) { result = false; /*ErrorTxt = e.Message;*/ return result; } }
            Bitmap Img;
            Graphics graphics;
            try { Img = new Bitmap(FileFrom); } catch (Exception e) { result = false; /*ErrorTxt = e.Message;*/ return result; }
            
            if (Img.Width != Info.ScreenWidth || Img.Height != Info.ScreenHeight) // Расчитано что исходный файл оч маленький и ВСЕГДА увеличиваетя размер холста
            {
                Color FirstPixel=Img.GetPixel(0,0);
                Img.Dispose();
                Img = new Bitmap(Info.ScreenWidth, Info.ScreenHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                graphics = Graphics.FromImage(Img);
                graphics.Clear(FirstPixel);
                Bitmap sourceImg = new Bitmap(FileFrom);
                graphics.DrawImage(sourceImg, 0, 0);
                sourceImg.Dispose();
            }
            else
            {
                graphics = Graphics.FromImage(Img);
            }
            if (BGImage(graphics)) result=Save(FileTo, Img);
            Img.Dispose();
            graphics.Dispose();
            return result;
        }
        public static bool BGImage(Graphics graphics)
        {
            //https://docs.microsoft.com/en-us/dotnet/framework/winforms/advanced/how-to-align-drawn-text               
            int xPosText = 10;
            int yPosText = 10;
            Font font1 = new Font("Arial", 72, FontStyle.Bold, GraphicsUnit.Point);
            Rectangle rect1 = new Rectangle(Info.ScreenWidth / 2, yPosText, (Info.ScreenWidth - xPosText) / 2, (Info.ScreenHeight - yPosText) / 4);
            TextFormatFlags flags = TextFormatFlags.Right;
            //https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.textrenderer.drawtext?view=netframework-4.8#System_Windows_Forms_TextRenderer_DrawText_System_Drawing_IDeviceContext_System_String_System_Drawing_Font_System_Drawing_Rectangle_System_Drawing_Color_System_Windows_Forms_TextFormatFlags_
            System.Windows.Forms.TextRenderer.DrawText(graphics, Info.hostName, font1, rect1,
                    System.Drawing.Color.White, flags);
            // Draw the text and the surrounding rectangle.
            //graphics.DrawString(hostName, font1, Brushes.White, rect1, stringFormat);
            //graphics.DrawRectangle(Pens.Black, rect1);

            /*graphics.DrawString(hostName, drawFont, Brushes.Black, xPosText-1, yPosText-1);
                            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;  
            graphics.DrawString(hostName, drawFont, Brushes.Black, xPosText+1, yPosText+1);
            graphics.DrawString(hostName, drawFont, Brushes.Black, xPosText-1, yPosText);
            graphics.DrawString(hostName, drawFont, Brushes.Black, xPosText+1, yPosText);
            graphics.DrawString(hostName, drawFont, Brushes.White, xPosText, yPosText);*/
            return true;
        }
    }
}
