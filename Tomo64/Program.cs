using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Tomo64
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.DateTime currenttime=System.DateTime.Now;
            int year=currenttime.Year;
            //int month = currenttime.Month;
            if (year!=2011)
            {
                //&&month!=1&&month!=3&&month!=5&&month!=7&&month!=9
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }          

        }
    }
}
