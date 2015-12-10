using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PolySphere
{
  public Vector3 origin;
  public int subdivisions = 3;
  public int scale = 1;

  public List<Triangle> icosahedronTris;
  public List<List<Triangle>> subdividedTris;
  public List<Triangle> finalTris;    // The finest level of subdivided tris
  public List<Hexagon> finalHexes, unitHexes;
  public List<SphereTile> sTiles;
 
  //For simplex
  public static SimplexNoise simplex;
  public float amplitude, lacunarity, persistence;
  public int octaves, multiplier;

  public PolySphere()
  {
      
  }
  public PolySphere(Vector3 o, int s, int d)
  {
    origin = o;
    scale = s;
    subdivisions = d;
    //For seeding dual centers
    amplitude = Random.Range(0.01f, 0.1f);
    lacunarity = Random.Range(0.2f, 2f);
    persistence = Random.Range(0.1f, 0.9f);
    //Vector3 weights = Random.Vector3;
    octaves = Random.Range(1, 10);
    multiplier = Random.Range(1, 24);
    simplex = new SimplexNoise(GameManager.gameSeed);
     
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
        st1 = new SphereTile(tri.v1);
        sTiles.Add(st1);
      }
      if (st2 == null)
      {
        st2 = new SphereTile(tri.v2);
        sTiles.Add(st2);
      }
      if (st3 == null)
      {
        st3 = new SphereTile(tri.v3);
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

      st.scale *= scale;
      st.Build();
    }
    
    unitHexes = new List<Hexagon>();
    foreach (SphereTile st in sTiles)
    {
      // Cache neighbors in list
      foreach (SphereTile t in st.neighborList)
      {
        if (!st.neighborDict.ContainsKey(t.index))
          st.neighborDict.Add(t.index, t);
      }
    }

    // === Cache unit hexagons ===
    foreach (SphereTile st in sTiles)
    {
      unitHexes.Add(st.ToHexagon());
    }

    // === Assign neighbors to unix hexes ===
    TraverseAndAssignNeighbors(unitHexes, sTiles);

    // === Scale with height function ===
    // @TODO: seed
    ScaleSimplex(PolySphere.simplex, octaves, multiplier, amplitude, lacunarity, persistence);

    //Set water depth using average of all scales 
    float sAverage = 0;
    foreach (SphereTile st in sTiles)
    {
      sAverage += st.scale;
    }
    sAverage /= sTiles.Count;

    // --- Number tris ---
    // Is this section still needed?
    count = 0;
    foreach (Triangle t in nextTris)
    {
      t.index = count;
      count++;
    }

    // === Cache FINAL hex tiles ===
    finalHexes = new List<Hexagon>();
    foreach (SphereTile st in sTiles)
    {
      finalHexes.Add(st.ToHexagon());
    }

    //finalTris = dualTris;
  }

  void TraverseAndAssignNeighbors(List<Hexagon> hexes, List<SphereTile> sTiles)
  {
    // === Create three initial bands ===
    // Starting hex 0
    hexes[0].neighbors[(int)Direction.Y] = 1;
    hexes[0].neighbors[(int)Direction.XY] = 2;
    hexes[0].neighbors[(int)Direction.X] = 3;
    hexes[0].neighbors[(int)Direction.NegY] = 13;
    hexes[0].neighbors[(int)Direction.NegXY] = 15;
    hexes[0].neighbors[(int)Direction.NegX] = 4;


    int currentHex, lastHex, startingHex, currentWinner;

    // --- Traverse +Y ---
    int breaker = 1000;
    currentHex = 1;
    startingHex = 0;
    lastHex = startingHex;

    do
    {
      SerializableVector3 currentCenter = hexes[currentHex].center;
      SerializableVector3 currentYDirection = hexes[lastHex].center - currentCenter;
      
      List<SphereTile> potentialNeighbors = sTiles[currentHex].neighborDict.Values.ToList();

      int nextIndex = FindNeighbor(currentCenter, currentYDirection, potentialNeighbors);

      hexes[currentHex].neighbors[(int)Direction.Y] = nextIndex;
      hexes[currentHex].neighbors[(int)Direction.NegY] = lastHex;

      // Move to next y
      lastHex = currentHex;
      currentHex = nextIndex;
      breaker--;
    }
    while (currentHex != startingHex && breaker > 0);
    if (breaker < 1)
    {
      Debug.LogError("No end was found to directional band during +Y neighbor traversal (Hit breaker limit).");
    }


    // --- Traverse +X+Y ---
    breaker = 1000;
    startingHex = currentHex;
    currentHex = 2;  // @TODO
    startingHex = 0;
    lastHex = startingHex;

    do
    {
      SerializableVector3 currentCenter = hexes[currentHex].center;
      SerializableVector3 currentXYDirection = hexes[lastHex].center - currentCenter;

      List<SphereTile> potentialNeighbors = sTiles[currentHex].neighborDict.Values.ToList();
      int nextIndex = FindNeighbor(currentCenter, currentXYDirection, potentialNeighbors);

      hexes[currentHex].neighbors[(int)Direction.XY] = nextIndex;
      hexes[currentHex].neighbors[(int)Direction.NegXY] = lastHex;

      // Move to next xy
      lastHex = currentHex;
      currentHex = nextIndex;
      breaker--;
    }
    while (currentHex != startingHex && breaker > 0);
    if (breaker < 1)
    {
      Debug.LogError("No end was found to directional band during neighbor +X+Y traversal (Hit breaker limit).");
    }

    
    // --- Traverse +X ---
    breaker = 1000;
    startingHex = hexes[0].neighbors[(int)Direction.NegY];
    currentHex = 10;
    lastHex = startingHex;

    hexes[startingHex].neighbors[(int)Direction.X] = currentHex;  // Only has to be done for this one tile

    do
    {
      SerializableVector3 currentCenter = hexes[currentHex].center;
      SerializableVector3 currentXDirection = hexes[lastHex].center - currentCenter;
      
      List<SphereTile> potentialNeighbors = sTiles[currentHex].neighborDict.Values.ToList();
      int nextIndex = FindNeighbor(currentCenter, currentXDirection, potentialNeighbors);

      hexes[currentHex].neighbors[(int)Direction.X] = nextIndex;
      hexes[currentHex].neighbors[(int)Direction.NegX] = lastHex;

      // Move to next x
      lastHex = currentHex;
      currentHex = nextIndex;
      currentXDirection = hexes[lastHex].center - currentCenter;
      breaker--;
    }
    while (currentHex != startingHex && breaker > 0);
    if (breaker < 1)
    {
      Debug.LogError("No end was found to directional band during neighbor +X traversal (Hit breaker limit).");
    }
    
    // === Define remaining neighbors ===
    for (int i=1; i<hexes.Count;i++)
    {
      List<SphereTile> potentialNeighbors = sTiles[i].neighborDict.Values.ToList();

      // Check Y
      if (hexes[i].neighbors[Direction.Y] == -1)
      {
        int previous = hexes[i-1].neighbors[Direction.Y];
        Vector3 direction = hexes[i-1].center - hexes[previous].center;
        hexes[i].neighbors[Direction.Y] = FindNeighbor(hexes[i].center, direction, potentialNeighbors);
      }

      // Check -Y
      if (hexes[i].neighbors[Direction.NegY] == -1)
      {
        int previous = hexes[i-1].neighbors[Direction.NegY];
        Vector3 direction = hexes[i-1].center - hexes[previous].center;
        hexes[i].neighbors[Direction.NegY] = FindNeighbor(hexes[i].center, direction, potentialNeighbors);
      }

      // Check XY
      if (hexes[i].neighbors[Direction.XY] == -1)
      {
        int previous = hexes[i-1].neighbors[Direction.XY];
        Vector3 direction = hexes[i-1].center - hexes[previous].center;
        hexes[i].neighbors[Direction.XY] = FindNeighbor(hexes[i].center, direction, potentialNeighbors);
      }

      //Check -XY
      if (hexes[i].neighbors[Direction.NegXY] == -1)
      {
        int previous = hexes[i-1].neighbors[Direction.NegXY];
        Vector3 direction = hexes[i-1].center - hexes[previous].center;
        hexes[i].neighbors[Direction.NegXY] = FindNeighbor(hexes[i].center, direction, potentialNeighbors);
      }

      // Check X
      if (hexes[i].neighbors[Direction.X] == -1)
      {
        int previous = hexes[i-1].neighbors[Direction.X];
        Vector3 direction = hexes[i-1].center - hexes[previous].center;
        hexes[i].neighbors[Direction.X] = FindNeighbor(hexes[i].center, direction, potentialNeighbors);
      }

      // Check -X
      if (hexes[i].neighbors[Direction.NegX] == -1)
      {
        int previous = hexes[i-1].neighbors[Direction.NegX];
        Vector3 direction = hexes[i-1].center - hexes[previous].center;
        hexes[i].neighbors[Direction.NegX] = FindNeighbor(hexes[i].center, direction, potentialNeighbors);
      }

    }
  }

  int FindNeighbor(SerializableVector3 center, Vector3 direction, List<SphereTile> potentialNeighbors)
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

  public void ScaleSimplex(SimplexNoise simplex, int octaves, int multiplier, float amplitude, float lacunarity, float persistence)
  {
    foreach (SphereTile st in sTiles)
    {
      float height = Mathf.Abs(simplex.coherentNoise(st.center.x, st.center.y, st.center.z, octaves, multiplier, amplitude, lacunarity, persistence));
      st.scale *= 24*(1 + height*100);
    }
  }
  
  List<Triangle> Icosahedron(int scale)
  {
    List<Triangle> output = new List<Triangle>();
    List<Vector3> vertices = new List<Vector3>();

    float goldRat = 1.6f; //golden ratio (Unity calculated it as 1.6)

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
    
    output.Add(new Triangle(vertices[3], vertices[7], vertices[11]));   // 7
    output.Add(new Triangle(vertices[3], vertices[11], vertices[2]));   // 6
    output.Add(new Triangle(vertices[3], vertices[2], vertices[12]));   // 5
    output.Add(new Triangle(vertices[3], vertices[12], vertices[8]));   // 9
    output.Add(new Triangle(vertices[3], vertices[8], vertices[7]));    // 8

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
