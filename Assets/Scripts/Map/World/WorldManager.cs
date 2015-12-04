﻿using UnityEngine;
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

    int currentTile = 0;

    // Traverse +y
    for (int i=0;i<10;i++)
    {
      DrawHexAxes(activeWorld.tiles, activeWorld.origin, currentTile);
      int n = activeWorld.tiles[currentTile].GetNeighborID(Direction.Y);
      currentTile = activeWorld.tiles[n].index;
    }

    // Traverse +X+Y
    currentTile = 0;

    for (int i=0;i<10;i++)
    {
      DrawHexAxes(activeWorld.tiles, activeWorld.origin, currentTile);
      int n  = activeWorld.tiles[currentTile].GetNeighborID(Direction.XY);
      currentTile = activeWorld.tiles[n].index;
    }

    // Traverse +X
    currentTile = 13;

    for (int i=0;i<10;i++)
    {
      DrawHexAxes(activeWorld.tiles, activeWorld.origin, currentTile);
      int n  = activeWorld.tiles[currentTile].GetNeighborID(Direction.X);
      currentTile = activeWorld.tiles[n].index;
    }
  }

  void DrawHexAxes(List<HexTile> tiles, Vector3 worldOrigin, int index)
  {
    SerializableVector3 origin = (tiles[index].hexagon.center + (SerializableVector3)worldOrigin) * 1.05f;

    // Y
    int y = tiles[index].GetNeighborID(Direction.Y);
    if (y != -1)
    {
      Gizmos.color = Color.yellow;
      SerializableVector3 direction = tiles[tiles[index].GetNeighborID(Direction.Y)].hexagon.center - tiles[index].hexagon.center;
      Gizmos.DrawRay((Vector3)origin, (Vector3)direction*.35f);
    }

    // XY
    int xy = tiles[index].GetNeighborID(Direction.XY);
    if (xy != -1)
    {
      Gizmos.color = Color.blue;
      SerializableVector3 direction = tiles[tiles[index].GetNeighborID(Direction.XY)].hexagon.center - tiles[index].hexagon.center;
      Gizmos.DrawRay((Vector3)origin, (Vector3)direction*.35f);
    }

    // X
    int x = tiles[index].GetNeighborID(Direction.X);
    if (x != -1)
    {
      Gizmos.color = Color.red;
      SerializableVector3 direction = tiles[tiles[index].GetNeighborID(Direction.X)].hexagon.center - tiles[index].hexagon.center;
      Gizmos.DrawRay((Vector3)origin, (Vector3)direction*.35f);
    }
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
