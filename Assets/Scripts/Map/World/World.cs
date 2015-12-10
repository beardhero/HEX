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


  public List<HexTile> tiles;

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

  public void CacheHexes(PolySphere s)  // Executed by the cacher
  {
    tiles = new List<HexTile>();

    foreach (Hexagon h in s.unitHexes)
    {
      tiles.Add(new HexTile(h));
    }

    Vector3 side1 = (Vector3)((tiles[0].hexagon.v1 + tiles[0].hexagon.v2) / 2.0f);
    radius = (tiles[0].hexagon.v1-origin).magnitude;
    circumference = Mathf.PI * radius * 2.0f;
    circumferenceInTiles = 10;//(int)Mathf.Ceil(circumference / side1.magnitude);
  }

  public void PrepForCache(int scale, int subdivisions)
  {
    if (tiles == null || tiles.Count == 0)
    {
      PolySphere sphere = new PolySphere(Vector3.zero, scale,subdivisions);
      CacheHexes(sphere);
    }
    else
      Debug.Log("tiles not null during cache prep");
  }
}
