using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
namespace BGInfo
{
    class Program
    {
        static int ScreenHeight, ScreenWidth;
        static String ErrorTxt;
        static String ScriptName;
        static String hostName;
        static bool CreateBGImage(string ImageFile, Color BGColor)
        {
            bool result = true;
            Bitmap Img;
            Graphics graphics;
            //TODO:Try
            Img = new Bitmap(ScreenWidth, ScreenHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            //for (int w=0;w!=ScreenWidth;w++) for (int h = 0; h != ScreenHeight; h++) Img.SetPixel(w,h,Color.DarkBlue);
            graphics = Graphics.FromImage(Img);
            graphics.Clear(BGColor);
            BGImage(graphics);
            try { Img.Save(ImageFile, System.Drawing.Imaging.ImageFormat.Jpeg); } catch (Exception e) { ErrorTxt = e.ToString(); result = false; }
            Img.Dispose();
            graphics.Dispose();
            return result;
        }
        static bool EditBGImage(string ImageFile)
        {
            bool result = true;
            if (!File.Exists(ImageFile)) { result = false; ErrorTxt = "Исходный файл не найден\n" + ImageFile; return result; };
            return result;
        }
        static bool CopyBGImage(string FileFrom, string FileTo)
        {
            bool result = true;
            if (String.Compare(FileFrom, FileTo, true) == 0) return (EditBGImage(FileFrom));
            if (!File.Exists(FileFrom)) { result = false; ErrorTxt = "Исходный файл не найден\n" + FileFrom; return result; };
            if (File.Exists(FileTo)) { try { File.Delete(FileTo); } catch (Exception e) { result = false; ErrorTxt = e.Message; return result; } }
            Bitmap Img;
            Graphics graphics;
            try { Img = new Bitmap(FileFrom); } catch (Exception e) { result = false; ErrorTxt = e.Message; return result; }
            graphics = Graphics.FromImage(Img);
            //TODO: Resize origin image to real resolution            
            BGImage(graphics);
            try { Img.Save(FileTo, System.Drawing.Imaging.ImageFormat.Jpeg); } catch (Exception e) { ErrorTxt = e.ToString(); result = false; }
            Img.Dispose();
            graphics.Dispose();
            return result;
        }
        static bool BGImage(Graphics graphics)
        {
            //https://docs.microsoft.com/en-us/dotnet/framework/winforms/advanced/how-to-align-drawn-text               
            int xPosText = 10;
            int yPosText = 10;
            Font font1 = new Font("Arial", 72, FontStyle.Bold, GraphicsUnit.Point);
            Rectangle rect1 = new Rectangle(ScreenWidth / 2, yPosText, (ScreenWidth - xPosText) / 2, (ScreenHeight - yPosText) / 4);
            TextFormatFlags flags = TextFormatFlags.Right;
            //https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.textrenderer.drawtext?view=netframework-4.8#System_Windows_Forms_TextRenderer_DrawText_System_Drawing_IDeviceContext_System_String_System_Drawing_Font_System_Drawing_Rectangle_System_Drawing_Color_System_Windows_Forms_TextFormatFlags_
            System.Windows.Forms.TextRenderer.DrawText(graphics, hostName, font1, rect1,
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
