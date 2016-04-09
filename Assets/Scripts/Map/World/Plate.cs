using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Plate {
  
  public List<SphereTile> tiles;

  private List<SphereTile> bound;
  public List<SphereTile> boundary
  {
    get { return bound; }
    set { bound = value; }
  }


  public Plate(List<SphereTile> t) 
  {
    float driftx, drifty, driftz; //drift axis components(randomized)
    float drift, spin; //rotation
    tiles = new List<SphereTile>(t);
    //Create boundary
    //boundary = new List<SphereTile>();

    /* BOUNDARY IS NOW A PROPERTY AND IS CREATED IN POLYSPHERE
    foreach (HexTile hp in tiles)
    {
      foreach (HexTile hc in hp.neighbors)
      {
        if(hc.plate != hp.plate) //if any neighbor has a plate index other than its parent, the parent is a boundary tile
        {
          hp.boundary = true;
          boundary.Add(hp);
          break;
        }
      }   
    }
    */
    
    //Random spin and drift @TODO: base randomization off worldseed for persistence
    //Define random axis and rotation about (drift)
    driftx = Random.Range(-1, 1);
    drifty = Random.Range(-1, 1);
    driftz = Random.Range(-1, 1);
    Vector3 driftAxis = new Vector3(driftx, drifty, driftz);
    drift = Random.Range(0.0001f, 0.24f);
    //Define random rotation about center axis (spin)
    spin = Random.Range(0.0001f, 0.24f);
    //Define center axis: index 0, t[0]

  }
}
