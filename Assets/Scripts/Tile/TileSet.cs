﻿using UnityEngine;
using System;
using System.Collections;

[System.Serializable]
public class TileSet
{
  public Texture2D texture;
  public int tileWidth;
  public TypeMap[] typeUVs;

  bool initialized = false;

  TypeMap[] _typeUVs;

  void Initialize()
  {
    int length = Enum.GetValues(typeof(TileType)).Length;
    _typeUVs = new TypeMap[length];

    foreach (TypeMap t in typeUVs)
    {
      _typeUVs[(int)t.type] = t;
    }

    initialized = true;
  }

  public IntCoord GetUVForType(TileType t)
  {
    if (!initialized)
      Initialize();

    if (_typeUVs[(int)t] == null)
      return IntCoord.Zero();

    return _typeUVs[(int)t].coord;
  }

  [System.Serializable]
  public class TypeMap
  {
    public TileType type;
    public IntCoord coord;
  }
}
