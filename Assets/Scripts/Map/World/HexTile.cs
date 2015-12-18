using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class HexTile
{
  public int index;
  string terrainType;
  public Hexagon hexagon;
  public TileType type;

  public HexTile() { }

  public HexTile(Hexagon h)
  {
    index = h.index;
    hexagon = h;
  }

  /*
  public void SetUVs()
  {
    switch (type)
    {
      case HexTileType.None:
        uvOffset = Vector2.zero;
        break;
      case HexTileType.Sand:
        uvOffset = new Vector2(0 * WorldManager.uvWidth, 0);
        break;
      case HexTileType.PinkSand:
        uvOffset = new Vector2(1 * WorldManager.uvWidth, 0);
        break;
      case HexTileType.Mud:
        uvOffset = new Vector2(2 * WorldManager.uvWidth, 0);
        break;
      case HexTileType.Dirt:
        uvOffset = new Vector2(3 * WorldManager.uvWidth, 0);
        break;
      case HexTileType.Stone:
        uvOffset = new Vector2(4 * WorldManager.uvWidth, 0);
        break;
      case HexTileType.Grass:
        break;
      case HexTileType.SmoothStone:
        break;
      case HexTileType.Road:
        break;
      case HexTileType.MossyRoad:
        break;
      case HexTileType.Snow:
        break;
      case HexTileType.Water:
        break;
      case HexTileType.DeepWater:
        break;
      case HexTileType.Abyss:
        break;
      default:
        break;
    }
  }
  */
  public int GetNeighborID(int dir)
  {
    return hexagon.neighbors[dir];
  }

  public HexTile GetNeighbor(List<HexTile> tiles, int dir)
  {
    return tiles[hexagon.neighbors[dir]];
  }

  public virtual void OnUnitEnter() { }
}

public class HexTile_Grass : HexTile
{ 
  public override void OnUnitEnter()
  {
    Debug.Log("The grass rustles as a unit enters.");
    // Some custom tile logic here
  }
}