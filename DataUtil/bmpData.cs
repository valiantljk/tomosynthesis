using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace DataUtil
{
    /// <summary>
    /// Author: JiaLin Liu
    /// Date:May.6. 2010
    /// Company:JNU
    /// 指针处理位图类。
    /// </summary>
    public unsafe class BitmapDataUtil : IDisposable
    {

        private PixelFormat BMPpf;

        private int stride;

        private int width, height;

        private byte* ptrData;

        private Bitmap bmp;

        private BitmapData BMPdata;

        public BitmapDataUtil(BitmapData BMPdata)
        {
            this.bmp = null;
            this.BMPdata = BMPdata;
            BMPpf = BMPdata.PixelFormat;
            stride = BMPdata.Stride;
            height = BMPdata.Height;
            width = BMPdata.Width;
            ptrData = (byte*)(BMPdata.Scan0.ToPointer());

        }

        public BitmapDataUtil(Bitmap srcbmp)
        {
            this.bmp = srcbmp;
            this.BMPdata = srcbmp.LockBits(new Rectangle(0, 0, srcbmp.Width, srcbmp.Height), ImageLockMode.ReadWrite, srcbmp.PixelFormat);
            BMPpf = BMPdata.PixelFormat;
            stride = BMPdata.Stride;
            height = BMPdata.Height;
            width = BMPdata.Width;
            ptrData = (byte*)(BMPdata.Scan0.ToPointer());

        }
        public byte[] easyGetPixel(int x, int y)
        {
            if (x > width || y > height)
            {
                throw new Exception("处理图片越界。");
            }
            byte[] color;
            if (BMPpf == PixelFormat.Format8bppIndexed)
            {
                color = new byte[1];

                color[0] = *(ptrData + y * stride + x);
            }
            else
            {
                color = new byte[3];
                color[0] = *(ptrData + y * stride + 3 * x);
                color[1] = *(ptrData + y * stride + 3 * x + 1);
                color[2] = *(ptrData + y * stride + 3 * x + 2);
            }

            return color;
        }

        public void easySetPixel(int x, int y, byte color)
        {
            if (x > width || y > height)
            {
                throw new Exception("处理图片越界。");
            }
            if (BMPpf != PixelFormat.Format8bppIndexed)
            {
                throw new Exception("图像色彩出错。");
            }
            *(ptrData + y * stride + x) = color;
            return;
        }
        public void easySetPixel(int x, int y, byte[] color)
        {
            if (x > width || y > height)
            {
                throw new Exception("处理图片越界。");
            }
            if (BMPpf == PixelFormat.Format24bppRgb && color.Length < 3)
            {
                throw new Exception("颜色字节数组出错。");
            }
            //8位图片取第一位
            if (BMPpf == PixelFormat.Format8bppIndexed)
            {
                *(ptrData + y * stride + x) = color[0];
            }
            else
            {
                *(ptrData + y * stride + 3 * x) = color[0];
                *(ptrData + y * stride + 3 * x + 1) = color[1];
                *(ptrData + y * stride + 3 * x + 2) = color[2];
            }

            return;
        }

        ~BitmapDataUtil()
        {
            if (this.BMPdata != null)
            {
                this.BMPdata = null;
            }

            if (this.ptrData != null)
            {
                this.ptrData = null;
            }

            if (this.bmp != null && this.BMPdata != null)
            {
                bmp.UnlockBits(this.BMPdata);
            }
        }

        #region IDisposable 成员

        public void Dispose()
        {
            if (this.bmp != null && this.BMPdata != null)
            {
                bmp.UnlockBits(this.BMPdata);
            }
        }

        #endregion
    }
}
