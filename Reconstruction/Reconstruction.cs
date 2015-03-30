using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using DataUtil;
namespace Reconstruction
{
    /// <summary>
    /// AUTHOR: Jialin Liu
    /// DATE: OCT.29. 2010
    /// DLL: Sheng Ye
    /// COMPANY: E-COM
    /// </summary>
    public unsafe class Reconstruction
    {      
       //函数1
       //初始化硬件系统
       //extern "C" __declspec (dllexport)  BOOL InitialHardWare()
        [DllImport("CUDATomo.dll", EntryPoint = "InitialHardWare", CharSet = CharSet.Auto, ExactSpelling = false,
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern System.Boolean InitialHardWare();

        //函数2
        //设定重建容积体和采集系统参数
        //参数： VOI				重建容积体信息
        //		 TOMO				采集系统信息
        //		 DetectorLeftTop	探测器从起始角开始，在各投影角度下的左上顶点坐标（单位：mm）
        //extern "C" __declspec (dllexport)  BOOL SetGeometry(VOI_INFO VOIInfo, TOMO_INFO TOMOInfo, VECTOR3f *DetectorLeftTop);
        //注意结构作为参数时候，一般前面要加上ref修饰符，否则会出现错误
        [DllImport("CUDATomo.dll",EntryPoint="SetGeometry",CharSet=CharSet.Auto,ExactSpelling=false,
            SetLastError =true,CallingConvention=CallingConvention.StdCall)]
        public static extern System.Boolean SetGeometry( DataUtil.tomoData.VOI_INFO VOIInfo,  DataUtil.tomoData.TOMO_INFO TOMOInfo,  DataUtil.tomoData.VECTOR3f[] DetectorLeftTop);

        //函数3
        //根据重建容积和采集系统参数，对所需内存进行预估
        //Tomo组件将根据当前容积的实际情况，自动选择是否要进行分块处理
        //参数：CPUMemory			预估所需内存量（单位：M)
        //extern "C" __declspec (dllexport) BOOL GetPeakMemory(PUINT CPUMemory);
        [DllImport("CUDATomo.dll", EntryPoint = "GetPeakMemory", CharSet = CharSet.Auto, ExactSpelling = false,
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern System.Boolean GetPeakMemory(System.UInt32[] CPUMemory);

        //函数4
        //初始化Tomo重建系统
        //extern "C" __declspec (dllexport) BOOL InitialTomo();
        [DllImport("CUDATomo.dll", EntryPoint = "InitialTomo", CharSet = CharSet.Auto, ExactSpelling = false,
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern System.Boolean InitialTomo();

        //函数5
        //按投影角顺序输入各角度下空扫描投影和全部的正常扫描投影
        //参数：pBlankScan			某投影角度下的空扫描数据
        //		pNormalScan			某投影角度下的正常扫描数据
        //extern "C" __declspec (dllexport) BOOL InputProjections(PWORD pBlankScan, PWORD pNormalScan);
        [DllImport("CUDATomo.dll", EntryPoint = "InputProjections", CharSet = CharSet.Auto, ExactSpelling = false,
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        //public static extern System.Boolean InputProjections( System.IntPtr pBlankScan, System.IntPtr pNormalScan);
        public static extern System.Boolean InputProjections(System.UInt16[] pBlankScan, System.UInt16[] pNormalScan);

        //函数6
        //进行MLEM重建
        //参数：nIteration			MLEM的迭代次数
        //		nCorrectLevel		修正等级（0为无修正、10为最强修正、默认为5）
        //extern "C" __declspec (dllexport) BOOL MakeReconstruction(INT nIteration, INT nCorrectLevel = 5);
        [DllImport("CUDATomo.dll", EntryPoint = "MakeReconstruction", CharSet = CharSet.Auto, ExactSpelling = false,
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        //[DllImport("CUDATomo.dlll)]
        public static extern System.Boolean MakeReconstruction(System.Int32 nIteration, System.Int32 nCorrectLevel);

        //函数7,8
        //输出指定层面的重建数据，第0层为VOI顶层，第nDepth - 1为VOI底层
        //参数：nSliceNum			需要输出的o指定层面
        //extern "C" __declspec (dllexport) PWORD ExportBySlice(INT nSliceNum);
        //extern "C" __declspec (dllexport) PWORD ExportGuessPrjByView(INT nViewNum);
        [DllImport("CUDATomo.dll", EntryPoint = "ExportBySlice", CharSet = CharSet.Auto, ExactSpelling = false,
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
       // [return: MarshalAs(UnmanagedType.LPTStr, SizeConst = 16)]
        //public static extern System.IntPtr ExportBySlice(System.Int32 nSliceNum);
        public static extern unsafe System.UInt16* ExportBySlice(System.Int32 nSliceNum);
        
        [DllImport("CUDATomo.dll", EntryPoint = "ExportGuessPrjByView", CharSet = CharSet.Auto, ExactSpelling = false,
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern System.UInt16[] ExportGuessPrjByView(System.Int32 nViewNum);
    }
}
