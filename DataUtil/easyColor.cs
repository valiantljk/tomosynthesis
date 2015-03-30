using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Data;
namespace DataUtil
{
    /// <summary>
    /// Author: JiaLin Liu
    /// Date: Nov.3. 2010
    /// Company: E-COM
    /// </summary>
    public class easyColor
    {
        private Byte[] part = new byte[3];

        private PixelFormat pixformat;

        public easyColor(byte color)
        {

            pixformat = PixelFormat.Format8bppIndexed;
            this.part[0] = color;
        }

        public easyColor(byte R, byte G, byte B)
        {
            pixformat = PixelFormat.Format24bppRgb;
            this.part[0] = R;
            this.part[1] = G;
            this.part[2] = B;
        }
    }
}
