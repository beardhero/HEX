using UnityEngine;
using System.Collections;

public class CreateWorldCache : MonoBehaviour {

  public float scale = 5;
  public int subdivisions = 3;

	public void BuildCache  (World world) 
  {
    world.PrepForCache(scale, subdivisions);

    try
    {
      BinaryHandler.WriteData<World>(world, World.cachePath);
      Debug.Log ("World cache concluded.");
    }
    catch(System.Exception e)
    {
      Debug.LogError ("World cache fail: "+e);
    }
	}
	
}
