using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class HexTile
{
  public int index;
  public int plate = -1;
  public float height;
  string terrainType;
  public Hexagon hexagon;
  public TileType type;
  public List<int> neighbors;
  public bool boundary;

  //Tectonics
  public float pressure /*colliding positive, seperating negative*/, shear, scale, temp, humidity;

  private float _elevation;
  public float elevation
  {
    get { return _elevation; }
    set { _elevation = value; }
  }
  private float _heat;

  public float heat
  {
    get { return _heat; }
    set { _heat = value; }
  }
  private float _precipitation;

  public float precipitation
  {
    get { return _precipitation; }
    set { _precipitation = value; }
  }
  public HexTile() { }

  public HexTile(Hexagon h)
  {
    index = h.index;
    hexagon = h;
  }
  public HexTile(Hexagon h, int p, List<int> neighbs, bool b, float hi)
  {
    index = h.index;
    hexagon = h;
    plate = p;
    boundary = b;
    neighbors = new List<int>(neighbs);
    height = hi;
  }
  public void ChangeType(TileType t)
  {
    type = t;
    //@TODO: other stuff
  }

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