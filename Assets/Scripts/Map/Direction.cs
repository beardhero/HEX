public static class Direction
{
  public static string ToString(int dir)
  {
    switch (dir)
    {
      case 0:   return "Positive Y";
      case 1:   return "Positive XY";
      case 2:   return "Positive X";
      case 3:   return "Negative Y";
      case 4:   return "Negative XY";
      case 5:   return "Negative X";
      default:  return "Not a direction";
    }
  }
  
  public const int Y=0, // 0, 1
  XY    = 1,     // 1, 1
  X     = 2,      // 1, 0
  NegY  = 3,   // 0, -1
  NegXY = 4,  // -1, -1
  NegX  = 5,   // -1, 0
  Count = 6;
}

