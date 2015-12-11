using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(WorldRenderer))]
public class WorldManager : MonoBehaviour
{
  // === Public ===
  public Transform textMeshPrefab;
  public World activeWorld;
  public TileSet regularTileSet;
  public float maxMag = 10;
  public static SimplexNoise simplex;

  // === Private ===
  bool labelDirections;
  //@TODO: These are for creating the heights, and are properties which should be serialized when we go to persistence.
  private int octaves, multiplier;
  private float amplitude, lacunarity, persistence;

  // === Cache ===
  WorldRenderer worldRenderer;
  GameObject currentWorldObject;
  Transform currentWorldTrans;
  //int layermask; @TODO: stuff

  public void Initialize()
  {
    activeWorld = LoadWorld();
    //Seed the world heights
    simplex = new SimplexNoise(GameManager.gameSeed);
    octaves = Random.Range(10, 20);
    multiplier = Random.Range(15, 25);
    amplitude = Random.Range(5f, 10f);
    lacunarity = Random.Range(1.0f, 2.0f);
    persistence = Random.Range(0.1f, 0.9f);
    SetHeights();
    currentWorldObject = new GameObject("World");
    currentWorldTrans = currentWorldObject.transform;

   //currentWorld = new World(WorldSize.Small, WorldType.Verdant, Season.Spring, AxisTilt.Slight);

    worldRenderer = GetComponent<WorldRenderer>();
    foreach (GameObject g in worldRenderer.RenderWorld(activeWorld, regularTileSet))
    {
      g.transform.parent =currentWorldTrans;
    }

    //layermask = 1 << 8;   // Layer 8 is set up as "Chunk" in the Tags & Layers manager

    labelDirections = true;

    //DrawHexIndices();
  }

  World LoadWorld()
  {
    return BinaryHandler.ReadData<World>(World.cachePath);
  }

  void SetHeights()
  {
    float s = 2.0f;
    foreach (HexTile ht in activeWorld.tiles)
    {
      //Debug.Log(Mathf.Abs(simplex.coherentNoise(ht.hexagon.center.x, ht.hexagon.center.y, ht.hexagon.center.z, octaves, multiplier, amplitude, lacunarity, persistence)));
      ht.hexagon.Scale = 10 + 0.5f * Mathf.Abs(simplex.coherentNoise(ht.hexagon.center.x, ht.hexagon.center.y, ht.hexagon.center.z, octaves, multiplier, amplitude, lacunarity, persistence)
                            + 0.5f * Mathf.Abs(simplex.coherentNoise(s*ht.hexagon.center.x, s*ht.hexagon.center.y, s*ht.hexagon.center.z, octaves, multiplier, amplitude, lacunarity, persistence)));
    }
  }

  void OnDrawGizmos()
  {
    if (!labelDirections || activeWorld.tiles.Count == 0)
      return;

    int currentTileX = 13, currentTileY = 0, currentTileXY = 0;

    // === Draw axes on all tiles ===
    for (int i=0; i<activeWorld.tiles.Count; i++)
    {
      DrawHexAxes(activeWorld.tiles, activeWorld.origin, i, .1f);
    }

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
  }

  void DrawHexAxes(List<HexTile> tiles, Vector3 worldOrigin, int index, float scale = .3f, bool suppressWarnings = true)
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

    // Y
    int y = tiles[index].GetNeighborID(Direction.Y);
    if (y != -1)
    {
      Gizmos.color = Color.yellow;
      SerializableVector3 direction = tiles[tiles[index].GetNeighborID(Direction.Y)].hexagon.center - tiles[index].hexagon.center;
      Gizmos.DrawRay((Vector3)origin, (Vector3)direction*scale);
    }
    else if (!suppressWarnings)
      Debug.LogError("Tile "+index+" has no neighbor set in the +Y direction!");

    // XY
    int xy = tiles[index].GetNeighborID(Direction.XY);
    if (xy != -1)
    {
      Gizmos.color = Color.blue;
      SerializableVector3 direction = tiles[tiles[index].GetNeighborID(Direction.XY)].hexagon.center - tiles[index].hexagon.center;
      Gizmos.DrawRay((Vector3)origin, (Vector3)direction*scale);
    }
    else if (!suppressWarnings)
      Debug.LogError("Tile "+index+" has no neighbor set in the +XY direction!");

    // X
    int x = tiles[index].GetNeighborID(Direction.X);
    if (x != -1)
    {
      Gizmos.color = Color.red;
      Vector3 direction = tiles[tiles[index].GetNeighborID(Direction.X)].hexagon.center - tiles[index].hexagon.center + tiles[index].hexagon.normal * scale;
      Gizmos.DrawRay((Vector3)origin, (Vector3)direction*scale);
    }
    else if (!suppressWarnings)
      Debug.LogError("Tile "+index+" has no neighbor set in the +X direction!");
  }

  void DrawHexIndices()
  {
    foreach (HexTile ht in activeWorld.tiles)
    {
      Transform t = (Transform)Instantiate(textMeshPrefab, (ht.hexagon.center-activeWorld.origin)*1.05f, Quaternion.LookRotation(activeWorld.origin-ht.hexagon.center));
      TextMesh x = t.GetComponent<TextMesh>();
      x.text = ht.index.ToString();
      t.parent = currentWorldTrans;
    }
  }
}
