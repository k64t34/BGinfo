using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;


namespace BGInfo
{
    public static class Wallpaper
    {
        static public String BGImageFile;
        static public Color  BGColor;
            static public int BGImageAlign_H=1, BGImageAlign_V=1; //H:0 - center, 1 - left, 2 - right , V: 0 -center, 1- Top, 2 - Bottom 
        static public byte Style = 0;//0-color, 1-Center, 2-Tile, 3- Stretch, 4-Span, 5-Fit, 6-Fill
        public const byte s_COLOR = 0;
        public const byte s_CENTER = 1;
        public const byte s_TILE = 2;
        public const byte s_STRETCH = 3;
        public const byte s_SPAN = 4;
        public const byte s_FIT = 5;
        public const byte s_FILL = 6;
        //static public int ScreenWidth=1920, ScreemHeight=1080;
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
        public static bool Create(string ImageFile) 
        {
            bool result = true;
            Bitmap SrcImg ;
            Graphics Graphics;
            int Src_Width,Src_Height;
            System.Drawing.Image SrcImgFile;
            //Create custom source file if wallpaper style is COLOR
            if (Wallpaper.Style == s_COLOR)
            {
                BGImageFile = Path.Combine(Environment.GetEnvironmentVariable("APPDATA") + @"\Microsoft\Windows\Themes\", "DesktopBGInfo.jpg");
                SrcImg = new Bitmap(BGInfo.Info.ScreenWidth, BGInfo.Info.ScreenHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                Graphics = Graphics.FromImage(SrcImg);
                Graphics.Clear(BGColor);
                Save(BGImageFile, SrcImg);
                Graphics.Dispose();
                SrcImg.Dispose();
            }
            SrcImgFile = System.Drawing.Image.FromFile(BGImageFile);                
            Src_Width = SrcImgFile.Width;
            Src_Height = SrcImgFile.Height;
            //Create new image 
            Bitmap NewImg = new Bitmap(BGInfo.Info.ScreenWidth, BGInfo.Info.ScreenHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Graphics = Graphics.FromImage(NewImg);
            //Fill image with color 
            if (Wallpaper.Style == s_FIT || Wallpaper.Style == s_CENTER) Graphics.Clear(BGColor);
            //Calculate source rectangle 
            Rectangle SrcRectangle= new Rectangle(0, 0, Src_Width, Src_Height);
            Rectangle newRectangle=new Rectangle(0, 0, Info.ScreenWidth, Info.ScreenHeight);
            if (Wallpaper.Style == s_CENTER)
            {
                if (Src_Width <= Info.ScreenWidth)
                {
                    //SrcRectangle.Width = Src_Width;
                    newRectangle.X= Convert.ToInt32(Math.Floor((double)(Info.ScreenWidth - Src_Width) / 2.0));
                    newRectangle.Width = Src_Width;
                }
                else
                {                    
                    SrcRectangle.X = Convert.ToInt32(Math.Floor((double)(Src_Width - Info.ScreenWidth  ) / 2.0));
                    SrcRectangle.Width = Info.ScreenWidth;
                    //newRectangle.Width = Info.ScreenWidth;
                }
                if (Src_Height <= Info.ScreenHeight)
                {
                    //SrcRectangle.Height = Src_Height;
                    newRectangle.Y = Convert.ToInt32(Math.Floor((double)(Info.ScreenHeight - Src_Height) / 2.0));
                    newRectangle.Height = Src_Height;
                }
                else
                {
                    SrcRectangle.Y = Convert.ToInt32(Math.Floor((double)(Src_Height - Info.ScreenHeight) / 2.0));
                    SrcRectangle.Height = Info.ScreenHeight;
                    //newRectangle.Height = Info.ScreenHeight;
                }
            }
            else if (Wallpaper.Style == s_FILL) 
            {

            }
            


            //Calculate target rectangle            
            if (/*Wallpaper.Style == s_CENTER || */Wallpaper.Style == s_FIT)
            {
                newRectangle = new Rectangle(0, 0, 0,0);
            }
            /*else
                newRectangle = new Rectangle(0, 0, Info.ScreenWidth, Info.ScreenHeight);*/

            //Copy source background image to new image
            GraphicsUnit units = GraphicsUnit.Pixel;
            Graphics.DrawImage(SrcImgFile,newRectangle,SrcRectangle, units); //https://docs.microsoft.com/ru-ru/dotnet/api/system.drawing.graphics.drawimage?view=dotnet-plat-ext-3.1#System_Drawing_Graphics_DrawImage_System_Drawing_Image_System_Drawing_RectangleF_System_Drawing_RectangleF_System_Drawing_GraphicsUnit_
            //Print background text and save file
            Info.GetInfo();            
            if (BGImage(Graphics)) result = Save(ImageFile, NewImg);
            NewImg.Dispose();
            Graphics.Dispose();
            SrcImgFile.Dispose();



            /*

            int BGImageFile_Width,BGImageFile_Height;




            System.Drawing.Image SrcImg;

            if (File.Exists(BGImageFile)) try
                {
                    srcimg = System.Drawing.Image.FromFile(BGImageFile);
                    BGImageFile_Width = srcimg.Width; BGImageFile_Height = srcimg.Height;
                }
                catch (Exception e) { Log.LogError(e.ToString()); BGImageFile = String.Empty;}
            else BGImageFile = String.Empty;
            Bitmap Img;
            Img = new Bitmap(BGInfo.Info.ScreenWidth, BGInfo.Info.ScreenHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Graphics graphics;
            graphics = Graphics.FromImage(Img);
            //TODO заполнить цветом \HKEY_CURRENT_USER\Control Panel\Colors backgroud или создавать сразу файл с цветом фона
            graphics.Clear(BGColor);
            //Fill image with color
            if (!String.IsNullOrEmpty(BGImageFile)) 
            {
                srcimg = System.Drawing.Image.FromFile(BGImageFile);
                BGImageFile_Width = srcimg.Width; BGImageFile_Height = srcimg.Height;
                int Margin_Left = 0, Margin_Top = 0;
                int Skip_left = 0, Skip_top = 0;
                int srcImgCopyWidth = BGImageFile_Width, srcImgCopyHeight = BGImageFile_Height;
                //TODO: remake over diff fld
                if (BGImageFile_Width < BGInfo.Info.ScreenWidth) Margin_Left = Convert.ToInt32(Math.Floor((double)(BGInfo.Info.ScreenWidth - BGImageFile_Width) / 2.0));
                else if (BGImageFile_Width > BGInfo.Info.ScreenWidth)
                {
                    Skip_left = Convert.ToInt32(Math.Floor((double)(BGImageFile_Width - BGInfo.Info.ScreenWidth) / 2.0));
                    srcImgCopyWidth = BGImageFile_Width - Skip_left - Skip_left;
                }
                if (BGImageFile_Height < BGInfo.Info.ScreenHeight) Margin_Top = Convert.ToInt32(Math.Floor((double)(BGInfo.Info.ScreenHeight - BGImageFile_Height) / 2.0));
                else if (BGImageFile_Height > BGInfo.Info.ScreenHeight) Skip_left = Convert.ToInt32(Math.Floor((double)(BGImageFile_Height - BGInfo.Info.ScreenHeight) / 2.0));

                GraphicsUnit units = GraphicsUnit.Pixel;                
                graphics.DrawImage(srcimg, new Rectangle(Margin_Left, Margin_Top, srcImgCopyWidth, srcImgCopyHeight), Skip_left, Skip_top, srcImgCopyWidth, srcImgCopyHeight, units);
                srcimg.Dispose();

            }
            Info.GetInfo();            
            if (BGImage(graphics)) result = Save(ImageFile, Img);
            Img.Dispose();
            graphics.Dispose();
            */

            /*                try { Img = new Bitmap(BGImageFile); } catch (Exception e) { Log.LogError(e.ToString()); return false; }
                        else
                        {
                            try
                            {
                                Img = new Bitmap(BGInfo.Info.ScreenWidth, BGInfo.Info.ScreenHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                            }
                            catch (Exception e) { Log.LogError(e.ToString()); return false; }
                        }*/

            /* public static void CopyRegionIntoImage(Bitmap srcBitmap, Rectangle srcRegion,ref Bitmap destBitmap, Rectangle destRegion)
    {
        using (Graphics grD = Graphics.FromImage(destBitmap))            
        {
            grD.DrawImage(srcBitmap, destRegion, srcRegion, GraphicsUnit.Pixel);                
        }
    }*/
     
            return result;
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
            try { Img.Save(ImageFile, jpgEncoder, myEncoderParameters); }
            catch /*(Exception e)*/ { /*ErrorTxt = e.ToString();*/ result = false; }
            return result;
        }
        public static bool Copy(string FileFrom, string FileTo)
        {
            bool result = true;
            if (String.Compare(FileFrom, FileTo, true) == 0) return (Edit(FileFrom));
            if (!File.Exists(FileFrom)) { result = false; /*ErrorTxt = "Исходный файл не найден\n" + FileFrom; */return result; };
            if (File.Exists(FileTo)) { try { File.Delete(FileTo); } catch /*(Exception e)*/ { result = false; /*ErrorTxt = e.Message;*/ return result; } }
            Bitmap Img;
            Graphics graphics;
            try { Img = new Bitmap(FileFrom); } catch /*(Exception e)*/ { result = false; /*ErrorTxt = e.Message;*/ return result; }

            if (Img.Width != Info.ScreenWidth || Img.Height != Info.ScreenHeight) // Расчитано что исходный файл оч маленький и ВСЕГДА увеличиваетя размер холста
            {
                Color FirstPixel = Img.GetPixel(0, 0);
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
            if (BGImage(graphics)) result = Save(FileTo, Img);
            Img.Dispose();
            graphics.Dispose();
            return result;
        }
        public static bool BGImage(Graphics graphics)
        {
            //https://docs.microsoft.com/en-us/dotnet/framework/winforms/advanced/how-to-align-drawn-text               
            //Draw hostName
            int xPosText = 10;
            int yPosText = 10;
            Font font1 = new Font("Arial", 72, FontStyle.Bold, GraphicsUnit.Point);
            Size proposedSize = new Size(int.MaxValue, int.MaxValue);
            Size size = TextRenderer.MeasureText(Info.hostName, font1, proposedSize);
            Rectangle rect1 = new Rectangle(Info.ScreenWidth / 2, yPosText, (Info.ScreenWidth - xPosText) / 2, size.Height/*(Info.ScreenHeight - yPosText) / 8*/);
            TextFormatFlags flags = TextFormatFlags.HorizontalCenter;
            //TextFormatFlags flags = TextFormatFlags.HorizontalCenter;
            //TextFormatFlags.VerticalCenter;
            //https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.textrenderer.drawtext?view=netframework-4.8#System_Windows_Forms_TextRenderer_DrawText_System_Drawing_IDeviceContext_System_String_System_Drawing_Font_System_Drawing_Rectangle_System_Drawing_Color_System_Windows_Forms_TextFormatFlags_
            System.Windows.Forms.TextRenderer.DrawText(graphics, Info.hostName, font1, rect1,
                    System.Drawing.Color.White, flags);
            //Draw hostDescription
            xPosText = 10;
            yPosText = 10 + rect1.Height;
            font1 = new Font("Arial", 36, FontStyle.Bold, GraphicsUnit.Point);
            proposedSize = new Size(int.MaxValue, int.MaxValue);
            size = TextRenderer.MeasureText(Info.hostDescription, font1, proposedSize);
            rect1 = new Rectangle(Info.ScreenWidth / 2, yPosText, (Info.ScreenWidth - xPosText) / 2, size.Height);
            flags = TextFormatFlags.HorizontalCenter;
            //https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.textrenderer.drawtext?view=netframework-4.8#System_Windows_Forms_TextRenderer_DrawText_System_Drawing_IDeviceContext_System_String_System_Drawing_Font_System_Drawing_Rectangle_System_Drawing_Color_System_Windows_Forms_TextFormatFlags_
            System.Windows.Forms.TextRenderer.DrawText(graphics, Info.hostDescription, font1, rect1,
                    System.Drawing.Color.LightGray, flags);

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
