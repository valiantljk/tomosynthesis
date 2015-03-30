using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Data;
using System.Drawing.Imaging;
using DataUtil;

namespace DataLoad
{
    /// <summary>
    /// AUTHOR: Jialin Liu
    /// DATE: OCT.29. 2010
    /// DLL: Sheng Ye
    /// COMPANY: E-COM
    /// Load raw data from hardware and make linearInterpolation
    /// Extract bitmap from rawdata.
    /// </summary>
    public class Load
    {
        private int WW = 0;
        private int WL = 0;
        public int Ww//定义窗宽
        {
            get { return this.WW; }
            set { this.WW = value; }
        }
        public int Wl//定义窗位
        {
            get { return this.WL; }
            set { this.WL = value; }
        }
        #region 从硬盘读入原始raw数据
        public System.UInt16[] LoadrawData(String filename, int w, int h)
        {
            try
            {
                System.UInt16[] pro = new ushort[w * h];
                BinaryReader binReader = new BinaryReader(File.Open((filename), FileMode.Open));
                int j = 0;
                while (j < w * h)
                {
                    pro[j] = binReader.ReadUInt16();
                    j++;
                }

                return pro;
            }
            catch (System.Exception e)
            {
                return null;
            }



        }
        #endregion
        #region 二次线性插值
        /// <summary>
        /// 从读入的unsigned short 原数据中构建bmp图像，用于显示观察
        /// </summary>
        /// <param name="pro"></param>
        /// <returns>bitmap</returns>
        public System.UInt16[] LinerInterpolation(System.UInt16[] pro, int w, int h)
        {
            try
            {
                System.UInt16[] temp = pro;
                System.UInt16[] zoomPicture = new ushort[257 * 268];
                double Rowcoe = (double)w / (double)257;
                double Colcoe = (double)h / (double)268;
                double k1;
                double k2;
                for (int i = 0; i < 268; i++)
                {
                    for (int j = 0; j < 257; j++)
                    {
                        double X = Rowcoe * j;//the target X position;
                        double Y = Colcoe * i;//the target Y position;
                        int tempX = (int)Math.Truncate(X);
                        int tempY = (int)Math.Truncate(Y);
                        int lefttopX = tempX < 0 ? 0 : tempX;
                        int lefttopY = tempY < 0 ? 0 : tempY;
                        int righttopX = (lefttopX + 1) < w ? (lefttopX + 1) : (w - 1);
                        int righttopY = lefttopY;

                        int leftbottomX = lefttopX;
                        int leftbottomY = (lefttopY + 1) < h ? (lefttopY + 1) : (h - 1);

                        int rightbottomX = righttopX;
                        int rightbottomY = leftbottomY;

                        k1 = X - lefttopX;
                        k2 = Y - lefttopY;

                        double top = (1 - k1) * (double)temp[lefttopY * w + lefttopX] + k1 * (double)temp[righttopY * w + righttopX];
                        double bottom = (1 - k1) * (double)temp[w * leftbottomY + leftbottomX] + k1 * (double)temp[w * rightbottomY + rightbottomX];
                        double value = (1 - k2) * top + k2 * bottom;
                        if (value>10000)
                        {
                            int a = 0;
                        }

                        zoomPicture[i * 257 + j] = (System.UInt16)value;
                        if (zoomPicture[i*257+j]>10000)
                        {
                            int b = 0;
                        }
                    }
                }
                return zoomPicture;
            }
            catch (System.Exception e)
            {
                return null;
            }

        }
        #endregion
        #region 从原始数据中提取图片
        public unsafe Bitmap Extract(System.UInt16[] pro, int w, int h)
        {
            System.UInt16[] tmppro = pro;
            int[] Twovalue = getmax(tmppro);
            Bitmap bitmap = new Bitmap(w, h, PixelFormat.Format8bppIndexed);

            using (DataUtil.BitmapDataUtil bmpdata = new DataUtil.BitmapDataUtil(bitmap))
            {
                for (int i = 0; i < h; i++)
                {
                    for (int j = 0; j < w; j++)
                    {
                        int greyvalue = tmppro[i * w + j];
                        if (greyvalue>10000)
                        {
                            int a = 0;
                        }
                        if (greyvalue > WL + (int)WW / 2)
                            greyvalue = 255;
                        else if (greyvalue < WL - (int)(WW / 2))
                            greyvalue = 0;
                        else
                        {
                            greyvalue = (int)(greyvalue - (WL - WW / 2)) * 256 / WW;
                            if (greyvalue == 256)
                                greyvalue = 255;
                        }
                        byte color = (byte)greyvalue;
                        bmpdata.easySetPixel(j, i, color);
                    }
                }
                ColorPalette tempPalette;
                Bitmap tempBmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed);
                tempPalette = tempBmp.Palette;
                for (int ii = 0; ii < 256; ii++)
                {
                    tempPalette.Entries[ii] = Color.FromArgb(ii, ii, ii);
                }
                bitmap.Palette = tempPalette;
            }
            return bitmap;
        }

        #endregion
        #region 求读入数据中灰度值值域
        /// <summary>
        /// 遍历寻找最大最小值
        /// </summary>
        /// <param name="pro"></param>
        /// <returns></returns>
        public int[] getmax(System.UInt16[] pro)
        {

            int min = System.UInt16.MaxValue;
            int max = System.UInt16.MinValue;
            for (int i = 0; i < pro.Length; i++)
            {
                int currentValue = pro[i];
                if (currentValue < min)
                    min = currentValue;
                else if (currentValue > max)
                    max = currentValue;
            }
            int[] result = new int[2];
            result[0] = min;
            result[1] = max;
            return result;
        }

        #endregion

    }
}


//脏旧代码
//public Bitmap load(int iofImage)
//{
//    //String headDataFilename = "E:\\工作\\实习\\Data\\TestData.h";
//    System.UInt16[] pro = new ushort[2880 * 2881];
//    try
//    {
//        //StreamReader srHead = new StreamReader(headDataFilename, Encoding.Default);
//        //String rawDataPath = srHead.ReadLine().ToString();
//        String rawDataPath = "K:\\Jialin\\data\\slice\\";
//        try
//        {
//            BinaryReader binReader = new BinaryReader(File.Open((rawDataPath + iofImage + ".raw"), FileMode.Open));
//            int i = 0;
//            while (i < 2880 * 2881)
//            {
//                pro[i] = binReader.ReadUInt16();
//                i++;
//            }
//            Bitmap bp = Extract(pro);
//            //修改生成位图的索引表，从伪彩修改为灰度
//            ColorPalette tempPalette;
//            Bitmap tempBmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed);
//            tempPalette = tempBmp.Palette;
//            for (int ii = 0; ii < 256; ii++)
//            {
//                tempPalette.Entries[ii] = Color.FromArgb(ii, ii, ii);
//            }
//            bp.Palette = tempPalette;
//            // bp.Save((rawDataPath + "\\19nd4.bmp"));
//            return bp;
//        }
//        catch (OutOfMemoryException)
//        { }
//    }
//    catch (IOException)
//    { }

//    return null;
//}
//private unsafe int[]getmax(Bitmap bp)
//{
//    int min = System.UInt16.MaxValue;
//    int max = System.UInt16.MinValue;
//    for (int i = 0; i <; i++)
//    {
//        int currentValue = pro[i];
//        if (currentValue < min)
//            min = currentValue;
//        else if (currentValue > max)
//            max = currentValue;
//    }
//    int[] result = new int[2];
//    result[0] = min;
//    result[1] = max;
//    return result;
//}
//#region 载入数据

//public Bitmap load()
//{
//    String headDataFilename = "K:\\Jialin\\data\\TestData.h";
//    System.UInt16[] pro = new ushort[2880 * 2881];
//    try
//    {
//        StreamReader srHead = new StreamReader(headDataFilename, Encoding.Default);
//        String rawDataPath = srHead.ReadLine().ToString();
//        try
//        {
//            BinaryReader binReader = new BinaryReader(File.Open((rawDataPath + "\\19.raw"), FileMode.Open));
//            int i = 0;
//            while (i < 2880 * 2881)
//            {
//                pro[i] = binReader.ReadUInt16();
//                i++;
//            }
//            Bitmap bp = Extract(pro);
//            //修改生成位图的索引表，从伪彩修改为灰度
//            ColorPalette tempPalette;
//            Bitmap tempBmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed);
//            tempPalette = tempBmp.Palette;
//            for (int ii = 0; ii < 256; ii++)
//            {
//                tempPalette.Entries[ii] = Color.FromArgb(ii, ii, ii);
//            }
//            bp.Palette = tempPalette;
//            //bp.Save((rawDataPath + "\\19nd4.bmp"));
//            return bp;
//        }
//        catch (OutOfMemoryException)
//        { }
//    }
//    catch (IOException)
//    { }

//    return null;
//}
//#endregion
//public Bitmap load(string filename,int w,int h)
//{
//    System.UInt16[] pro = new ushort[w * h];
//    try
//    {
//        //StreamReader srHead = new StreamReader(headDataFilename, Encoding.Default);
//        //String rawDataPath = srHead.ReadLine().ToString();
//        try
//        {
//            BinaryReader binReader = new BinaryReader(File.Open((filename), FileMode.Open));
//            int i = 0;
//            while (i < w*h)
//            {
//                pro[i] = binReader.ReadUInt16();
//                i++;
//            }
//            Bitmap bp = Extract(pro,w,h);
//            //修改生成位图的索引表，从伪彩修改为灰度
//            ColorPalette tempPalette;
//            Bitmap tempBmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed);
//            tempPalette = tempBmp.Palette;
//            for (int ii = 0; ii < 256; ii++)
//            {
//                tempPalette.Entries[ii] = Color.FromArgb(ii, ii, ii);
//            }
//            bp.Palette = tempPalette;
//            //bp.Save((rawDataPath + "\\19nd4.bmp"));
//            return bp;
//        }
//        catch (OutOfMemoryException)
//        { }
//    }
//    catch (IOException)
//    { }

//    return null;
//}
//private unsafe Bitmap Extract(Bitmap bp,int w,int h)
//{
//    int[] Twovalue = getmax(bp);
//    Bitmap bitmap = new Bitmap(w, h, PixelFormat.Format8bppIndexed);
//    int low = Twovalue[0];
//    int high = Twovalue[1];
//    //定义窗宽窗位
//    // int WW, WL;
//    if (WW == 0 || WL == 0)
//    {
//        WW = (int)(RateWW * (high - low) + 1);//窗宽
//        WL = System.Math.Max((int)(RateWL * (high - low)), (int)(WW / 2 + 1));//窗位 
//    }
//    using (DataUtil.BitmapDataUtil bmpdata = new DataUtil.BitmapDataUtil(bitmap))
//    {
//        for (int i = 0; i < h; i++)
//        {
//            for (int j = 0; j < w; j++)
//            {
//                int greyvalue = tmppro[i * w + j];
//                // if (greyvalue > 1300)
//                if (greyvalue > WL + (int)WW / 2)
//                    greyvalue = 255;
//                //if (greyvalue <700)
//                if (greyvalue < WL - (int)(WW / 2))
//                    greyvalue = 0;
//                else
//                {
//                    greyvalue = (int)(greyvalue - (WL - WW / 2)) * 256 / WW;
//                    if (greyvalue == 256)
//                        greyvalue = 255;
//                }
//                byte color = (byte)greyvalue;
//                bmpdata.easySetPixel(j, i, color);
//            }
//        }
//    }
//    return bitmap;
//}