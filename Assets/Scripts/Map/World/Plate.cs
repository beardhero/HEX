using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Plate {
  public int index;
  public bool oceanic;
  public bool subducted;
  public List<SphereTile> tiles;
  public SphereTile origin;
  public float driftAngle, spinAngle; //spin axis is the plate origin's center
  public Vector3 driftAxis; //randomized
  public Vector3 drift;
  public Vector3 spin;
  private List<SphereTile> bound;
  public List<SphereTile> boundary;
  public float oceanProb = 0.6f;

  public Plate(List<SphereTile> t, SphereTile ori, int ind)
  {
    index = ind;
    origin = ori;
    float driftx, drifty, driftz; //drift axis components(randomized)
    tiles = new List<SphereTile>(t);
    //Random spin and drift
    //Define random axis and rotation about (drift)
    driftx = Random.Range(-1, 1);
    drifty = Random.Range(-1, 1);
    driftz = Random.Range(-1, 1);
    driftAxis = new Vector3(driftx, drifty, driftz);
    driftAngle = Random.Range(0.1f, 0.24f);
    drift = driftAxis * driftAngle;
    //Define random rotation about center axis (spin)
    spinAngle = Random.Range(0.1f, 0.24f);

    /* used to be for finding origin, now passed
    int i = 0;
    foreach (SphereTile st in tiles)
    {
      if (st.plateOrigin == true)
      {
        origin = st;
        i++;
      }
      if (i > 1)
        Debug.Log("There are " + i + " origins in plate " + index);
    }
    */

    //set spin
    spin = origin.center * spinAngle;

    //oceanic?
    float rand = Random.Range(0, 1f);
    if (rand <= oceanProb)
    {
      oceanic = true;
    }
    //calculate movement
    //set drift for each tile
    foreach (SphereTile st in tiles)
    {
      //calculate movement of each tile given the plate movement
      st.drift = (drift + spin + st.center) / 3;
    }
  }
}
