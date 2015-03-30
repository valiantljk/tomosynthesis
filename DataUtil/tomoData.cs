using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
namespace DataUtil
{
    /// <summary>
    /// AUTHOR: Jialin Liu
    /// DATE: OCT.29. 2010
    /// DLL: Sheng Ye
    /// COMPANY: E-COM
    /// </summary>
    public class tomoData
    {
        //定义托管状态下的结构体，使之与API中类型相同，从而可以向托管状态下的动态链接库传递一个用户定义的复杂数据类型；
        //数据类型的排列有三种，分别是automatic，sequence，explicit，此处选择第二种顺序排列
        //三维矢量(点）
        [StructLayout(LayoutKind.Sequential)]
        public struct VECTOR3f
        {
            public float x;
            public float y;
            public float z;
        };
        //重建容积体信息
        [StructLayout(LayoutKind.Sequential)]
        public struct VOI_INFO
        {
            public System.Int32 nWidth;//容积体断层的宽
            public System.Int32 nHeight;//容积体断层的高
            public System.Int32 nDepth;
            public System.Single fPixelSpacing;//容积体断层上分辨率(单位：mm）
            public System.Single fSliceDis;//容积体断层层间距（单位：mm）
            public VECTOR3f FirstSliceLeftTop;//最靠近平板层面的左上角顶点坐标（单位：mm）
        }
        //TOMO采集系统信息
        [StructLayout(LayoutKind.Sequential)]
        public struct TOMO_INFO
        {
            public VECTOR3f Axis;//球管旋转的中心（为简化模型，这里把旋转轴定为y轴，即球管在ZOX平面内绕中心转动）
            public System.Single fRadius;//球管绕中心旋转的半径（单位：mm）
            public System.Int32 nStartAngle;//球管起始角度(单位：°,以Z轴为0度，从Y轴负向朝正向看，在Z轴右侧的为正、在左侧的为负)
            public System.Int32 nStepAngle;//球管旋转步进角度(单位：°,从Y轴负向朝正向看，逆时针旋转为负、正时针旋转为正)
            public System.Int32 nTotalView;//投照总次数
            public System.Single fPixelSpacing;//平板探测器的XOY分辨率（单位：mm）
            public System.Int32 nWidth;//平板探测器的宽
            public System.Int32 nHeight;//平板探测器的高
        }
        //定义测试数据的头文件
        public struct headData
        {
            public string path;
            TOMO_INFO tomo_info;
        }
        //定义测试数据的原文件
        public struct rawData
        {
            System.Int16[] NonProjection;
            System.Int16[] FullProjection;

        }

    }
}
