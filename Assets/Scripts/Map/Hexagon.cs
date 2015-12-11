using UnityEngine;
using System.Collections;

[System.Serializable]
public class Hexagon
{
  public int index;
  public SerializableVector3 center, normal, v1, v2, v3, v4, v5, v6;
  public int[] neighbors;
  bool isPentagon;
  private float _scale;

  public float Scale //This will multiply all vectors in the hexagon by the value, if you want to set the scale directly you must first normalize the hexagon
  {
    get { return _scale; }
    set
    {
      _scale = value;
      v1 *= _scale;
      v2 *= _scale;
      v3 *= _scale;
      v4 *= _scale;
      v5 *= _scale;
      v6 *= _scale;
      center *= _scale;
    }
  }

  public Hexagon(){}
  public Hexagon(int i, Vector3 c, Vector3[] verts, SerializableVector3 origin, bool isPent = false)
  {
    index = i;
    neighbors = new int[]{-1,-1,-1,-1,-1,-1};
    center = c;
    v1 = verts[0];
    v2 = verts[1];
    v3 = verts[2];
    v4 = verts[3];
    v5 = verts[4];
    v6 = verts[5];
    normal = (SerializableVector3)(((Vector3)(center - origin)).normalized);
    isPentagon = isPent;
  }
}
