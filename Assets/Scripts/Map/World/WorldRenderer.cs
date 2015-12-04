﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldRenderer : MonoBehaviour
{
  public GameObject worldPrefab;
  //Zone currentZone;
  bool controlx;
  bool controly;
  bool controlz;

  PolySphere activePolySphere;

  public List<GameObject> RenderWorld(World world, TileSet tileSet)
  {
    List<GameObject> output = new List<GameObject>();

    for (int i=0; i<1; i++)
    {
      //StartCoroutine(RecursiveRender(world, tileSet, controlx, controly, controlz, i));
      output.Add(RecursiveRender(world, tileSet, controlx, controly, controlz, i));
      //Call our control function, which will iterate through the cyclic permutations to define 8 quadrants
      Cycle(controlx, controly, controlz);
    }
    return output;
  }

  public GameObject RecursiveRender(World world, TileSet tileSet, bool cx, bool cy, bool cz, int it)
  {
    //Debug.Log("ITERATION "+it);
    //currentZone = GameManager.currentZone;
    GameObject output = (GameObject)Instantiate(worldPrefab, Vector3.zero, Quaternion.identity);

    MeshFilter myFilter = output.GetComponent<MeshFilter>();
    MeshCollider myCollider = output.GetComponent<MeshCollider>();

    SerializableVector3 origin = Vector3.zero;
    Vector2 uv0 = Vector2.zero,
          uv1 = new Vector2(.5f, 1),
          uv2 = new Vector2(1, 0);

    Vector2 uvOffset = Vector3.zero;

    //LabelCenters(sphere.finalTris);
    //LabelNeighbors(sphere);

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector3> normals = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();


    //Generate quadrant
    foreach (HexTile ht in world.tiles)
    { 
      if (QuadrantActive(it))
      {
        //Debug.Log("Building quadrant "+it);

        // Origin point
        int originIndex = vertices.Count;
        vertices.Add(origin);
        uvs.Add(uv1+uvOffset);
        normals.Add(ht.hexagon.center - origin);

        // Center of hexagon
        int centerIndex = vertices.Count;

        // Triangle 1
        vertices.Add(ht.hexagon.center);
        normals.Add((origin + ht.hexagon.center));
        uvs.Add(uv1+uvOffset);

        vertices.Add(ht.hexagon.v1);
        normals.Add((origin + ht.hexagon.v1));
        uvs.Add(uv0 + uvOffset);

        vertices.Add(ht.hexagon.v2);
        normals.Add((origin + ht.hexagon.v2));
        uvs.Add(uv2 + uvOffset);

        triangles.Add(centerIndex);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 1);

        // T2
        vertices.Add(ht.hexagon.v3);
        normals.Add((origin + ht.hexagon.v3));
        uvs.Add(uv2 + uvOffset);

        triangles.Add(centerIndex);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 1);

        // T3
        vertices.Add(ht.hexagon.v4);
        normals.Add((origin + ht.hexagon.v4));
        uvs.Add(uv2 + uvOffset);

        triangles.Add(centerIndex);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 1);

        // T4
        vertices.Add(ht.hexagon.v5);
        normals.Add((origin + ht.hexagon.v5));
        uvs.Add(uv2 + uvOffset);

        triangles.Add(centerIndex);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 1);

        // T5
        vertices.Add(ht.hexagon.v6);
        normals.Add((origin + ht.hexagon.v6));
        uvs.Add(uv2 + uvOffset);

        triangles.Add(centerIndex);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 1);

        // T6
        triangles.Add(centerIndex);
        triangles.Add(vertices.Count - 1);
        triangles.Add(vertices.Count - 6);

        
        // Side 1
        triangles.Add(originIndex);
        triangles.Add(vertices.Count - 1);
        triangles.Add(vertices.Count - 2);

        // Side 2
        triangles.Add(originIndex);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 3);

        // Side 3
        triangles.Add(originIndex);
        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 4);

        // Side 4
        triangles.Add(originIndex);
        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 5);

        // Side 5
        triangles.Add(originIndex);
        triangles.Add(vertices.Count - 5);
        triangles.Add(vertices.Count - 6);

        // Side 6
        triangles.Add(originIndex);
        triangles.Add(vertices.Count - 6);
        triangles.Add(vertices.Count - 1);
      }
    }

    //GameObject centerMarker = (GameObject)GameObject.Instantiate(centerMarkerPrefab, tri.center, Quaternion.identity);
    Mesh m = new Mesh();
    m.vertices = vertices.ToArray();
    m.triangles = triangles.ToArray();
    m.normals = normals.ToArray();
    m.uv = uvs.ToArray();

    myCollider.sharedMesh = m;
    myFilter.sharedMesh = m;

    return output;
  }

  public void Cycle(bool x, bool y, bool z)
  {
    //8 quadrants
    if (!x && !y && !z)
    {      
      controlz = true;
      return;
    }
    if (!x && !y && z)
    {
      controlz = false;
      controly = true;
      return;
    }
    if (!x && y && !z)
    {
      controlz = true;
      return;
    }
    if (!x && y && z)
    {
      controlx = true;
      controly = false;
      controlz = false;
      return;
    }
    if (x && !y && !z)
    {
      controlz = true;
      return;
    }
    if (x && !y && z)
    {
      controly = true;
      controlz = false;
      return;
    }
    if (x && y && !z)
    {
      controlz = true;
      return;
    }
    if (x && y && z)
    {
      controlx = controly = controlz = false;
    }
  }

  bool QuadrantActive(int iteration)
  {
    switch (iteration)
    {
      case 0:
        return !controlx && !controly && !controlz;
      case 1:
        return !controlx && !controly && controlz;
      case 2:
        return !controlx && controly && !controlz;
      case 3:
        return !controlx && controly && controlz;
      case 4:
        return controlx && !controly && !controlz;
      case 5:
        return controlx && !controly && controlz;
      case 6:
        return controlx && controly && !controlz;
      default:
        return controlx && controly && controlz;
    }
  }

  /*
  void LabelNeighbors(PolySphere sphere)
  {
    
    Dictionary<Triangle, bool> neighborsLabeled = new Dictionary<Triangle, bool>();

    foreach (Triangle tri in sphere.finalTris)
    {
      if (neighborsLabeled.ContainsKey(tri) )
        continue;

      neighborsLabeled.Add(tri.instance, true);

      // Do the three immediate neighbors
      if (!neighborsLabeled.ContainsKey(tri.top))
      {
        //neighborsLabeled.Add(tri.top.instance, true);

        Vector3 midPointNX = (tri.center+tri.top.center) / 2;
        GameObject textObj = (GameObject)Instantiate(textMeshPrefab, midPointNX * 1.03f,
                                Quaternion.LookRotation(-midPointNX, tri.center-tri.top.center));
        textObj.GetComponent<TextMesh>().text = "|";
      }

      if (!neighborsLabeled.ContainsKey(tri.right))
      {
        //neighborsLabeled.Add(tri.right.instance, true);

        Vector3 midPointNY = (tri.center+tri.right.center) / 2;
        GameObject textObj = (GameObject)Instantiate(textMeshPrefab, midPointNY * 1.03f,
                                Quaternion.LookRotation(-midPointNY, tri.center-tri.right.center));
        textObj.GetComponent<TextMesh>().text = "|";
      }

      if (!neighborsLabeled.ContainsKey(tri.left))
      {
        //neighborsLabeled.Add(tri.left.instance, true);

        Vector3 midPointNZ = (tri.center+tri.left.center) / 2;
        GameObject textObj = (GameObject)Instantiate(textMeshPrefab, midPointNZ * 1.03f,
                                Quaternion.LookRotation(-midPointNZ, tri.center-tri.left.center));
        textObj.GetComponent<TextMesh>().text = "|";
      }
    }

    foreach (Triangle tri in sphere.finalTris)
    {
      float scale = tri.subdivisionLevel>0?1.055f : 1.36f;

      Vector3 midPointTop = (tri.center+tri.top.center) / 2,
              midPointRight = (tri.center+tri.right.center) / 2,
              midPointLeft = (tri.center+tri.left.center) / 2;

      GameObject textObj = (GameObject)Instantiate(textMeshPrefab, midPointTop * scale,
                              Quaternion.LookRotation(-midPointTop, tri.center-tri.top.center));
      textObj.GetComponent<TextMesh>().text = "|";

      Instantiate(textObj, midPointRight * scale, Quaternion.LookRotation(-midPointRight, tri.center - tri.right.center));
      Instantiate(textObj, midPointLeft * scale, Quaternion.LookRotation(-midPointLeft, tri.center-tri.left.center));
    }

  }
  */
  /*
  void LabelCenters(List<Triangle> tris)
  {
    foreach (Triangle t in tris)
    {
      GameObject textObj = (GameObject)Instantiate(textMeshPrefab, t.center * 1.01f, Quaternion.LookRotation(-1f*t.center));
      textObj.name = "Face "+t.index;
      textObj.GetComponent<TextMesh>().text = t.index.ToString();
    }
  }
  */
  
  public bool ControlX(float centerx)
  {
    if (controlx)
      return (centerx >= 0);
    else
      return (centerx < 0);
  }
  public bool ControlY(float centery)
  {
    if (controly)
      return (centery >= 0);
    else
      return (centery < 0);
  }
  public bool ControlZ(float centerz)
  {
    if (controlz)
      return (centerz >= 0);
    else
      return (centerz < 0);
  }
}

