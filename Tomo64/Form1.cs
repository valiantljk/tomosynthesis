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
using System.Threading;
using System.Diagnostics;
namespace Tomo64
{
    /// <summary>
    /// AUTHOR: Jialin Liu
    /// DATE: OCT.29. 2010
    /// DLL: Sheng Ye
    /// COMPANY: E-COM
    /// </summary>
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            string totalview = this.numericUpDown6.Value.ToString();
            int total = int.Parse(totalview);
            string npath = this.numericUpDown7.Value.ToString();
            int nPath = int.Parse(npath);
            string startangle = this.textBox11.Text.ToString();
            int startAngle = int.Parse(startangle);
            string stepangle = this.textBox10.Text.ToString();
            int stepAngle = int.Parse(stepangle);
            string npixel = this.textBox16.Text.ToString();
            float nPixelvoi = float.Parse(npixel);
            string npixelTomo = this.textBox9.Text.ToString();
            float nPixelTomo = float.Parse(npixelTomo);
            string voiwidth = this.textBox8.Text.ToString();
            int Voiwidth = int.Parse(voiwidth);
            string voiheight = this.textBox7.Text.ToString();
            int Voiheight = int.Parse(voiheight);
            int nTotalView = this.numericUpDown6.Value > 0 ? total : 21;
            VOIInfo.nWidth = Voiwidth;//1024;//512;
            VOIInfo.nHeight = Voiheight;//1024;//512;
            VOIInfo.nDepth = nPath > 0 ? nPath : 50;
            VOIInfo.fPixelSpacing = nPixelvoi;//0.2f;
            VOIInfo.fSliceDis = 1.0f;
            VOIInfo.FirstSliceLeftTop.x = -VOIInfo.fPixelSpacing * VOIInfo.nWidth / 2;
            VOIInfo.FirstSliceLeftTop.y = -VOIInfo.fPixelSpacing * VOIInfo.nHeight / 2;
            VOIInfo.FirstSliceLeftTop.z = 1.0f;
            TOMOInfo.nWidth = 2048;//1024;
            TOMOInfo.nHeight = 2048;//1024;
            TOMOInfo.Axis.x = TOMOInfo.Axis.y = TOMOInfo.Axis.z = 0;
            TOMOInfo.fPixelSpacing = nPixelTomo;//0.2f;
            TOMOInfo.fRadius = 1000.0f;// 100.0f;
            TOMOInfo.nStartAngle = startAngle;//30;//15;//15;手从-30开始，胸腔从+30开始
            TOMOInfo.nStepAngle = stepAngle;//-3;//-1;
            TOMOInfo.nTotalView = nTotalView;
        }
        private DataUtil.tomoData.VOI_INFO VOIInfo;//VOI容积体信息
        private DataUtil.tomoData.TOMO_INFO TOMOInfo;//TOMO系统信息
        private DataUtil.tomoData.VECTOR3f[] DetectorLeftTop;//平板信息
        private String blankPath;//空投影数据存放文件名
        private String originalPath;//正常投影数据存放文件名
        private Stopwatch Stw = new Stopwatch();//计时表
        private List<Bitmap> ReconstructionSliceBitmap = new List<Bitmap>();//重建后的图片
        private List<System.UInt16[]> ReconstructionSliceRawData;//重建后的原始数据
        private List<System.UInt16[]> ReconstructionSliceRawDataZoom;//重建后原始数据的线性插值
        private System.UInt16[] CommonRawDataZoom;//当前显示图像插值数据
        private List<String> dictory;//数据存放文件名集合
        private int countshowslice = 0;//当前显示计数；
        private Bitmap restoreProjection;
        private Bitmap restoreSlice;
        #region 初始化硬件
        private void InitialHardWareMethod()
        {
            if (Reconstruction.Reconstruction.InitialHardWare())
            {

                this.toolStripStatusLabel1.Text = "InitialHardWare成功";
                //Thread.Sleep(5000);
            }
            else
            {
                this.toolStripStatusLabel1.Text = "InitialHardWare失败";
                //Thread.Sleep(1000);
            }
        }
        #endregion
        #region 放大
        private void ZoomOut_Click_1(object sender, EventArgs e)
        {
            if (this.radioButton4.Checked)
            {
                if (this.pictureBox4.Image != null)
                {
                    this.pictureBox4.SizeMode = PictureBoxSizeMode.Zoom;
                    this.pictureBox4.Width *= 2;
                    this.pictureBox4.Height *= 2;
                }
            }
            else if (this.radioButton3.Checked)
            {
                if (this.pictureBox2.Image != null)
                {
                    this.pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
                    this.pictureBox2.Width *= 2;
                    this.pictureBox2.Height *= 2;
                }

            }
            else
            {
                MessageBox.Show("请先选择操作区域：点击中央区域的四个单选项之一");
            }
        }
        #endregion
        #region 缩小
        private void ZoomIn_Click(object sender, EventArgs e)
        {
            if (this.radioButton4.Checked)
            {
                if (this.pictureBox4.Image != null)
                {
                    this.pictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;
                    this.pictureBox4.Width /= 2;
                    this.pictureBox4.Height /= 2;
                }
            }
            else if (this.radioButton3.Checked)
            {
                if (this.pictureBox2.Image != null)
                {
                    this.pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                    this.pictureBox2.Width /= 2;
                    this.pictureBox2.Height /= 2;
                }
            }
            else
            {
                MessageBox.Show("请先选择操作区域：点击中央区域的四个单选项之一");
            }
        }
        #endregion
        #region 载入投影数据
        private void normalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                originalPath = this.folderBrowserDialog1.SelectedPath;
                this.toolStripStatusLabel1.Text = "将从“" + originalPath + "”文件夹中读入正常投影数据";
            }
        }
        private void blankToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.folderBrowserDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                blankPath = this.folderBrowserDialog1.SelectedPath;
                this.toolStripStatusLabel1.Text = "将从“" + blankPath + "”文件夹中读入空投影数据";
            }

        }
        #endregion
        #region 重建
        private void ReconstructionButton_Click(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            Thread thd = new Thread(new ThreadStart(RescontructionMethod));
            thd.Start();

        }

        private void RescontructionMethod()
        {
            string totalview = this.numericUpDown6.Value.ToString();
            int total = int.Parse(totalview);
            string npath = this.numericUpDown7.Value.ToString();
            int nPath = int.Parse(npath);
            string startangle = this.textBox11.Text.ToString();
            int startAngle = int.Parse(startangle);
            string stepangle = this.textBox10.Text.ToString();
            int stepAngle = int.Parse(stepangle);
            string npixel = this.textBox16.Text.ToString();
            float nPixelvoi = float.Parse(npixel);
            string npixelTomo = this.textBox9.Text.ToString();
            float nPixelTomo = float.Parse(npixelTomo);
            string voiwidth = this.textBox8.Text.ToString();
            int Voiwidth = int.Parse(voiwidth);
            string voiheight = this.textBox7.Text.ToString();
            int Voiheight = int.Parse(voiheight);
            int nTotalView = this.numericUpDown6.Value > 0 ? total : 21;
            VOIInfo.nWidth = Voiwidth;//1024;//512;
            VOIInfo.nHeight = Voiheight;//1024;//512;
            VOIInfo.nDepth = nPath > 0 ? nPath : 50;
            VOIInfo.fPixelSpacing = nPixelvoi;//0.2f;
            VOIInfo.fSliceDis = 1.0f;
            VOIInfo.FirstSliceLeftTop.x = -VOIInfo.fPixelSpacing * VOIInfo.nWidth / 2;
            VOIInfo.FirstSliceLeftTop.y = -VOIInfo.fPixelSpacing * VOIInfo.nHeight / 2;
            VOIInfo.FirstSliceLeftTop.z = 1.0f;
            TOMOInfo.nWidth = 2048;//1024;
            TOMOInfo.nHeight = 2048;//1024;
            TOMOInfo.Axis.x = TOMOInfo.Axis.y = TOMOInfo.Axis.z = 0;
            TOMOInfo.fPixelSpacing = nPixelTomo;//0.2f;
            TOMOInfo.fRadius = 1000.0f;// 100.0f;
            TOMOInfo.nStartAngle = startAngle;//30;//15;//15;手从-30开始，胸腔从+30开始
            TOMOInfo.nStepAngle = stepAngle;//-3;//-1;
            TOMOInfo.nTotalView = nTotalView;

            Thread inihardThread = new Thread(new ThreadStart(InitialHardWareMethod));
            inihardThread.Start();
            Thread.Sleep(500);
            DetectorLeftTop = new DataUtil.tomoData.VECTOR3f[nTotalView];
            for (int nView = 0; nView < nTotalView; nView++)
            {
                DetectorLeftTop[nView].x = -TOMOInfo.fPixelSpacing * TOMOInfo.nWidth / 2;
                DetectorLeftTop[nView].y = -TOMOInfo.fPixelSpacing * TOMOInfo.nHeight / 2;
                DetectorLeftTop[nView].z = -10.0f;//-115.0f;// 胸腔在VOI下方11.5cm处，手在1cm处
            }
            if (!Reconstruction.Reconstruction.SetGeometry(VOIInfo, TOMOInfo, DetectorLeftTop))
            {
                this.toolStripStatusLabel1.Text = "SetGeometry失败";
            }
            else
            {
                this.toolStripStatusLabel1.Text = "重建容积体及系统参数采集成功";
                Thread.Sleep(500);
            }
            //GetPeakMemory
            System.UInt32[] CPUMemory = new UInt32[1] { 0 };
            if (!Reconstruction.Reconstruction.GetPeakMemory(CPUMemory))
            {
                this.toolStripStatusLabel1.Text = "GetPeakMemory()失败!";
                Thread.Sleep(500);
            }
            else
            {
                this.toolStripStatusLabel1.Text = "Tomo所需内存为：" + CPUMemory[0] + " Mb";
                Thread.Sleep(500);
            }
            //InitialTomo
            if (!Reconstruction.Reconstruction.InitialTomo())
            {
                this.toolStripStatusLabel1.Text = "InitialTomo()失败！";
                Thread.Sleep(500);
            }
            else this.toolStripStatusLabel1.Text = "Tomo初始成功";
            Thread.Sleep(500);
            //InputProjectionByView
            int nOriPriSize = TOMOInfo.nWidth * TOMOInfo.nHeight;
            System.UInt16[] pNormalScan = new System.UInt16[nOriPriSize];
            System.UInt16[] pBlankScan = new System.UInt16[nOriPriSize];
            this.toolStripStatusLabel1.Text = "开始整合投影数据";
            if (Directory.Exists(originalPath) && Directory.Exists(blankPath))
            {
                for (int i = 0; i < TOMOInfo.nTotalView; i++)
                {

                    BinaryReader blankReader = new BinaryReader(File.Open((blankPath + "\\" + (i + 1) + ".raw"), FileMode.Open));
                    BinaryReader originalReader = new BinaryReader(File.Open((originalPath + "\\" + (i + 1) + ".raw"), FileMode.Open));
                    int j = 0;
                    while (j < TOMOInfo.nWidth * TOMOInfo.nHeight)
                    {
                        pBlankScan[j] = blankReader.ReadUInt16();
                        pNormalScan[j] = originalReader.ReadUInt16();
                        j++;
                    }

                    if (!Reconstruction.Reconstruction.InputProjections(pBlankScan, pNormalScan))
                    {
                        this.toolStripStatusLabel1.Text = "InputProjections()失败";
                    }
                }
                this.toolStripStatusLabel1.Text = "投影数据整合成功，开始重建";
                Thread.Sleep(500);
                this.progressBar1.Visible = true;
                this.progressBar1.PerformStep();
                //MakeReconstruction
                Thread ReconstructionThread = new Thread(new ThreadStart(BeginRecontruction));
                ReconstructionThread.Start();

            }

            else
            {
                this.toolStripStatusLabel1.Text = "error：请确定已输入投影数据，单击LoadData指定文件夹";
            }
        }

        private void BeginRecontruction()
        {
            int interation = int.Parse(this.numericUpDown4.Value.ToString());
            int correctlevel = int.Parse(this.numericUpDown5.Value.ToString());
            int nIteration = interation > -1 ? interation : 8;
            int nCorrectLevel = correctlevel > -1 ? correctlevel : 5;
            Stw.Start();
            if (!Reconstruction.Reconstruction.MakeReconstruction(nIteration, nCorrectLevel))
            {
                this.progressBar1.Visible = false;
                this.toolStripStatusLabel1.Text = "MakeReconstruction()失败!";

            }
            Stw.Stop();
            this.progressBar1.Visible = false;
            TimeSpan ts = Stw.Elapsed;
            int totaltime = ts.Minutes * 60 + ts.Seconds;
            this.toolStripStatusLabel1.Text = "重建完成！共计用时：" + totaltime + "Seconds";
        }
        #endregion
        #region 保存重建数据
        private void reconstrucionToolStripMenuItem_Click(object sender, EventArgs e)
        {

            string exportPath = "";
            DialogResult dr = folderBrowserDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                exportPath = this.folderBrowserDialog1.SelectedPath;
            }
            int nSliceSize = VOIInfo.nWidth * VOIInfo.nHeight;
            this.toolStripStatusLabel1.Text = "正在保存...";
            if (Directory.Exists(exportPath))
            {
                for (int i = 0; i < VOIInfo.nDepth; i++)
                {
                    FileStream fs = new FileStream((exportPath + "\\" + (i + 1) + ".raw"), FileMode.Create);
                    unsafe
                    {
                        System.UInt16* exportPointer = Reconstruction.Reconstruction.ExportBySlice(i);
                        for (int pointernum = 0; pointernum < nSliceSize; pointernum++)
                        {
                            System.Byte[] word = BitConverter.GetBytes(exportPointer[pointernum]);
                            fs.Write(word, 0, 2);

                        }

                    }

                }
                this.toolStripStatusLabel1.Text = "重建结果已输出并保存到硬盘，通过Loaddata->Reconstruction载入以查看不同层面重建结果。";

            }
            else
            {
                this.toolStripStatusLabel1.Text = "保存失败";
            }

        }
        #endregion
        #region 载入重建数据
        private void reconstructionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReconstructionSliceRawData = new List<ushort[]>();
            ReconstructionSliceRawDataZoom = new List<ushort[]>();
            string voiwidth = this.textBox8.Text.ToString();
            int Voiwidth = int.Parse(voiwidth);
            string voiheight = this.textBox7.Text.ToString();
            int Voiheight = int.Parse(voiheight);
            int w = Voiwidth > 0 ? Voiwidth : 2048;
            int h = Voiheight > 0 ? Voiheight : 2048;
            List<String> filepaths = new List<string>();
            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == DialogResult.OK)
                filepaths.AddRange(openFileDialog1.FileNames);
            Load ld = new Load();
            dictory = new List<string>();
            for (int filenum = 0; filenum < filepaths.Count; filenum++)
            {
                ReconstructionSliceRawData.Add(ld.LoadrawData(filepaths[filenum], w, h));
                dictory.Add(filepaths[filenum]);
            }

            for (int i = 0; i < ReconstructionSliceRawData.Count; i++)
            {
                System.UInt16[] temp = new ushort[2048 * 2048];
                temp = ReconstructionSliceRawData[i];
                ReconstructionSliceRawDataZoom.Add(ld.LinerInterpolation(temp, w, h));
            }
            this.toolStripStatusLabel1.Text = "重建容积体载入内存，单击Next查看";
        }
        #endregion
        #region 查看下一张
        private int countshowprojection = 0;
        private System.UInt16[] CommonproRawDataZoom;//当前显示图像插值数据 
        private void Nextslice_Click(object sender, EventArgs e)
        {
            if (this.radioButton4.Checked)
            {
                if (ReconstructionSliceRawDataZoom == null)
                    MessageBox.Show("请先通过Loaddata->Reconstruction载入重建结果");
                else
                {
                    countshowslice++;
                    if (countshowslice > ReconstructionSliceRawDataZoom.Count && countshowslice > 0)
                        countshowslice = 1;
                    ShowNextSlice(countshowslice);
                    if (this.pictureBox4.Image != null)
                    {
                        restoreSlice = new Bitmap(this.pictureBox4.Image);
                    }

                }

            }
            else if (this.radioButton3.Checked)
            {
                if (originalPath != "")
                {
                    countshowprojection++;

                    if (countshowprojection > 21)
                    {
                        countshowprojection = 1;
                    }
                    ShowNextSlice(countshowprojection);
                    if (this.pictureBox2.Image != null)
                    {
                        restoreProjection = new Bitmap(this.pictureBox2.Image);
                    }

                }
                else
                {
                    MessageBox.Show("请先通过Loaddata->projection->Normal载入投影数据");
                }
            }
            else
            {
                MessageBox.Show("请先选择操作区域：点击中央区域的四个单选项之一");
            }

        }

        private void ShowNextSlice(int i)
        {

            string ww = this.textBox1.Text.ToString();
            string wl = this.textBox2.Text.ToString();
            int WW = 0;
            int WL = 0;

            Load ld = new Load();

            int w = 257;
            int h = 268;

            if (radioButton4.Checked)
            {
                try
                {
                    CommonRawDataZoom = new ushort[w * h];
                    Bitmap bp = new Bitmap(w, h);
                    int totalslice = ReconstructionSliceRawDataZoom.Count;
                    if (totalslice > 0)
                    {
                        CommonRawDataZoom = ReconstructionSliceRawDataZoom[i - 1];
                        int[] range = ld.getmax(CommonRawDataZoom);
                        if (ww != "" && wl != "")
                        {
                            WW = int.Parse(ww);
                            WL = int.Parse(wl);
                            ld.Wl = WW;
                            ld.Ww = WL;
                        }
                        else
                        {
                            if (this.hScrollBar1.Value == 0 && this.hScrollBar2.Value == 0)
                            {
                                WW = range[1] - range[0];
                                WL = (int)WW / 2;
                                ld.Ww = WW;
                                ld.Wl = WL;
                            }
                            else
                            {
                                double rWW = (double)this.hScrollBar1.Value / this.hScrollBar1.Maximum;
                                double rWL = (double)this.hScrollBar2.Value / this.hScrollBar1.Maximum;
                                WW = (int)(rWW * (range[1] - range[0]) + 1);//窗宽
                                WL = (int)(rWL * (range[1] - range[0]));//窗位 
                                ld.Ww = WW;
                                ld.Wl = WL;
                            }

                        }
                        bp = ld.Extract(CommonRawDataZoom, w, h);
                        this.pictureBox4.Image = (Image)bp;
                        String current_filename = dictory[i - 1];
                        int xs = current_filename.LastIndexOf("\\");
                        string name1 = current_filename.Substring(xs + 1);

                        this.toolStripStatusLabel1.Text = "共载入" + totalslice + "张图,这是第" + i + "张图" + name1 + ".窗宽:" + WW + "窗位:" + WL;
                    }
                    else
                    {
                        this.toolStripStatusLabel1.Text = "未将重建结果载入内存";
                    }
                }
                catch (System.Exception e)
                {
                    MessageBox.Show("文件未找到或不符合要求");
                    this.toolStripStatusLabel1.Text = "文件未找到或不符合要求";
                }
            }
            else if (radioButton3.Checked)
            {
                CommonproRawDataZoom = new ushort[w * h];
                if (originalPath == null)
                {
                    MessageBox.Show("请将投影数据载入：LoadData->projection->Normal");
                }
                else
                {
                    try
                    {
                        System.UInt16[] temp = ld.LoadrawData((originalPath + "\\" + i + ".raw"), VOIInfo.nWidth, VOIInfo.nWidth);
                        CommonproRawDataZoom = ld.LinerInterpolation(temp, VOIInfo.nWidth, VOIInfo.nWidth);
                        int[] range = ld.getmax(CommonproRawDataZoom);
                        if (ww != "" && wl != "")
                        {
                            WW = int.Parse(ww);
                            WL = int.Parse(wl);
                            ld.Wl = WW;
                            ld.Ww = WL;
                        }
                        else
                        {
                            if (this.hScrollBar1.Value == 0 && this.hScrollBar2.Value == 0)
                            {
                                WW = range[1] - range[0];
                                WL = (int)WW / 2;
                                ld.Ww = WW;
                                ld.Wl = WL;
                            }
                            else
                            {
                                double rWW = (double)this.hScrollBar1.Value / this.hScrollBar1.Maximum;
                                double rWL = (double)this.hScrollBar2.Value / this.hScrollBar1.Maximum;
                                WW = (int)(rWW * (range[1] - range[0]) + 1);//窗宽
                                WL = (int)(rWL * (range[1] - range[0]));//窗位 
                                ld.Ww = WW;
                                ld.Wl = WL;
                            }

                        }
                        Bitmap bp = new Bitmap(w, h);
                        bp = ld.Extract(CommonproRawDataZoom, w, h);
                        this.pictureBox2.Image = (Image)bp;
                        this.toolStripStatusLabel1.Text = "这是第" + i + "张图";
                    }
                    catch (System.Exception e)
                    {
                        MessageBox.Show("文件未找到或不符合要求");
                        this.toolStripStatusLabel1.Text = "文件未找到或不符合要求";
                    }

                }



            }
        }
        #endregion
        #region 移动滑动条，动态改变窗宽窗位
        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            int WW = 0;
            int WL = 0;
            Load ld = new Load();
            int w = 257;
            int h = 268;
            double rWW = (double)this.hScrollBar1.Value / this.hScrollBar1.Maximum;
            double rWL = (double)this.hScrollBar2.Value / this.hScrollBar1.Maximum;
            if (radioButton4.Checked)
            {
                if (CommonRawDataZoom.Length > 0)
                {
                    int[] range = ld.getmax(CommonRawDataZoom);
                    WW = (int)(rWW * (range[1] - range[0]) + 1);//窗宽
                    WL = (int)(rWL * (range[1] - range[0]));//窗位 
                    ld.Ww = WW;
                    ld.Wl = WL;
                    this.pictureBox4.Image = (Image)ld.Extract(CommonRawDataZoom, w, h);
                    this.toolStripStatusLabel1.Text = "当前第" + countshowslice + "张图。" + "窗宽:" + WW + "窗位:" + WL;
                }
            }
            else if (radioButton3.Checked)
            {
                if (CommonproRawDataZoom.Length > 0)
                {
                    int[] range = ld.getmax(CommonproRawDataZoom);
                    WW = (int)(rWW * (range[1] - range[0]) + 1);//窗宽
                    WL = (int)(rWL * (range[1] - range[0]));//窗位 
                    ld.Ww = WW;
                    ld.Wl = WL;
                    this.pictureBox2.Image = (Image)ld.Extract(CommonproRawDataZoom, w, h);
                    this.toolStripStatusLabel1.Text = "当前第" + countshowprojection + "张图。" + "窗宽:" + WW + "窗位:" + WL;
                }
            }
            else
            {
                MessageBox.Show("请先选择操作区域：点击中央区域的四个单选项之一");
            }

        }

        private void hScrollBar2_Scroll(object sender, ScrollEventArgs e)
        {
            int WW = 0;
            int WL = 0;
            Load ld = new Load();
            int w = 257;
            int h = 268;
            double rWW = (double)this.hScrollBar1.Value / this.hScrollBar1.Maximum;
            double rWL = (double)this.hScrollBar2.Value / this.hScrollBar1.Maximum;
            if (radioButton4.Checked)
            {
                if (CommonRawDataZoom.Length > 0)
                {
                    int[] range = ld.getmax(CommonRawDataZoom);
                    WW = (int)(rWW * (range[1] - range[0]) + 1);//窗宽
                    WL = (int)(rWL * (range[1] - range[0]));//窗位 
                    ld.Ww = WW;
                    ld.Wl = WL;
                    this.pictureBox4.Image = (Image)ld.Extract(CommonRawDataZoom, w, h);
                    this.toolStripStatusLabel1.Text = "当前第" + countshowslice + "张图。" + "窗宽:" + WW + "窗位:" + WL;
                }
            }
            else if (radioButton3.Checked)
            {
                if (CommonproRawDataZoom.Length > 0)
                {
                    int[] range = ld.getmax(CommonproRawDataZoom);
                    WW = (int)(rWW * (range[1] - range[0]) + 1);//窗宽
                    WL = (int)(rWL * (range[1] - range[0]));//窗位 
                    ld.Ww = WW;
                    ld.Wl = WL;
                    this.pictureBox2.Image = (Image)ld.Extract(CommonproRawDataZoom, w, h);
                    this.toolStripStatusLabel1.Text = "当前第" + countshowprojection + "张图。" + "窗宽:" + WW + "窗位:" + WL;
                }
            }
            else
            {
                MessageBox.Show("请先选择操作区域：点击中央区域的四个单选项之一");
            }

        }
        #endregion
        #region 清除显示图片
        private void button2_Click(object sender, EventArgs e)
        {
            this.pictureBox4.Image = null;
            this.pictureBox2.Image = null;
            CommonRawDataZoom = null;
        }
        #endregion
        #region 恢复数据
        private void button3_Click(object sender, EventArgs e)
        {
            if (this.radioButton3.Checked)
            {
                if (restoreProjection != null)
                {
                    this.pictureBox2.Image = restoreProjection;
                    this.toolStripStatusLabel1.Text = "重建图片恢复成功" + "当前第" + countshowprojection + "张图。";
                }

            }
            else if (this.radioButton4.Checked)
            {
                if (restoreSlice != null)
                {
                    this.pictureBox4.Image = restoreSlice;
                    this.toolStripStatusLabel1.Text = "投影恢复成功" + "当前第" + countshowslice + "张图。";
                }

            }

        }
        #endregion

    }
}


//脏旧代码
//private void loadData_Button_Click(object sender, EventArgs e)
//{
//    string ww = this.textBox1.Text.ToString();
//    string wl = this.textBox2.Text.ToString();
//    if (ww != "" && wl != "")
//    {
//        Load loadNewData = new Load();
//        loadNewData.Wl = int.Parse(wl);
//        loadNewData.Ww = int.Parse(ww);
//        this.pictureBox2.Image = loadNewData.load();
//        this.groupBox3.Visible = true;
//        this.textBox2.Clear();
//        this.textBox1.Clear();
//        this.toolStripStatusLabel1.Text = "Windows Width:" + ww + ";Windows Location:" + wl + ".";
//    }
//    else
//    {
//        double WW = (double)this.hScrollBar1.Value / this.hScrollBar1.Maximum;
//        double WL = (double)this.hScrollBar2.Value / this.hScrollBar1.Maximum;
//        Load loadNewData = new Load();
//        loadNewData.RateWW = WW;
//        loadNewData.RateWL = WL;
//        this.pictureBox2.Image = loadNewData.load();
//        this.groupBox3.Visible = true;
//        this.textBox1.Clear();
//        this.textBox2.Clear();
//        this.toolStripStatusLabel1.Text = "Windows Width:" + loadNewData.Ww + ";Windows Location:" + loadNewData.Wl + ".";
//    }
//}

///// 设置鼠标单击的坐标，以及图片的坐标
///// 
//int mouseX;
//int mouseY;
//int picX;
//int picY;

///// 
///// 当鼠标单击时，给鼠标设定值。初始化。
///// 
///// 
///// 
//private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
//{
//    mouseX = Cursor.Position.X;
//    mouseY = Cursor.Position.Y;
//    picX = this.pictureBox2.Left;
//    picY = this.pictureBox2.Top;

//    //if (isMouseMoveEventAviable == false)
//    //    //添加鼠标移动事件
//    //    this.movablePic.MouseMove += this.movablePic_MouseMove;
//}

///// 
///// 根据鼠标的移动的值，设置
///// 
///// 
///// 
//private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
//{
//    int y = Cursor.Position.Y - mouseY + picY;
//    int x = Cursor.Position.X - mouseX + picX;
//    if (e.Button == MouseButtons.Left)
//    {
//        this.pictureBox2.Top = y;
//        this.pictureBox2.Left = x;
//    }
//}

//private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
//{
//    mouseX = 0;
//    mouseY = 0;
//    if (this.pictureBox2.Location.X < 0)
//    {
//        this.pictureBox2.Left = 0;

//    }
//    if (this.pictureBox2.Location.Y < 0)
//    {
//        this.pictureBox2.Top = 0;
//    }
//    if ((this.pictureBox2.Left + this.pictureBox2.Width) > this.ClientSize.Width)
//    {
//        this.pictureBox2.Left = this.ClientSize.Width - this.pictureBox2.Width;
//    }
//    if ((this.pictureBox2.Top + this.pictureBox2.Height) > this.ClientSize.Height)
//    {
//        this.pictureBox2.Top = this.ClientSize.Height - this.pictureBox2.Height;
//    }
//}

//private void pictureBox2_Click(object sender, EventArgs e)
//{

//}
