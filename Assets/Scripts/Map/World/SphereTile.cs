using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LibNoise.Unity;
using LibNoise.Unity.Generator;
using LibNoise.Unity.Operator;

public class SphereTile
{
  public int index;    // The index of the tile in our map. Translates into HexTile.id [set by PolySphere]
  public int[] neighbors;   // Indexes of the surrounding sphere tiles in our map [set by PolySphere] in array form for serialization
  public Dictionary<int, SphereTile> neighborDict;    // A list of unique neighbors
  public List<SphereTile> neighborList;   // This is the first raw list, which will contain duplicates

  public bool colliding; //OnCollisionStay
  public TileType type;
  //The inital triangles from the subdivided polysphere which we will use to build the spheretile
  public List<Triangle> subTriangles;
  //The triangles that make up this piece of the entire dual polygon
  public List<Triangle> faceTris;
  public List<Triangle> sideTris;
  //Checking equality with the center vertex 
  public Vector3 center{ get; set; }
  public Vector3 origin;
  //Scaling property
  private float _scale = 1;
  public float scale
  {
    get { return _scale; }
    set
    {
      _scale = value;
      center.Normalize();
      center *= scale;
      foreach (Triangle t in faceTris)
      {
        t.v1.Normalize();
        t.v1 *= scale;
        t.v2.Normalize();
        t.v2 *= scale;
        t.v3.Normalize();
        t.v3 *= scale;
        t.center.Normalize();
        t.center *= scale;
      }
      foreach (Triangle t in sideTris)
      {
        t.v1.Normalize();
        t.v1 *= scale;
        t.v2.Normalize();
        t.v2 *= scale;
        t.v3.Normalize();
        t.v3 *= scale;
        t.center.Normalize();
        t.center *= scale;
      }
    }
  }

  //Unit SphereTile
  public SphereTile(Vector3 c, Vector3 o)
  {
    center = c;
    origin = o;
    faceTris = new List<Triangle>();
    neighbors = new int[7];
    neighborDict = new Dictionary<int, SphereTile>();
    neighborList = new List<SphereTile>();
    sideTris = new List<Triangle>();
    subTriangles = new List<Triangle>();

  }
  //Given the subdivided triangles, build the spheretile, which is made up of 12 triangles. 6 face, 6 side
  public void Build()
  {
    //simplex = new SimplexNoise(GameManager.gameSeed);
    List<Triangle> subCopies = new List<Triangle>(subTriangles);
    //unit vector in the direction of subCopies[0].center. Each vector will have the same scaling factor, so we only need to do this once
    Vector3 dir = subCopies[0].center / subCopies[0].center.magnitude;
    //center gives us both a point and normal for our face plane
    //Parameter (D) determined by plane equation
    float planeParam = -center.x * center.x - center.y * center.y - center.z * center.z;
    //Scaling factor determined by plane equation parameters -> s(Ax + By + Cz) + D = 0
    float s = (-planeParam) / (center.x * dir.x + center.y * dir.y + center.z * dir.z);
    //Scale our vectors to be on the face plane
    foreach (Triangle t in subCopies)
    {
      t.center = (t.center / t.center.magnitude) * s; //Could use t.center.Normalize() * s
    }

    Triangle startingAt = subCopies[0];
    Triangle triCopy = new Triangle(subCopies[0].v1, subCopies[0].v2, subCopies[0].v3);
    Transform triCopyTrans = GameManager.myTrans;
    triCopyTrans.rotation = Quaternion.identity;
    triCopyTrans.position = triCopy.center;

    List<float> subs = new List<float>();


    for (int z=0;z < 6;z++) //Make our 12 triangles (the pentagons apparently work) @TODO: optimize, scew
    {
      //Rotate our tester to where we want it, check for subTriangle here
      triCopyTrans.RotateAround(center, center, 60);

      //Get the list of triCopyTrans distance vectors and sort it to find the smallest(?)
      subs.Clear();
      foreach (Triangle t in subCopies)
      {
        subs.Add(((Vector3)t.center - triCopyTrans.position).sqrMagnitude);
      }
      subs.Sort();
      foreach (Triangle t in subCopies)
      {
        //If this center corresponds to the smallest value in subs
        if (((Vector3)t.center - triCopyTrans.position).sqrMagnitude == subs[0])
        {
          Triangle faceTri = new Triangle(center, startingAt.center, t.center);
          Triangle sideTri = new Triangle(Vector3.zero, faceTri.v3, faceTri.v2);
          faceTris.Add(faceTri);
          sideTris.Add(sideTri);
          startingAt = t;
        }
      }
    }
  }

  void OnCollisionStay()
  {
    colliding = true;
  }

  public Hexagon ToHexagon()
  {
    Vector3[] verts = new Vector3[]{faceTris[0].v2, faceTris[0].v3, faceTris[1].v3, faceTris[2].v3, faceTris[3].v3, faceTris[4].v3};

    Dictionary<Vector3, SphereTile> neighbs = new Dictionary<Vector3, SphereTile>();
    

    //Debug.Log(hexNeighbors.Count);

    return new Hexagon(index, faceTris[0].v1, verts, origin);
  }
}

