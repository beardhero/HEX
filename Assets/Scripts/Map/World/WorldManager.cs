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

  // === Private ===
  bool labelDirections;

  // === Cache ===
  WorldRenderer worldRenderer;
  GameObject currentWorldObject;
  Transform currentWorldTrans;
  //int layermask; @TODO: stuff

  public void Initialize()
  {
    activeWorld = LoadWorld();
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

    DrawHexIndices();
  }

  World LoadWorld()
  {
    return BinaryHandler.ReadData<World>(World.cachePath);
  }

  void OnDrawGizmos()
  {
    if (!labelDirections || activeWorld.tiles.Count == 0)
      return;

    int currentTileX = 0, currentTileY = 0, currentTileXY = 0;

    // === Draw axes on all tiles ===
    for (int i=0; i<activeWorld.tiles.Count; i++)
    {
      DrawHexAxes(activeWorld.tiles, activeWorld.origin, i);
    }

    // === Draw Bands Only ===
    /*
    for (int x=0; x<activeWorld.circumferenceInTiles; x++)
    {
      for (int y=0; y<activeWorld.circumferenceInTiles; y++)
      {
        for (int xy=0; xy<activeWorld.circumferenceInTiles; xy++)
        {
          DrawHexAxes(activeWorld.tiles, activeWorld.origin, currentTileXY);
          currentTileXY = activeWorld.tiles[currentTileXY].GetNeighborID(Direction.XY);
        }
        DrawHexAxes(activeWorld.tiles, activeWorld.origin, currentTileY);
        currentTileXY = activeWorld.tiles[currentTileY].GetNeighborID(Direction.Y);
      }
      DrawHexAxes(activeWorld.tiles, activeWorld.origin, currentTileX);
      currentTileXY = activeWorld.tiles[currentTileX].GetNeighborID(Direction.X);
    }
    */
  }

  void DrawHexAxes(List<HexTile> tiles, Vector3 worldOrigin, int index, bool suppressWarnings = true)
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
      Gizmos.DrawRay((Vector3)origin, (Vector3)direction*.35f);
    }
    /*
    else if (!suppressWarnings)
      Debug.LogError("Tile "+index+" has no neighbor set in the +Y direction!");
    */

    // XY
    int xy = tiles[index].GetNeighborID(Direction.XY);
    if (xy != -1)
    {
      Gizmos.color = Color.blue;
      SerializableVector3 direction = tiles[tiles[index].GetNeighborID(Direction.XY)].hexagon.center - tiles[index].hexagon.center;
      Gizmos.DrawRay((Vector3)origin, (Vector3)direction*.35f);
    }
    /*
    else if (!suppressWarnings)
      Debug.LogError("Tile "+index+" has no neighbor set in the +XY direction!");
    */

    // X
    int x = tiles[index].GetNeighborID(Direction.X);
    if (x != -1)
    {
      Gizmos.color = Color.red;
      SerializableVector3 direction = tiles[tiles[index].GetNeighborID(Direction.X)].hexagon.center - tiles[index].hexagon.center;
      Gizmos.DrawRay((Vector3)origin, (Vector3)direction*.35f);
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
