using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PolySphere
{
  //Initial icosahedron coords
  public static List<Vector3> icoCoords;
  public GameObject go; //Using this transform to rotate around centers of hexes
  public Vector3 origin;
  public int subdivisions;
  public static int maxPlates = 12; //max number of tectonic plates
  public static int minPlates = 3;
  public float scale = 1;

  public List<Triangle> icosahedronTris;
  public List<List<Triangle>> subdividedTris;
  public List<Triangle> finalTris;    // The finest level of subdivided tris
  public List<HexTile> unitHexes;
  public List<SphereTile> sTiles;
  public List<List<SphereTile>> plates;
  public int numberOfPlates;
  public static SimplexNoise simplex;
  public float amplitude, lacunarity, dAmplitude;
  public int octaves, multiplier;

  public PolySphere(){}
  public PolySphere(Vector3 o, float s, int d)
  {
    origin = o;
    scale = s;
    subdivisions = d;
    //For seeding dual centers
    amplitude = Random.Range(0.01f, 0.1f);
    lacunarity = Random.Range(0.2f, 2f);
    dAmplitude = Random.Range(0.1f, 0.9f);
    //Vector3 weights = Random.Vector3;
    octaves = Random.Range(1, 10);
    multiplier = Random.Range(1, 24);
    simplex = new SimplexNoise(GameManager.gameSeed);
    icoCoords = new List<Vector3>();
    icosahedronTris = Icosahedron(scale);
 
    SubdivideAndDuals();
  }

  void SubdivideAndDuals()
  {
    List<Triangle> currentTris;
    List<Triangle> nextTris = new List<Triangle>(icosahedronTris); //Original icosahedron
    List<List<Triangle>> subdividedTris = new List<List<Triangle>>();

    sTiles = new List<SphereTile>();
    
    // Subdivide icosahedron
    for (int i = 0; i < subdivisions; i++)
    {
      currentTris = new List<Triangle>(nextTris);
      nextTris = new List<Triangle>();
      //triforces = new List<Triforce>();

      foreach (Triangle tri in currentTris)
      {
        //Bisect
        Vector3 v1 = (tri.v1+tri.v2)/2.0f;
        Vector3 v2 = (tri.v2+tri.v3)/2.0f;
        Vector3 v3 = (tri.v3+tri.v1)/2.0f;
        
        //Project onto sphere
        v1 *= (float)(1.902084 / v1.magnitude) * scale; //golden rectangle sphere radius 1.902084
        v2 *= (float)(1.902084 / v2.magnitude) * scale;
        v3 *= (float)(1.902084 / v3.magnitude) * scale;

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
      }
      
      //Set Neighbors
      foreach (Triangle tri in currentTris)
      {
        tri.childMid.AssignNeighbors(tri.childTop, tri.childRight, tri.childLeft);
        tri.childTop.AssignNeighbors(tri.NeighborOne(tri.childTop), tri.childMid, tri.NeighborTwo(tri.childTop));
        tri.childRight.AssignNeighbors(tri.NeighborOne(tri.childRight), tri.NeighborTwo(tri.childRight), tri.childMid);
        tri.childLeft.AssignNeighbors(tri.childMid, tri.NeighborOne(tri.childLeft), tri.NeighborTwo(tri.childLeft));
      }    

      //Save our subdivided levels
      subdividedTris.Add(nextTris); 
    }
    
    //Create SphereTiles, give them neighbors
    
    foreach (Triangle tri in nextTris)
    {
      //Tiles to assign
      SphereTile st1 = null, 
                 st2 = null, 
                 st3 = null;
      
      //Create empty SphereTiles, or, if we've already created a SphereTile at this point just reference it
      foreach (SphereTile st in sTiles)
      {
        if ((Vector3)st.center == (Vector3)tri.v1)
        {
          st1 = st;
        }
        if ((Vector3)st.center == (Vector3)tri.v2)
        {
          st2 = st;
        }
        if ((Vector3)st.center == (Vector3)tri.v3)
        {
          st3 = st;
        }
      }
      if (st1 == null)
      {
        st1 = new SphereTile(tri.v1, origin);
        sTiles.Add(st1);
      }
      if (st2 == null)
      {
        st2 = new SphereTile(tri.v2, origin);
        sTiles.Add(st2);
      }
      if (st3 == null)
      {
        st3 = new SphereTile(tri.v3, origin);
        sTiles.Add(st3);
      }


      //Add in the new neighbors from this triangle
        st1.neighborList.Add(st2);
        st1.neighborList.Add(st3);

        st2.neighborList.Add(st1);
        st2.neighborList.Add(st3);

        st3.neighborList.Add(st1);
        st3.neighborList.Add(st2);

      //Add this triangle as an inital triangle in each spheretile
      st1.subTriangles.Add(tri);
      st2.subTriangles.Add(tri);
      st3.subTriangles.Add(tri);
    }
    //dualCenters = dualCenters.Distinct().ToList();
    
    // --- Number sphere tiles ---
    int count = 0;
    //Build the SphereTiles!
    foreach(SphereTile st in sTiles)
    {
      st.index = count;
      count++;

      //st.scale *= scale;
      st.Build();
    }

    unitHexes = new List<HexTile>();
    foreach (SphereTile st in sTiles)
    {
      // Cache neighbors in list
      foreach (SphereTile t in st.neighborList)
      {
        if (!st.neighborDict.ContainsKey(t.index))
          st.neighborDict.Add(t.index, t);
      }
    }

    TectonicPlates(); //Gives SphereTiles a plate index

    // === Cache unit hexagons ===
    foreach (SphereTile st in sTiles)
    {
      unitHexes.Add(st.ToHexTile()); //plate index passed along to hextiles
    }
    // === Assign neighbors to unit hexes ===
    //TraverseAndAssignNeighbors(unitHexes, sTiles);
    //RecursiveNeighbors(unitHexes);
  }

  void TectonicPlates()
  {
    plates = new List<List<SphereTile>>();
    //Start at some random points across the sphere
    int i = 0; //increments through plates
    //each tile will have a chance to be assigned its own plate
    while (plates.Count < minPlates)
    {
      foreach (SphereTile st in sTiles)
      {
        float rand = Random.Range(0, 1f);
        if (rand > 0.99f && plates.Count < maxPlates)
        {
          plates.Add(new List<SphereTile>());
          plates[i].Add(st);
          i++;
        }
      } 
    }
    //Fill in neighbors, fill in neighbors of neighbors, repeat until filled
    FloodFill();
    //Debug.Log(plates.Count);
  }

  void FloodFill() //Recursive
  {
    int i = 0;
    bool go = false;
    List<SphereTile> toAssign = new List<SphereTile>();
    while (i < plates.Count)
    {
      foreach (SphereTile st in plates[i])
      {
        foreach (SphereTile nst in st.neighborList)
        {
          if (nst.plate == -1)
          {
            toAssign.Add(nst);
            nst.plate = i;
          }
        }
      }
      foreach (SphereTile s in toAssign)
      {
        plates[i].Add(s);
      }
      i++;
    }

    foreach (SphereTile s in sTiles)
    {
      if (s.plate == -1)
      {
        go = true;
      }
    }

    if (go) { FloodFill(); }
  }

  void RecursiveNeighbors(List<Hexagon> hexes)
  {
    //Logs the 12 pentagon indices
    foreach (Hexagon h in hexes)
    {
      if (h.isPentagon)
        Debug.Log(h.index);
    }
    bool[] tilesDefined = new bool[hexes.Count];

    // === Set initial seed hex neighbors ===
    hexes[0].neighbors = GetLeftHexagonInitialNeighborsAtSubdivion(subdivisions);
    tilesDefined[0] = true;
    //Now that we have a single hexagon defined, we can define the rest on the sphere based on it
    RecursiveAssign(hexes, tilesDefined);  //This goes into the list of hexes as many times as it takes to assign directions to all hexes
  }

  void RecursiveAssign(List<Hexagon> hexes, bool[] tilesDefined)
  {
    int it = 0;
    List<int> definedIndices = new List<int>();
    //Figure out the indices of tiles which are already defined, increment "it" if we find any that aren't defined so we go back into the function
    for (int i = 0; i < tilesDefined.Length; i++)
    {
      if (tilesDefined[i])
      {
        definedIndices.Add(i);
      }
      else
      {
        it++;
      }
    }
    //For each tile that has been defined, define its neighbors (if they aren't already defined)
    foreach (int i in definedIndices)
    {
      //So we are working with each defined hexagon, hexes[i]
      //Walk to its neighbors one at a time and assign their neighbors
      if (!hexes[i].isPentagon)
      { 
        for (int w = 0; w < hexes[i].neighbors.Length; w++)
        {
          if(!tilesDefined[hexes[i].neighbors[w]])
          {
            DefineNeighborsFromNeighbors(hexes, hexes[i].neighbors[w], hexes[i].index);
            tilesDefined[hexes[i].neighbors[w]] = true;
          }
        }
      }
    }
    if (it > 0)
    {
      RecursiveAssign(hexes, tilesDefined);
    }
  }
  //Using RecursiveNeighbors instead
  void TraverseAndAssignNeighbors(List<Hexagon> hexes, List<SphereTile> sTiles)
  { 
    bool[] tilesDefined = new bool[hexes.Count];

    // === Set initial seed hex neighbors ===
    hexes[0].neighbors = GetLeftHexagonInitialNeighborsAtSubdivion(subdivisions);
    tilesDefined[0] = true;

    int r = hexes[0].neighbors[Direction.NegY];
    hexes[r].neighbors = GetRightHexagonInitialNeighborsAtSubdivion(subdivisions);
    tilesDefined[r] = true;

    // @TODO: Damien come here
    // My idea is to first define the three rings (this example shows y-ring)
    //  then to do the traversal as we discussed.
    //  I beleive the three rings must be defined first 

    // === Define remaining neighbors ===
    int previous = 0, direction = Direction.Y;
    int current = hexes[0].neighbors[direction];
    int timer = 100;    // @TODO: increase lol

    while (timer > 0)
    {
      timer --;

      if (!tilesDefined[current])
      {
        DefineNeighborsFromNeighbors(hexes, current, previous);
        tilesDefined[current] = true;
      }
      
      previous = current;
      current = hexes[current].neighbors[direction];
     }
  }

  int HasACompleteNeighbor(List<Hexagon> hexes, int index)
  {
    int found = -1;

    for (int i=0; i<hexes[index].neighbors.Length; i++)
    {
      if (NeighborsDefined(hexes, hexes[index].neighbors[i]))
      {
        found = hexes[index].neighbors[i];
      }
    }

    return found;
  }

  bool NeighborsDefined(List<Hexagon> hexes, int i)
  {
    if (i >= hexes.Count)
    {
      Debug.LogError("index:"+i+" hexes.Count:"+hexes.Count);
      return false;
    }
    if (i==-1)
      return false;
    else if (hexes[i].neighbors.Length > 5)
    {
      if (i==1)
      {

      }
      Debug.Log("Y:" + (hexes[i].neighbors[Direction.Y] != -1) +
            " XY:"+(hexes[i].neighbors[Direction.XY] != -1)+
            " X:"+(hexes[i].neighbors[Direction.X] != -1)+
            " -Y:"+(hexes[i].neighbors[Direction.NegY] != -1)+
            " -XY:"+(hexes[i].neighbors[Direction.NegXY] != -1 )+
            " -X:"+(hexes[i].neighbors[Direction.NegX] != -1));

      return hexes[i].neighbors[Direction.Y] != -1 &&
            hexes[i].neighbors[Direction.XY] != -1 &&
            hexes[i].neighbors[Direction.X] != -1 &&
            hexes[i].neighbors[Direction.NegY] != -1 &&
            hexes[i].neighbors[Direction.NegXY] != -1 &&
            hexes[i].neighbors[Direction.NegX] != -1;
    }
    else
    {
      return hexes[i].neighbors[Direction.Y] != -1 &&
            hexes[i].neighbors[Direction.XY] != -1 &&
            hexes[i].neighbors[Direction.X] != -1 &&
            hexes[i].neighbors[Direction.NegY] != -1 &&
            hexes[i].neighbors[Direction.NegX] != -1;
    }
  }

  void DefineNeighborsFromNeighbors(List<Hexagon> hexes, int i, int knownDefined = -1)
  {
    
    // Here we assume there are two adjacent tiles, i and knownDefined
    if (i==-1)
    {
      Debug.LogError("Attempting to define a null index");
      return;
    }

    if (knownDefined == -1)
    {
      knownDefined = i-1;
    }
    
    List<SphereTile> potentialNeighbors;

    try
    {
      potentialNeighbors = sTiles[i].neighborDict.Values.ToList();
    }
    catch (System.Exception e)
    {
      Debug.LogError(" index:"+i+" sTiles.Count:"+sTiles.Count);
      return;
    }

    for (int dir = 0; dir<hexes[i].neighbors.Length; dir++)
    {
      //Debug.Log(i+": Defining "+Direction.ToString(dir)+"-neighbor with reference "+knownDefined+"->"+hexes[knownDefined].neighbors[dir]);

      if (hexes[i].neighbors[dir] != -1)
      {
        continue;
      }

      Vector3 direction = hexes[knownDefined].center -
        hexes[ hexes[knownDefined].neighbors[dir] ].center;

      int n = FindNeighbor(hexes[i].center, direction, potentialNeighbors);
      hexes[i].neighbors[dir] = n;
    } 
  }

  /*
  Vector3 GetNearbyDefinedNeighborVector(List<Hexagon> hexes, int index, int direction)
  {
    if (index == -1)
    {
      Debug.LogError("null index");
      return Vector3.zero;
    }

    int winner2 = -1, winner1 = -1;

    for (int i=0;i<6;i++)
    {
      int neighbor = hexes[index].neighbors[i], neighNeighb = -1;

      if (neighbor != -1)
      {
       //Debug.Log("tile defined: "+index+","+i+"   neighbor: "+neighbor);
        neighNeighb = hexes[neighbor].neighbors[direction];
        if (neighNeighb != -1)
        {
          winner1 = neighbor;
          winner2 = neighNeighb;
          break;
        }
      }
    }

    if (winner2 == -1 || winner1 == -1)
    {
      return Vector3.zero;
    }
    else
      return hexes[winner1].center - hexes[winner2].center;
  }
  */

  int FindNeighbor(Vector3 center, Vector3 direction, List<SphereTile> potentialNeighbors)
  {
    float winningAngle = 190, angle=9999;
    int winningNeighborIndex = -1;

    for (int i=0; i<potentialNeighbors.Count; i++)
    {
      angle = Vector3.Angle(center-potentialNeighbors[i].center, direction);

      if (angle < winningAngle)
      {
        winningAngle = angle;
        winningNeighborIndex = i;
      }
    }

    return potentialNeighbors[winningNeighborIndex].index;
  }

  List<Triangle> Icosahedron(float scale)
  {
    if (scale <= 0)
      Debug.LogError("NO SCALE?!");
    if (scale > 50)
      Debug.LogError("Why so big?");
      
    List<Triangle> output = new List<Triangle>();
    List<Vector3> vertices = new List<Vector3>();

    float goldRat = 1.618f; //golden ratio (Unity calculated it as 1.6)s

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
    icoCoords.Add(xy1);
    icoCoords.Add(xy2);
    icoCoords.Add(xy3);
    icoCoords.Add(xy4);
    icoCoords.Add(xz1);
    icoCoords.Add(xz2);
    icoCoords.Add(xz3);
    icoCoords.Add(xz4);
    icoCoords.Add(zy1);
    icoCoords.Add(zy2);
    icoCoords.Add(zy3);
    icoCoords.Add(zy4);


    //Debug.Log(xz4.magnitude);
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

    // === Faces of the Original 5 Triforces ===
    output.Add(new Triangle(vertices[1], vertices[6], vertices[10]));   // 0
    output.Add(new Triangle(vertices[1], vertices[10], vertices[4]));   // 1
    output.Add(new Triangle(vertices[1], vertices[4], vertices[9]));    // 2
    output.Add(new Triangle(vertices[1], vertices[9], vertices[5]));    // 3
    output.Add(new Triangle(vertices[1], vertices[5], vertices[6]));    // 4
    
    output.Add(new Triangle(vertices[3], vertices[7], vertices[11]));   // 5
    output.Add(new Triangle(vertices[3], vertices[11], vertices[2]));   // 6
    output.Add(new Triangle(vertices[3], vertices[2], vertices[12]));   // 7
    output.Add(new Triangle(vertices[3], vertices[12], vertices[8]));   // 8
    output.Add(new Triangle(vertices[3], vertices[8], vertices[7]));    // 9

    output.Add(new Triangle(vertices[10], vertices[7], vertices[4]));   // 10
    output.Add(new Triangle(vertices[4], vertices[7], vertices[8]));    // 11
    output.Add(new Triangle(vertices[4], vertices[8], vertices[9]));    // 12
    output.Add(new Triangle(vertices[9], vertices[8], vertices[12]));   // 13
    output.Add(new Triangle(vertices[9], vertices[12], vertices[5]));   // 14
    output.Add(new Triangle(vertices[5], vertices[12], vertices[2]));   // 15
    output.Add(new Triangle(vertices[5], vertices[2], vertices[6]));    // 16
    output.Add(new Triangle(vertices[6], vertices[2], vertices[11]));   // 17
    output.Add(new Triangle(vertices[11], vertices[10], vertices[6]));  // 18
    output.Add(new Triangle(vertices[10], vertices[11], vertices[7]));  // 19

    // Assign initial neighbors
    output[0].AssignNeighbors(output[1],  output[4], output[18]);
    output[1].AssignNeighbors(output[2],  output[0], output[10]);
    output[2].AssignNeighbors(output[1],  output[12],output[3]);
    output[3].AssignNeighbors(output[2],  output[14],output[4]);
    output[4].AssignNeighbors(output[3],  output[16],output[0]);
    output[5].AssignNeighbors(output[19], output[6], output[9]);
    output[6].AssignNeighbors(output[5],  output[17],output[7]);
    output[7].AssignNeighbors(output[6],  output[15],output[8]);
    output[8].AssignNeighbors(output[7],  output[13],output[9]);
    output[9].AssignNeighbors(output[8],  output[11],output[5]);
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

  public static int[] GetLeftHexagonInitialNeighborsAtSubdivion(int subdivisionLevel)
  {
    int[] output = new int[6];

    switch (subdivisionLevel)
    {
                      // Y, XY, X, -Y, -XY -X
      case 1:
        output[Direction.Y]     = 1;
        output[Direction.XY]    = 2;
        output[Direction.X]     = 3;
        output[Direction.NegY]  = 13;
        output[Direction.NegXY] = 15;
        output[Direction.NegX]  = 4;
        return output;
      case 2:
        return new int[]{1,2,3,9,10,4};
      default:
        return new int[6];
    }
  }

  public static int[] GetRightHexagonInitialNeighborsAtSubdivion(int subdivisionLevel)
  {
    switch (subdivisionLevel)
    {
                      // Y, XY, X, -Y, -XY -X
      case 1:
        return new int[]{0,3,10,12,14,15};
      case 2:
        return new int[]{0,3,46,50,11,10};
      default:
        return new int[6];
    }
  }
}
