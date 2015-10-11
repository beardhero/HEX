﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PolySphere
{
  public List<Triangle> icosahedronTris;
  public List<List<Triangle>> subdividedTris;
  public List<Triangle> finalTris;    // The finest level of subdivided tris
  public List<Triforce> triforces;
  int scale, subdivisions;

  public PolySphere(int s, int d)
  {
    scale = s;
    subdivisions = d;
    icosahedronTris = Icosahedron(scale);
    Subdivide(d);
    //SubdivideAndDuals(d);
  }

  void Subdivide(int divisions)
  {
    List<Triangle> currentTris;
    List<Triangle> nextTris = new List<Triangle>(icosahedronTris);
    //List<Triforce> triforces;
    subdividedTris = new List<List<Triangle>>();

    // Subdivide icosahedron
    for (int i = 0; i < divisions; i++)
    {
      currentTris = new List<Triangle>(nextTris);
      nextTris = new List<Triangle>();
      //triforces = new List<Triforce>();
      
      // --- Create children ---
      foreach (Triangle tri in currentTris)
      {
        //Bisect
        Vector3 v1 = Vector3.Lerp(tri.v1, tri.v2, .5f);
        Vector3 v2 = Vector3.Lerp(tri.v2, tri.v3, .5f);
        Vector3 v3 = Vector3.Lerp(tri.v3, tri.v1, .5f);

        //Project onto sphere
        v1 *= (float)(1.902113 / v1.magnitude)*scale; //golden rectangle sphere radius 1.902113
        v2 *= (float)(1.902113 / v2.magnitude)*scale;
        v3 *= (float)(1.902113 / v3.magnitude)*scale;

        //Add the four new triangles
        Triangle mid = new Triangle(v1, v2, v3, tri, TriforcePosition.Mid, i+1);
        Triangle top = new Triangle(tri.v1, v1, v3, tri, TriforcePosition.Top, i+1);
        Triangle right = new Triangle(v1, tri.v2, v2, tri, TriforcePosition.Right, i+1);
        Triangle left = new Triangle(v3, v2, tri.v3, tri, TriforcePosition.Left, i+1);
       
        nextTris.Add(mid);   // Center of triforce
        nextTris.Add(top);
        nextTris.Add(right);
        nextTris.Add(left);

        tri.AssignChildren(mid, top, left, right);
        //These new triangles (along with the original, for reference later, make a triforce)
        //Triforce tf = new Triforce(tri, mid, top, right, left);
        //triforces.Add(tf);
      }

      // --- Assign neighbors ---
      foreach (Triangle tri in currentTris)
      {
        tri.childMid.AssignNeighbors(tri.childTop, tri.childRight, tri.childLeft);

        tri.childTop.AssignNeighbors(tri.top.childTop, null, tri.childMid);

        tri.childRight.AssignNeighbors(tri.childMid, tri.childMid, null);

        tri.childLeft.AssignNeighbors(tri.right.childRight, tri.childMid, tri.left.childLeft);
      }

      // --- Number tris ---
      int count = 0;
      foreach (Triangle t in nextTris)
      {
        t.index = count;
        count++;
      }

      // --- Add to subdivided output ---
      subdividedTris.Add(nextTris);
    }

    finalTris = nextTris;

    //finalTris.RemoveAt(finalTris[0].parent.childTop.top.index);
    //finalTris.RemoveAt(finalTris[0].parent.childRight.top.index);
    //finalTris.RemoveAt(finalTris[0].parent.childLeft.top.index);
  }

  void SubdivideAndDuals(int divisions)
  {
    List<Triangle> currentTris;
    List<Triangle> nextTris = new List<Triangle>(icosahedronTris);
    //List<Triforce> triforces;
    List<List<Triangle>> dualTris = new List<List<Triangle>>();

    // Subdivide icosahedron
    for (int i = 0; i < divisions; i++)
    {
      currentTris = new List<Triangle>(nextTris);
      nextTris = new List<Triangle>();
      //triforces = new List<Triforce>();

      foreach (Triangle tri in currentTris)
      {
        //Bisect
        Vector3 v1 = Vector3.Lerp(tri.v1, tri.v2, .5f);
        Vector3 v2 = Vector3.Lerp(tri.v2, tri.v3, .5f);
        Vector3 v3 = Vector3.Lerp(tri.v3, tri.v1, .5f);

        //Project onto sphere
        v1 *= (float)(1.902113 / v1.magnitude) * scale; //golden rectangle sphere radius 1.902113
        v2 *= (float)(1.902113 / v2.magnitude) * scale;
        v3 *= (float)(1.902113 / v3.magnitude) * scale;

        //Add the four new triangles
        Triangle mid = new Triangle(v1, v2, v3, tri, TriforcePosition.Mid, subdivisions);
        nextTris.Add(mid);   // Center of triforce

        Triangle top = new Triangle(tri.v1, v1, v3, tri, TriforcePosition.Top, subdivisions);
        nextTris.Add(top);

        Triangle right = new Triangle(v1, tri.v2, v2, tri, TriforcePosition.Right, subdivisions);
        nextTris.Add(right);

        Triangle left = new Triangle(v3, v2, tri.v3, tri, TriforcePosition.Left, subdivisions);
        nextTris.Add(left);

        tri.AssignChildren(mid, top, left, right);
        //These new triangles (along with the original, for reference later, make a triforce)
        //Triforce tf = new Triforce(tri, mid, n1, n2, n3);
        //triforces.Add(tf);
      }

      foreach (Triangle tri in nextTris)
      {

        tri.childMid.AssignNeighbors(tri.childTop, tri.childRight, tri.childLeft);
        tri.childTop.AssignNeighbors(tri.top.childLeft, tri.childMid, tri.left.childTop);
        tri.childRight.AssignNeighbors(tri.top.childRight, tri.right.childTop, tri.childMid);
        tri.childLeft.AssignNeighbors(tri.childMid, tri.right.childLeft, tri.left.childRight);
        
      }

      /*
      foreach (Triforce tf in triforces)
      {
        tf.AssignNeighbors(tf.original.nx.OriginalToTriforce(triforces), tf.original.ny.OriginalToTriforce(triforces), tf.original.nz.OriginalToTriforce(triforces));
      }
      //Once it's subdivided and new neighbors are ready to be assigned to mid tiles, 
      //Set new neighbors for remaining tiles based on old neighbors
      foreach (Triforce tf in triforces)
      {
        tf.top.AssignNeighbors(tf.mid, tf.ny.left, tf.nz.right);
        tf.right.AssignNeighbors(tf.mid, tf.nz.top, tf.nx.left);
        tf.left.AssignNeighbors(tf.mid, tf.nx.right, tf.ny.top);
        tf.mid.AssignNeighbors(tf.top, tf.right, tf.left);
      }
      nextTris = Duals(triforces);
      */
      dualTris.Add(nextTris);
    }
    finalTris = nextTris;
  }

  /*
  List<Triangle> Duals(List<Triforce> triforces)
  {
    List<Triangle> dualTris = new List<Triangle>();
    List<Hexagon> hexes = new List<Hexagon>();
    foreach (Triforce tf in triforces)
    {
      //So, here we're going to make triangles that then render to become the duals.  
      //Each hexagon in the dual polyhedron has 6 triangles that need to be rendered.
      //The thing about duals is that for every polygon there also exists a dual of that polygon,
      //  so we don't actually have to move any vertices around, just render the correct faces.
      //The Hexagon class will take 6 vertices and Hexagon.ToRender will give you the 6 triangles on its face.
      //Hexagon vertices have to be in the right order, with "ne" corresponding to the top vertex when looking vertex down at the hexagon.

      //For every triforce, make the hexagons.
      //Just trying to get this first one to work then seeing which others I have to add.
      //For every triforce, there are a possible 6 hexagons or 5 hexagons and a pentagon to make.  The inner three should be more than enough, with overlapping.

      hexes.Add(new Hexagon(tf.nz.right.center, tf.nz.mid.center, tf.nz.top.center, tf.right.center, tf.mid.center, tf.top.center));
      //hexes.Add()
      //hexes.Add()
    }
    //Get the triangles out of the hexes and return them
    foreach (Hexagon hex in hexes)
    {
      foreach (Triangle tri in (hex.ToRender()))
      {
        dualTris.Add(tri);
      }
    }
    return dualTris;
  }
  */

  List<Triangle> Icosahedron(int scale)
  {
    List<Triangle> output = new List<Triangle>();
    List<Vector3> vertices = new List<Vector3>();

    float goldRat = (1 + Mathf.Sqrt(5)) / 2;

    //Icosahedron coords
    Vector3 origin = Vector3.zero,
            xy1 = new Vector3(1, goldRat, 0) * scale,
            xy2 = new Vector3(1, -goldRat, 0) * scale,
            xy3 = new Vector3(-1, -goldRat, 0) * scale,
            xy4 = new Vector3(-1, goldRat, 0) * scale,
            xz1 = new Vector3(goldRat, 0, 1) * scale,
            xz2 = new Vector3(goldRat, 0, -1) * scale,
            xz3 = new Vector3(-goldRat, 0, -1) * scale,
            xz4 = new Vector3(-goldRat, 0, 1) * scale,
            zy1 = new Vector3(0, 1, goldRat) * scale,
            zy2 = new Vector3(0, 1, -goldRat) * scale,
            zy3 = new Vector3(0, -1, -goldRat) * scale,
            zy4 = new Vector3(0, -1, goldRat) * scale;

    vertices.Add(origin);         // 0
    vertices.Add(origin + xy1);   // 1
    vertices.Add(origin + xy2);   // 2
    vertices.Add(origin + xy3);   // 3
    vertices.Add(origin + xy4);   // 4
    vertices.Add(origin + xz1);   // 5
    vertices.Add(origin + xz2);   // 6
    vertices.Add(origin + xz3);   // 7
    vertices.Add(origin + xz4);   // 8
    vertices.Add(origin + zy1);   // 9
    vertices.Add(origin + zy2);   // 10
    vertices.Add(origin + zy3);   // 11
    vertices.Add(origin + zy4);   // 12

    //(float)(1.902113 / v3.magnitude)*scale

    // === Faces of the Original 3 Triforces ===
    output.Add(new Triangle(vertices[1], vertices[6], vertices[10]));   // 0
    output.Add(new Triangle(vertices[1], vertices[10], vertices[4]));   // 1
    output.Add(new Triangle(vertices[1], vertices[4], vertices[9]));    // 2
    output.Add(new Triangle(vertices[4], vertices[8], vertices[9]));    // 3
    output.Add(new Triangle(vertices[9], vertices[8], vertices[12]));   // 4
    output.Add(new Triangle(vertices[6], vertices[11], vertices[10]));  // 5
    output.Add(new Triangle(vertices[1], vertices[5], vertices[6]));    // 6
    output.Add(new Triangle(vertices[4], vertices[7], vertices[8]));    // 7
    output.Add(new Triangle(vertices[3], vertices[2], vertices[12]));   // 8
    output.Add(new Triangle(vertices[3], vertices[11], vertices[2]));   // 9
    output.Add(new Triangle(vertices[5], vertices[12], vertices[2]));   // 10
    output.Add(new Triangle(vertices[3], vertices[12], vertices[8]));   // 11
    output.Add(new Triangle(vertices[3], vertices[8], vertices[7]));    // 12
    output.Add(new Triangle(vertices[3], vertices[7], vertices[11]));   // 13
    output.Add(new Triangle(vertices[9], vertices[12], vertices[5]));   // 14
    output.Add(new Triangle(vertices[10], vertices[7], vertices[4]));   // 15
    output.Add(new Triangle(vertices[5], vertices[2], vertices[6]));    // 16
    output.Add(new Triangle(vertices[6], vertices[2], vertices[11]));   // 17
    output.Add(new Triangle(vertices[1], vertices[9], vertices[5]));    // 18
    output.Add(new Triangle(vertices[10], vertices[11], vertices[7]));  // 19


    // Assign neighbors
     output[0].AssignNeighbors(output[1], output[4], output[18]);
     output[1].AssignNeighbors(output[2], output[0], output[10]);
     output[2].AssignNeighbors(output[1], output[12],output[3]);
     output[3].AssignNeighbors(output[2], output[14],output[4]);
     output[4].AssignNeighbors(output[3], output[16],output[0]);
     output[5].AssignNeighbors(output[19],output[6],output[9]);
     output[6].AssignNeighbors(output[5], output[17],output[7]);
     output[7].AssignNeighbors(output[6], output[15],output[8]);
     output[8].AssignNeighbors(output[7], output[13],output[9]);
     output[9].AssignNeighbors(output[8], output[11],output[5]);
    output[10].AssignNeighbors(output[1], output[19],output[11]);
    output[11].AssignNeighbors(output[10],output[9], output[12]);
    output[12].AssignNeighbors(output[11],output[13],output[2]);
    output[13].AssignNeighbors(output[12],output[8], output[14]);
    output[14].AssignNeighbors(output[13],output[15],output[3]);
    output[15].AssignNeighbors(output[14],output[7], output[16]);
    output[16].AssignNeighbors(output[15],output[17],output[4]);
    output[17].AssignNeighbors(output[16],output[6], output[18]);
    output[18].AssignNeighbors(output[17],output[19],output[0]);
    output[19].AssignNeighbors(output[18],output[5], output[10]);

    // --- Number tris ---
    int count = 0;
    foreach (Triangle t in output)
    {
      t.index = count;
      count++;
    }

    return output;
  }
}