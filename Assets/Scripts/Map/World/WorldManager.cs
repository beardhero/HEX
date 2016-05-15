using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(WorldRenderer))]
public class WorldManager : MonoBehaviour
{
  // === Public ===
  public Transform textMeshPrefab;
  [HideInInspector] public World activeWorld;
  public TileSet regularTileSet;
  public float maxMag = 10;
  public float worldScale = 1;
  public int worldSubdivisions = 1;
  public static SimplexNoise simplex;
  public static int uvWidth = 100;
  public static int uvHeight;

  // === Private ===
  bool labelDirections;
  //@TODO: These are for creating the heights, and are properties which should be serialized when we go to persistent galaxy.
  private int octaves, multiplier;
  private float amplitude, lacunarity, dAmplitude;

  // === Properties ===
  private float _averageScale;
  public float averageScale
  {
    get {
      _averageScale = 0;
      foreach (HexTile ht in activeWorld.tiles)
      {
        _averageScale += ht.hexagon.scale;
      }
      _averageScale /= activeWorld.tiles.Count;
      return _averageScale; }
    set { _averageScale = value; }
  }


  // === Cache ===
  WorldRenderer worldRenderer;
  GameObject currentWorldObject;
  Transform currentWorldTrans;
  //int layermask; @TODO: stuff

  void OnDrawGizmos()
  {
    //Debug.Log("going here");
    DrawAxes();
  }

  public World Initialize(bool loadWorld = false)
  {
    simplex = new SimplexNoise(GameManager.gameSeed);
    octaves = Random.Range(4, 4);
    multiplier = Random.Range(10, 10);
    amplitude = Random.Range(0.6f, 1f);
    lacunarity = Random.Range(0.7f, .9f);
    dAmplitude = Random.Range(0.5f, .1f);


    if (loadWorld)
    {
      activeWorld = LoadWorld();
    }
    else
    {
      activeWorld = new World();
      activeWorld.PrepForCache(worldScale, worldSubdivisions);
    }
    
    //Seed the world heights
    SetHeights();
    
    CreateOcean();
    
    currentWorldObject = new GameObject("World");
    currentWorldTrans = currentWorldObject.transform;

    //currentWorld = new World(WorldSize.Small, WorldType.Verdant, Season.Spring, AxisTilt.Slight);

    worldRenderer = GetComponent<WorldRenderer>();
    //changed this to run RenderPlates instead of RenderWorld
    foreach (GameObject g in worldRenderer.RenderPlates(activeWorld, regularTileSet))
    {
      g.transform.parent =currentWorldTrans;
    }

    //layermask = 1 << 8;   // Layer 8 is set up as "Chunk" in the Tags & Layers manager

    labelDirections = true;

    //DrawHexIndices();

    return activeWorld;
  }

  World LoadWorld()
  {
    return BinaryHandler.ReadData<World>(World.cachePath);
  }

  void SetHeights() //@TODO we should be reading heights from hextile (based on the worldseed?)
  {
    //Alright, let's get rid of this simplex nonsense and get some heights from the plate tectonics
    //Each plate has has two axes it's moving on with a small velocity, each tile shares this movement
    //Well we're going to have to do this before caching and save heights into the hextiles for access to plates
    //Then read those heights
    foreach (HexTile ht in activeWorld.tiles)
    {
      ht.hexagon.Scale(ht.height);
    }

    /*
    float s = Random.Range(-99999,99999);
    foreach (HexTile ht in activeWorld.tiles)
    {
      ht.hexagon.Scale(1f + (int)(100 * (0.7f * Mathf.Abs(simplex.coherentNoise(ht.hexagon.center.x, ht.hexagon.center.y, ht.hexagon.center.z, octaves, multiplier, amplitude, lacunarity, dAmplitude) //))) / 100f);
                           + 0.3f * Mathf.Abs(simplex.coherentNoise(s*ht.hexagon.center.x, s*ht.hexagon.center.y, s*ht.hexagon.center.z, octaves, multiplier, amplitude, lacunarity, dAmplitude)))))/100f);
      //Debug.Log(1f + (int)(100 * (0.7f * Mathf.Abs(simplex.coherentNoise(ht.hexagon.center.x, ht.hexagon.center.y, ht.hexagon.center.z, octaves, multiplier, amplitude, lacunarity, dAmplitude)
      //                      + 0.3f * Mathf.Abs(simplex.coherentNoise(s * ht.hexagon.center.x, s * ht.hexagon.center.y, s * ht.hexagon.center.z, octaves, multiplier, amplitude, lacunarity, dAmplitude)))))/100f);
      //Debug.Log(ht.hexagon.scale);
    }
    */
  }
  //@TODO: This is preliminary, it sets the ocean tiles using average scale 
  //by making any tile close to the average or below blue, then scaling the blue tiles up to the average.
  void CreateOcean()
  {
    foreach (HexTile ht in activeWorld.tiles)
    {
      ht.type = TileType.Blue;
    }
    TileType typeToSet = TileType.Blue;
    foreach (HexTile ht in activeWorld.tiles)
    {
      float rand = Random.Range(0, 1f);
      //@TODO: this is just a preliminary variation of the land types
      if (rand <= 0.4f)
        typeToSet = TileType.Gray;
      if (rand > 0.4f)
        typeToSet = TileType.Green;
      if (ht.hexagon.scale >= averageScale*0.99f)
      {
        ht.type = typeToSet;
      }
    }
    foreach (HexTile ht in activeWorld.tiles)
    {
      if (ht.type == TileType.Blue)
      {
        ht.hexagon.Scale(averageScale*0.99f / ht.hexagon.scale);
      }
    }
  }
  //So now with the land masses, we're going to make the "biomes" more coherent like we did in Zone -> SpreadGround and RefineGround
  void RefineTypes()
  {
    foreach (HexTile ht in activeWorld.tiles)
    {
      //int i = 0;
      //foreach (HexTile h in ht.ne) ;
    }
  }
  void DrawAxes()
  {
    if (!labelDirections || activeWorld.tiles.Count == 0)
      return;

    //int currentTileX = 13, currentTileY = 0, currentTileXY = 0;  

    // === Draw axes on all tiles ===
    for (int i=0; i<activeWorld.tiles.Count; i++)
    {
      DrawHexAxes(activeWorld.tiles, activeWorld.origin, i);
    }

    /*
    // === Draw Bands Only ===
    // Y-band
    for (int y=0; y<activeWorld.circumferenceInTiles; y++)
    {
      if (currentTileY != -1)
      {
        DrawHexAxes(activeWorld.tiles, activeWorld.origin, currentTileY);
        currentTileY = activeWorld.tiles[currentTileY].GetNeighborID(Direction.Y);
      }
    }
    // XY-band
    for (int xy=0; xy<activeWorld.circumferenceInTiles; xy++)
    {
      if (currentTileXY != -1)
      {
        DrawHexAxes(activeWorld.tiles, activeWorld.origin, currentTileXY);
        currentTileXY = activeWorld.tiles[currentTileXY].GetNeighborID(Direction.XY);
      }
    }
    // X-band
    for (int x=0; x<activeWorld.circumferenceInTiles; x++)
    {
      if (currentTileX != -1)
      {
        DrawHexAxes(activeWorld.tiles, activeWorld.origin, currentTileX);
        currentTileX = activeWorld.tiles[currentTileX].GetNeighborID(Direction.X);
      }
    }
    */
  }

  void DrawHexAxes(List<HexTile> tiles, Vector3 worldOrigin, int index, float scale = .1f, bool suppressWarnings = true)
  {
    if (index == -1)
    {
      Debug.LogError("Invalid index: -1");
      return;
    }

    SerializableVector3 origin = new SerializableVector3();
    try
    {
      origin = (tiles[index].hexagon.center + (SerializableVector3)worldOrigin) * 1.05f;
    }
    catch(System.Exception e)
    {
      Debug.LogError("Error accessing tile "+ index+": "+e);
      return;
    }

    for (int dir = 0; dir<Direction.Count && dir<tiles[index].hexagon.neighbors.Length; dir++)
    {
      int y = tiles[index].GetNeighborID(dir);
      if (y != -1)
      {
        Gizmos.color = Direction.ToColor(dir);
        SerializableVector3 direction = tiles[tiles[index].GetNeighborID(dir)].hexagon.center - tiles[index].hexagon.center;

        float finalScale = scale;
        if (dir == Direction.X || dir == Direction.Y || dir == Direction.NegXY)   // Prime directions
          finalScale *= 2;

        Gizmos.DrawRay((Vector3)origin, (Vector3)direction*finalScale);
      }
    }
  }

  void DrawHexIndices()
  {
    foreach (HexTile ht in activeWorld.tiles)
    {
      Transform t = (Transform)Instantiate(textMeshPrefab, (ht.hexagon.center-activeWorld.origin)*1.01f, Quaternion.LookRotation(activeWorld.origin-ht.hexagon.center));
      TextMesh x = t.GetComponent<TextMesh>();
      x.text = ht.index.ToString();
      t.parent = currentWorldTrans;
    }
  }
}
