using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarClipping : MonoBehaviour {




    /// <summary>
    /// 凸三角形切割法
    /// </summary>
    /// <param name="verts">三角形顶点数组</param>
    /// <returns></returns>
    public static int[] LimitedTriangleIndex(Vector3[] verts)
    {
        int len = verts.Length;
        if (len > 1 && Mathf.Equals(verts[0], verts[len - 1]))//如果头尾闭合，删除重复点
        {
            len--;
        }
        if (len < 3)
        {
            return new int[0];
        }

        int triangleNum = len - 2;//三角形数量

        int[] triangles = new int[triangleNum * 6];//正反两面所以要有6个顶点

        for (int i = 0; i < triangleNum; i++)
        {
            int start = i * 6;
            triangles[start] = 0;
            triangles[start+1] = i+1;
            triangles[start+2] = i+2;
            triangles[start+3] = 0;
            triangles[start+4] = i+2;
            triangles[start+5] = i+1;

        }

        return triangles;
    }


    /// <summary>
    /// 判断点是否在三角形内
    /// </summary>
    /// <param name="target">目标点</param>
    /// <param name="left">三角形左边的点</param>
    /// <param name="center">三角形中间的点</param>
    /// <param name="right">三角形右边的点</param>
    /// <returns></returns>
    public static bool InTrigon(Vector3 target, Vector3 left, Vector3 center, Vector3 right)
    {
        Vector3 c2l = left - center;
        Vector3 c2r = right - center;
        Vector3 c2t = target - center;
        Vector3 l2r = right - left;
        Vector3 l2c = center - left;
        Vector3 l2t = target - left;
        Vector3 r2l = right - left;
        Vector3 r2c = center - right;
        Vector3 r2t = target - right;
        //如果方向一样，目标点的长度小于边长说明目标点在边上，在三角形边上也算在三角形内
        if (c2l.normalized == c2t.normalized)
        {
            if (c2l.magnitude >= c2t.magnitude)
            {
                return true;
            }
        }
        else if (c2r.normalized == c2t.normalized)
        {
            if (c2r.magnitude >= c2t.magnitude)
            {
                return true;
            }
        }
        else if (l2r.normalized == l2t.normalized)
        {
            if (l2r.magnitude >= l2t.magnitude)
            {
                return true;
            }
        }
        //以边和点到目标点的向量作比较，两条边A在B左边的话目标点肯定在B的左边，这么判断，得到的法向量都是相同的，所以点乘等于1，这样的话点就在三角形内
        if (Vector3.Dot(Vector3.Cross(c2l, c2r).normalized, Vector3.Cross(c2l, c2t).normalized) == 1 &&
            Vector3.Dot(Vector3.Cross(l2r, l2c).normalized, Vector3.Cross(l2r, l2t).normalized) == 1 &&
            Vector3.Dot(Vector3.Cross(r2c, r2l).normalized, Vector3.Cross(r2c, r2t).normalized) == 1)
        {
            return true;
        }
        else
        {
            return false;
        }


    }

    public static int[] EarClipPolygonal(Vector3[] verts)
    {
        //例子集合用来找出耳尖下标
        List<Vector3> sampleVertsList = new List<Vector3>();
        //用来遍历耳朵的集合
        List<Vector3> vertsList = new List<Vector3>();
        //用来存放三角形点阵顺序
        List<int> triangleOrder = new List<int>();
        //判断是否有点在三角形内
        bool isInTrigon = false;
        //遍历数组把顶点分别放到例子和点的集合中
        for (int i = 0; i < verts.Length; i++)
        {
            vertsList.Add(verts[i]);
            sampleVertsList.Add(verts[i]);
        }
        //如果首位相交闭合，去除掉尾的点
        if (vertsList[0] == vertsList[vertsList.Count - 1])
        {
            vertsList.RemoveAt(vertsList.Count - 1);
        }
        //三角形数量
        int triangleNum = vertsList.Count - 2;
        //每次循环剪去一个耳朵，所以循环最大三角形数量次
        for (int k = 0; k < triangleNum; k++)
        {
            for (int i = 0; i < vertsList.Count-2; i++)
            {
                Vector3 a = vertsList[i];
                Vector3 b = vertsList[i + 1];
                Vector3 c = vertsList[i + 2];
                Vector3 ab = b - a;
                Vector3 bc = c - b;
                //顺时针情况下，两向量叉乘大于0，说明角度小于180度，是凸角
                if (Vector3.Cross(ab, bc).y > 0)
                {
                    List<Vector3> tempLish = new List<Vector3>();
                    //取出三角形3个点，拿其余的点遍历，看其余的点有没有在三角形内
                    tempLish.AddRange(vertsList);
                    tempLish.Remove(a);
                    tempLish.Remove(b);
                    tempLish.Remove(c);
                    for (int j = 0; j < tempLish.Count; j++)
                    {
                        if (InTrigon(tempLish[i], a, b, c))
                        {
                            isInTrigon = true;
                        }
                    }

                    if (isInTrigon == true)
                    {
                        isInTrigon = false;
                        continue;
                    }
                    //如果没有点在三角形内记录下顺序点位

                    int index1 = sampleVertsList.IndexOf(a);
                    int index2 = sampleVertsList.IndexOf(b);
                    int index3 = sampleVertsList.IndexOf(c);
                    triangleOrder.Add(index1);
                    triangleOrder.Add(index2);
                    triangleOrder.Add(index3);
                    //去除耳尖
                    vertsList.Remove(b);
                    break;
                }
                else//如果是凹交就跳过
                {
                    continue;
                }

            }

           

        }
        //如果剩余的点大于2个说明没取完点，按照我们的需求画的都是简单多边形，不存在三角形画一半的情况
        //所以只有一正一反两种情况，如果正面没画完说明就是逆时针画法，所以三角形叉乘<0为正常情况，
        //画出来的三角形在反面所以最后存点的时候只要逆时针存点就会显示在正面


        if (vertsList.Count > 2)
        {
            //例子集合用来找出耳尖下标
            sampleVertsList = new List<Vector3>();
            //用来遍历耳朵的集合
            vertsList = new List<Vector3>();
            //用来存放三角形点阵顺序
            triangleOrder = new List<int>();
            //判断是否有点在三角形内
            isInTrigon = false;
            //遍历数组把顶点分别放到例子和点的集合中
            for (int i = 0; i < verts.Length; i++)
            {
                vertsList.Add(verts[i]);
                sampleVertsList.Add(verts[i]);
            }
            //如果首位相交闭合，去除掉尾的点
            if (vertsList[0] == vertsList[vertsList.Count - 1])
            {
                vertsList.RemoveAt(vertsList.Count - 1);
            }
          
            //每次循环剪去一个耳朵，所以循环最大三角形数量次
            for (int k = 0; k < triangleNum; k++)
            {
                for (int i = 0; i < vertsList.Count - 2; i++)
                {
                    Vector3 a = vertsList[i];
                    Vector3 b = vertsList[i + 1];
                    Vector3 c = vertsList[i + 2];
                    Vector3 ab = b - a;
                    Vector3 bc = c - b;
                    //逆时针情况下，两向量叉乘小于0，说明角度小于180度，是凸角
                    if (Vector3.Cross(ab, bc).y < 0)
                    {
                        List<Vector3> tempLish = new List<Vector3>();
                        //取出三角形3个点，拿其余的点遍历，看其余的点有没有在三角形内
                        tempLish.AddRange(vertsList);
                        tempLish.Remove(a);
                        tempLish.Remove(b);
                        tempLish.Remove(c);
                        for (int j = 0; j < tempLish.Count; j++)
                        {
                            if (InTrigon(tempLish[i], a, b, c))
                            {
                                isInTrigon = true;
                            }
                        }

                        if (isInTrigon == true)
                        {
                            isInTrigon = false;
                            continue;
                        }
                        //如果没有点在三角形内记录下顺序点位

                        int index1 = sampleVertsList.IndexOf(a);
                        int index2 = sampleVertsList.IndexOf(c);
                        int index3 = sampleVertsList.IndexOf(b);
                        triangleOrder.Add(index1);
                        triangleOrder.Add(index2);
                        triangleOrder.Add(index3);
                        //去除耳尖
                        vertsList.Remove(b);
                        break;
                    }
                    else//如果是凹交就跳过
                    {
                        continue;
                    }

                }
            }
        }

        int[] triangles = new int[triangleOrder.Count];
        for (int i = 0; i < triangleOrder.Count; i++)
        {
            triangles[i] = triangleOrder[i];

        }

        return triangles;
    }


}
