using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime;
using System.Runtime.InteropServices;
using Reconstruction;
using DataLoad;
using DataUtil;
namespace Tomosynthesis_simulation_plantform
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void loadData_Button_Click(object sender, EventArgs e)
        {
            string ww = this.textBox1.Text.ToString();
            string wl = this.textBox2.Text.ToString();
            if (ww != "" && wl != "")
            {
                Load loadNewData = new Load();
                loadNewData.Wl = int.Parse(wl);
                loadNewData.Ww = int.Parse(ww);
                this.pictureBox2.Image = loadNewData.load();
                this.groupBox3.Visible = true;
                this.textBox2.Clear();
                this.textBox1.Clear();
                this.toolStripStatusLabel1.Text = "Windows Width:" + ww + ";Windows Location:" + wl + ".";
            }
            else
            {
                double WW = (double)this.hScrollBar1.Value / this.hScrollBar1.Maximum;
                double WL = (double)this.hScrollBar2.Value / this.hScrollBar1.Maximum;
                Load loadNewData = new Load();
                loadNewData.RateWW = WW;
                loadNewData.RateWL = WL;
                this.pictureBox2.Image = loadNewData.load();
                this.groupBox3.Visible = true;
                this.textBox1.Clear();
                this.textBox2.Clear();
                this.toolStripStatusLabel1.Text = "Windows Width:" + loadNewData.Ww + ";Windows Location:" + loadNewData.Wl + ".";
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void ZoomOut_Click(object sender, EventArgs e)
        {
            if (this.pictureBox2.Image != null)
            {
                this.pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
                this.pictureBox2.Width *= 2;
                this.pictureBox2.Height *= 2;
            }
        }

        private void ZoomIn_Click(object sender, EventArgs e)
        {
            if (this.pictureBox2.Image != null)
            {
                this.pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                this.pictureBox2.Width /= 2;
                this.pictureBox2.Height /= 2;
            }
        }
        private DataUtil.tomoData.VOI_INFO VOIInfo;
        private DataUtil.tomoData.TOMO_INFO TOMOInfo;
        private DataUtil.tomoData.VECTOR3f[] DetectorLeftTop;
        private System.UInt16[] pBlankScan;
        private System.UInt16[] pNormalScan;
        private String blankPath = "E:\\工作\\实习\\Data\\blank\\";
        private String originalPath = "E:\\工作\\实习\\Data\\original\\";
        private void button12_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            string totalview = this.numericUpDown6.Value.ToString();
            int total = Int16.Parse(totalview);

            // InitialHardWare();
            if (Reconstruction.Reconstruction.InitialHardWare())
            {
                this.toolStripStatusLabel1.Text = "InitialHardWare成功";
            }
            else
            {
                this.toolStripStatusLabel1.Text = "InitialHardWare失败";
            }
            //SetGeometry
            const int nTotalView = 21;//31;//11;
            DetectorLeftTop = new DataUtil.tomoData.VECTOR3f[nTotalView];

            VOIInfo.nWidth = 2048;//1024;//512;
            VOIInfo.nHeight = 2048;//1024;//512;
            VOIInfo.nDepth = 50;
            VOIInfo.fPixelSpacing = 0.2f;//0.2f;
            VOIInfo.fSliceDis = 1.0f;
            VOIInfo.FirstSliceLeftTop.x = -VOIInfo.fPixelSpacing * VOIInfo.nWidth / 2;
            VOIInfo.FirstSliceLeftTop.y = -VOIInfo.fPixelSpacing * VOIInfo.nHeight / 2;
            VOIInfo.FirstSliceLeftTop.z = 1.0f;

            TOMOInfo.nWidth = 2048;//1024;
            TOMOInfo.nHeight = 2048;//1024;
            TOMOInfo.Axis.x = TOMOInfo.Axis.y = TOMOInfo.Axis.z = 0;
            TOMOInfo.fPixelSpacing = 0.2f;//0.2f;
            TOMOInfo.fRadius = 1000.0f;// 100.0f;
            //TOMOInfo.nStartAngle = 30;//30;//15;//15;手从-30开始，胸腔从+30开始
            //TOMOInfo.nStepAngle = -3;//-3;//-1;
            TOMOInfo.nStartAngle = -30;//30;//15;//15;手从-30开始，胸腔从+30开始
            TOMOInfo.nStepAngle = 3;//-3;//-1;
            TOMOInfo.nTotalView = nTotalView;
            for (int nView = 0; nView < nTotalView; nView++)
            {
                DetectorLeftTop[nView].x = -TOMOInfo.fPixelSpacing * TOMOInfo.nWidth / 2;
                DetectorLeftTop[nView].y = -TOMOInfo.fPixelSpacing * TOMOInfo.nHeight / 2;
                DetectorLeftTop[nView].z = -10.0f;//-115.0f;// 胸腔在VOI下方11.5cm处，手在1cm处
            }
            if (!Reconstruction.Reconstruction.SetGeometry(ref VOIInfo, ref TOMOInfo, ref DetectorLeftTop))
            {
                this.toolStripStatusLabel1.Text = "SetGeometry失败";
            }
            else
            {
                this.toolStripStatusLabel1.Text = "SetGeometry失败";
            }
            //GetPeakMemory
            System.UInt32[] CPUMemory = new UInt32[1] { 0 };
            if (!Reconstruction.Reconstruction.GetPeakMemory(CPUMemory))
            {
                //System.Environment.Exit(1);
                this.toolStripStatusLabel1.Text = "GetPeakMemory()失败!";
            }
            else
            {
                this.toolStripStatusLabel1.Text = "Tomo所需内存为：" + CPUMemory + " Mb";
            }
            //InitialTomo
            if (!Reconstruction.Reconstruction.InitialTomo())
            {
                this.toolStripStatusLabel1.Text = "InitialTomo()失败！";
                //system("PAUSE");return -1;
            }
            else this.toolStripStatusLabel1.Text = "InitialTomo()成功";
            //InputProjectionByView
            int nOriPriSize = TOMOInfo.nWidth * TOMOInfo.nHeight;

            //unsigned short *pPrj = (unsigned short*)malloc(sizeof(unsigned short) * nOriPrjSize);
            //unsigned short *pDose = (unsigned short*)malloc(sizeof(unsigned short) * nOriPrjSize);
            //pBlankScan=pDose
            //pNormalScan=pPrj
            pNormalScan = new System.UInt16[nOriPriSize];
            pBlankScan = new System.UInt16[nOriPriSize];
            for (int i = 0; i < TOMOInfo.nTotalView; i++)
            {
                BinaryReader blankReader = new BinaryReader(File.Open((blankPath + (i + 1) + ".raw"), FileMode.Open));
                BinaryReader originalReader = new BinaryReader(File.Open((originalPath + (i + 1) + ".raw"), FileMode.Open));
                int j = 0;
                while (j < TOMOInfo.nWidth * TOMOInfo.nHeight)
                {
                    pBlankScan[i] = blankReader.ReadUInt16();
                    pNormalScan[i] = originalReader.ReadUInt16();
                    i++;
                }
                if (!Reconstruction.Reconstruction.InputProjections(pBlankScan, pNormalScan))
                {
                    this.toolStripStatusLabel1.Text = "InputProjections()失败";
                }
            }
            this.toolStripStatusLabel1.Text = "InputProjectionByView()成功";
            //MakeReconstruction
            const int nIteration = 8;
            const int nCorrectLevel = 5;
            if (!Reconstruction.Reconstruction.MakeReconstruction(nIteration, nCorrectLevel))
            {
                this.toolStripStatusLabel1.Text = "MakeReconstruction()失败!";
            }
            int nSliceSize = VOIInfo.nWidth * VOIInfo.nHeight;


            for (int i = 0; i < VOIInfo.nDepth; i++)
            {
                System.UInt16[] pExport = Reconstruction.Reconstruction.ExportBySlice(i);
                //unsigned short *pExport = ExportBySlice(i);
                if (pExport == null)
                {
                    this.toolStripStatusLabel1.Text = "ExportBySlice()失败";
                }
                //BinaryWriter sliceWriter = new BinaryWriter();
                //sliceWriter.Write(
                //TextWriter slicewriter = new StringWriter();
                //System.IO.MemoryStream sliceWriter = new System.IO.MemoryStream(
                StreamWriter slice = new StreamWriter("E:\\工作\\实习\\Data\\slice" + i + 1 + "\\.raw");
                slice.Write(pExport);

            }
            this.toolStripStatusLabel1.Text = "重建完成，点击Next查看不同层面重建结果。";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Random rd = new Random();
            int iofImage = rd.Next(21)+1;
            string ww = this.textBox1.Text.ToString();
            string wl = this.textBox2.Text.ToString();
            if (ww != "" && wl != "")
            {
                Load loadNewData = new Load();
                loadNewData.Wl = int.Parse(wl);
                loadNewData.Ww = int.Parse(ww);
               
                this.pictureBox2.Image = loadNewData.load(iofImage);
                this.groupBox3.Visible = true;
                this.textBox2.Clear();
                this.textBox1.Clear();
                this.toolStripStatusLabel1.Text = "Windows Width:" + ww + ";Windows Location:" + wl + ".";
            }
            else
            {
                double WW = (double)this.hScrollBar1.Value / this.hScrollBar1.Maximum;
                double WL = (double)this.hScrollBar2.Value / this.hScrollBar1.Maximum;
                Load loadNewData = new Load();
                loadNewData.RateWW = WW;
                loadNewData.RateWL = WL;
                this.pictureBox2.Image = loadNewData.load(iofImage);
                this.groupBox3.Visible = true;
                this.textBox1.Clear();
                this.textBox2.Clear();
                this.toolStripStatusLabel1.Text = "Windows Width:" + loadNewData.Ww + ";Windows Location:" + loadNewData.Wl + ".";
            }

        }

    }
}
