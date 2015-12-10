using UnityEngine;
using System.Collections;

[System.Serializable]
public class HexTile
{
  public int index;
  string terrainType;
  public Hexagon hexagon;

  public HexTile(Hexagon h)
  {
    index = h.index;
    hexagon = h;
  }

  public int GetNeighborID(int dir)
  {
    return hexagon.neighbors[dir];
  }
}