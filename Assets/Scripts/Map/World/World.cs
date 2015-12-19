using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum WorldSize {None, Small, Medium, Large};
public enum WorldType {None, Verdant, Icy, Ocean, Barren, Volcanic, Radioactive, Gaseous};
public enum Season {None, Spring, Summer, Fall, Winter};
public enum AxisTilt { None, Slight, Moderate, Severe };      // Affects intensity of difficulty scaling during seasons

[System.Serializable]
public class World
{
  public const string cachePath = "currentWorld.save";

  public string name;

  public WorldSize size;
  public WorldType type;
  public Season season;
  public AxisTilt tilt;

  public SerializableVector3 origin;
  public int circumferenceInTiles;
  public float circumference, radius;


  [HideInInspector] public List<HexTile> tiles;
  private bool neighborInit;
  private List<List<HexTile>> _neighbors;
  public List<List<HexTile>> neighbors{
    get{
      if (!neighborInit)
      {
        if (tiles.Count < 1)
          Debug.LogError("Making neighbor list from null tiles");

        neighborInit = true;
        _neighbors = new List<List<HexTile>>();

        foreach (HexTile t in tiles)
        {
          List<HexTile> neighbs = new List<HexTile>();

          for (int i=0; i<t.hexagon.neighbors.Length; i++)
          {
            try
            {
              neighbs.Add(tiles[t.hexagon.neighbors[i]]);
            }
            catch (System.Exception e)
            {
              //Debug.LogError("tile "+t.index+"'s "+Direction.ToString(i)+" neighbor is bad: "+t.hexagon.neighbors[i]);
            }
          }
          _neighbors.Add(neighbs);
        }
      }

      return _neighbors;
    }
    set{}
  }

  public World()
  {
    origin = Vector3.zero;
  }

  public World(WorldSize s, WorldType t, Season se, AxisTilt at)
  {
    size = s;
    type = t;
    season = se;
    tilt = at;
    origin = Vector3.zero;
  }

  public void PrepForCache(float scale, int subdivisions)
  {
    if (tiles == null || tiles.Count == 0)
    {
      neighborInit = false;
      PolySphere sphere = new PolySphere(Vector3.zero, scale, subdivisions);
      CacheHexes(sphere);
    }
    else
      Debug.Log("tiles not null during cache prep");
  }
  
  public void CacheHexes(PolySphere s)  // Executed by the cacher
  {
    tiles = new List<HexTile>();

    foreach (Hexagon h in s.unitHexes)
    {
      tiles.Add(new HexTile(h));
    }
    neighborInit = false;

    Vector3 side1 = (Vector3)((tiles[0].hexagon.v1 + tiles[0].hexagon.v2) / 2.0f);
    Vector3 side2 = (tiles[0].hexagon.v4 + tiles[0].hexagon.v5) / 2.0f;
    Vector3 dividingSide = side1 - side2;
    radius = (tiles[0].hexagon.v1-origin).magnitude;
    circumference = Mathf.PI * radius * 2.0f;
    circumferenceInTiles = (int)Mathf.Ceil(circumference / dividingSide.magnitude);
  }
}
