using UnityEngine;

[System.Serializable]
public class Unit
{
  public string name;
  public Actor actor;
  public Stats stats;
  [HideInInspector] public int currentLocation;

  public void Spawn(int loc)
  {
    if (loc == -1 || loc >= CombatManager.activeWorld.tiles.Count)
    {
      Debug.LogError("Invalid spawn location: "+loc);
    }

    currentLocation = loc;
    HexTile tile = CombatManager.activeWorld.tiles[loc];
    actor.Spawn(tile.hexagon.center, tile.hexagon.normal);
  }
}

[System.Serializable]
public class Stats
{
  public int maxHealth, health, moveSpeed;
}