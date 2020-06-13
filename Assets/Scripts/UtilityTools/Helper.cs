﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace Helper
{
  // we use float[3] rather than Vector3 in order to store precise position data in the format of JSON
  #region Structs
  public struct Triangleids
  {
    public int A;
    public int B;
    public int C;

    public float AB;
    public float BC;
    public float CA;
  }

  public struct VertexData
  {
    public float[][] positions;
    public Triangleids[] sortedTriangles;
  }

  public struct UInt3Struct
  {
    public uint deltaXInt;
    public uint deltaYInt;
    public uint deltaZInt;
  }

  #endregion

  public class _Convert
  {

    public static float[] Vector3ToFloat(Vector3 vect)
    {
      return new float[3]{vect.x, vect.y, vect.z};
    }

    public static Vector3 FloatToVector3(float[] flts)
    {
      if (flts.Length == 3)
      {
        return new Vector3(flts[0], flts[1], flts[2]);
      } else
      {
        Debug.LogError("Float passed does not have exactly length of 3");
        return new Vector3(0, 0, 0);
      }
    }

    public static Vector3[] FloatArrayToVector3Array(float[][] fltArray)
    {
      if (fltArray != null)
      {
        Vector3[] vc3Array = new Vector3[fltArray.Length];

        for (int i=0; i < fltArray.Length; i++)
        {
          vc3Array[i] = FloatToVector3(fltArray[i]);
        }

        return vc3Array;
      } else
      {
        return new Vector3[0];
      }
    }

    public static float[][] Vector3ArrayToFloatArray(Vector3[] vc3Array)
    {
      if (vc3Array != null)
      {
        float[][] fltArray = new float[vc3Array.Length][];

        for (int i=0; i < vc3Array.Length; i++)
        {
          fltArray[i] = Vector3ToFloat(vc3Array[i]);
        }

        return fltArray;
      } else
      {
        return new float[0][];
      }
    }

    public static Vector3[] LocalVector3ListToWold(Transform transform, Vector3[] vc3List)
    {
      for (int i=0; i < vc3List.Length; i++)
      {
        vc3List[i] = transform.TransformPoint(vc3List[i]);
      }
      return vc3List;
    }

    public static Vector3[] WoldVector3ListToLocal(Transform transform, Vector3[] vc3List)
    {
      for (int i=0; i < vc3List.Length; i++)
      {
        vc3List[i] = transform.InverseTransformPoint(vc3List[i]);
      }
      return vc3List;
    }

  }
  
  public class _Check
  {

    public static bool ListContainsFloatArray(List<float[]> list, float[] arr, out int element)
    {
      // return element id if found similar array, return element length if no similar array found
      for (int i=0; i < list.Count; i++)
      {
        if (list[i].SequenceEqual(arr))
        {
          element = (int)i;
          return true;
        }
      }
      element = (int)list.Count;
      return false;
    }

    public static List<int[]> RemoveSimilarTriangleGrps(List<int[]> sortedTri)
    {
      for (int i=0; i < sortedTri.Count; i++)
      {
        for (int _i=0; _i < sortedTri.Count; _i++)
        {
          bool similar = true;
          for (int _ii=0; _ii < 3; _ii++)
          {
            if (!sortedTri[i].Contains(sortedTri[_i][_ii]))
            {
              similar = false;
              break;
            }
          }
          if (similar)
          {
            sortedTri.RemoveAt(_i);
          }
        }
      }
      return sortedTri;
    }

  }

  public class _Vertex
  {

    public static VertexData SortTrianglesByGrp(int[] tri, int totalTrianglePoints, VertexData vd)
    {
      if (totalTrianglePoints % 3 == 0)
      {
        vd.sortedTriangles = new Triangleids[totalTrianglePoints/3];

        for (int i=0; i < totalTrianglePoints; i+=3)
        {
          Triangleids t = new Triangleids();
          t.A = tri[i];
          t.B = tri[i+1];
          t.C = tri[i+2];

          t.AB = CalculateCustomidDistance(t.A, t.B, vd);
          t.BC = CalculateCustomidDistance(t.B, t.C, vd);
          t.CA = CalculateCustomidDistance(t.C, t.A, vd);
          vd.sortedTriangles[i/3] = (t);
        }
      }
      return vd;
    }

    public static void InitRawMesh(Mesh mesh, out int totalVerts, out int totalTrianglePoints)
    {
      totalVerts = mesh.vertexCount;
      totalTrianglePoints = mesh.triangles.Length;
    }

    public static void ResetMeshData(float[][] positions, Mesh mesh)
    {
      mesh.vertices = _Convert.FloatArrayToVector3Array(positions);
      mesh.RecalculateNormals();
    }

    public static float CalculateCustomidDistance(int id1, int id2, VertexData vd)
    {
      return Vector3.Distance(_Convert.FloatToVector3(vd.positions[id1]), _Convert.FloatToVector3(vd.positions[id2]));
    }

    #region Saving and Loading
    public static bool CheckSaveFolder(string folder)
    {
      if (folder != "" && folder != null)
      {
        if (!Directory.Exists($"{Application.streamingAssetsPath}/{folder}"))
        {
          Directory.CreateDirectory($"{Application.streamingAssetsPath}/{folder}");
        }
        return true;
      } else
      {
        Debug.LogError("Please specify the folder you want to save at in the inspector");
        return false;
      }
    }

    public static void SaveData(string filename,  string folder, VertexData vd)
    {
      if (CheckSaveFolder(folder))
      {
        string savePath = $"{Application.streamingAssetsPath}/{folder}/{filename}.json";
        string json = JsonConvert.SerializeObject(vd);
        File.WriteAllText(savePath, json);
        Debug.Log($"Cached particles data at {savePath}");
      }
    }

    public static VertexData LoadData(string filename, string folder)
    {
      if (CheckSaveFolder(folder))
      {
        string savePath = $"{Application.streamingAssetsPath}/{folder}/{filename}.json";
        string json = File.ReadAllText(savePath);
        VertexData vd = JsonConvert.DeserializeObject<VertexData>(json);
        Debug.Log($"Loaded cached particles data at {savePath}");
        return vd;
      } else
      {
        Debug.LogError($"An error occured when loading VertexData data from {filename}");
        return new VertexData();
      }
    }
    #endregion

  }

  public class _Mesh
  {

    public static Mesh DeepCopyMesh(Mesh mesh)
    {
      Mesh newMesh = new Mesh();
      newMesh.vertices = mesh.vertices;
      newMesh.triangles = mesh.triangles;
      newMesh.RecalculateBounds();
      newMesh.RecalculateNormals();
      newMesh.RecalculateTangents();
      return newMesh;
    }

    public static GameObject CreateBackSide(GameObject parent)
    {
      GameObject child = new GameObject(parent.name + "_BACK");
      child.transform.parent = parent.transform;
      child.transform.localPosition = Vector3.zero;
      child.transform.localRotation = Quaternion.identity;
      child.transform.localScale = new Vector3(1, 1, 1);
      child.AddComponent<SkinnedMeshRenderer>();
      child.GetComponent<SkinnedMeshRenderer>().material = parent.GetComponent<SkinnedMeshRenderer>().material;

      Mesh reverseMesh = new Mesh();
      reverseMesh = DeepCopyMesh(parent.GetComponent<SkinnedMeshRenderer>().sharedMesh);
      reverseMesh.MarkDynamic();

      // reverse the triangle order
      for (int m = 0; m < reverseMesh.subMeshCount; m++) {
        int[] triangles = reverseMesh.GetTriangles(m);
        for (int i = 0; i < triangles.Length; i += 3) {
          int temp = triangles[i + 0];
          triangles[i + 0] = triangles[i + 1];
          triangles[i + 1] = temp;
        }
        reverseMesh.SetTriangles(triangles, m);
      }
      child.GetComponent<SkinnedMeshRenderer>().sharedMesh = reverseMesh;
      return child;
    }

    public static Vector3[] ReverseNormals(Vector3[] normals)
    {
      Vector3[] reverseNormals = normals;
      for (int i = 0; i < reverseNormals.Length; i++)
      {
        reverseNormals[i] *= -1;
      }
      return reverseNormals;
    }

    public static void AutoWeld(Mesh mesh, float threshold)
    {
      Vector3[] verts = mesh.vertices;
      BoneWeight[] boneWeights = mesh.boneWeights;
      
      // Build new vertex buffer and remove "duplicate" verticies
      // that are within the given threshold.
      List<Vector3> newVerts = new List<Vector3>();
      List<BoneWeight> newBoneWeights = new List<BoneWeight>();
      List<Vector2> newUVs = new List<Vector2>();
      
      int k = 0;
      
      for (int i=0; i < mesh.vertexCount; i++)
      {
        // Has vertex already been added to newVerts list?
        foreach (Vector3 newVert in newVerts)
          if (Vector3.Distance(newVert, verts[i]) <= threshold) goto skipToNext;
        // Accept new vertex!
        newVerts.Add(verts[i]);
        newBoneWeights.Add(boneWeights[i]);
        newUVs.Add(mesh.uv[k]);

        skipToNext:;
        ++k;
      }
      
      // Rebuild triangles using new verticies
      int[] tris = mesh.triangles;
      for (int i = 0; i < tris.Length; ++i)
      {
        // Find new vertex point from buffer
        for (int j = 0; j < newVerts.Count; ++j)
        {
          if (Vector3.Distance(newVerts[j], verts[ tris[i] ]) <= threshold)
          {
            tris[i] = j;
            break;
          }
        }
      }
      
      // Update mesh!
      mesh.Clear();
      mesh.vertices = newVerts.ToArray();
      mesh.triangles = tris;
      mesh.boneWeights = newBoneWeights.ToArray();
      mesh.uv = newUVs.ToArray();
      mesh.RecalculateNormals();
      mesh.RecalculateBounds();
    }

  }

}