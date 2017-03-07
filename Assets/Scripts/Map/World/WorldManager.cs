using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(WorldRenderer))]
public class WorldManager : MonoBehaviour
{
  // === Public ===
  public Transform textMeshPrefab;
  [HideInInspector] public World activeWorld;
  public TileSet regularTileSet;
  public float maxMag = 10;
  public float worldScale = 1;
  public int worldSubdivisions = 1;
  public static SimplexNoise simplex;
  public static int uvWidth = 100;
  public static int uvHeight;
 
  // === Private ===
  bool labelDirections;
  //@TODO: These are for creating the heights, and are properties which should be serialized when we go to persistent galaxy.
  private int octaves, multiplier;
  private float amplitude, lacunarity, dAmplitude;

  // === Properties ===
  private float _averageScale;
  public float averageScale
  {
    get {
      _averageScale = 0;
      foreach (TriTile tt in activeWorld.triTiles)
      {
        _averageScale += tt.height;
      }
      _averageScale /= activeWorld.triTiles.Count;
      return _averageScale; }
    set { _averageScale = value; }
  }


  // === Cache ===
  WorldRenderer worldRenderer;
  GameObject currentWorldObject;
  Transform currentWorldTrans;
  //int layermask; @TODO: stuff

  //for type changer
  public Ray ray;
  public RaycastHit hit;
  public TileType switchToType;
  public float heightToSet;
  //float uvTileWidth = regularTileSet.tileWidth / texWidth;
  //float uvTileHeight = regularTileSet.tileWidth / texHeight;

  void Update()
  {
    //Type Changer
    if (Input.GetKeyDown(KeyCode.Mouse1))
    {
      
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      //ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(ray, out hit, 100.0f))
      {
        StartCoroutine(TypeChange(hit));
      }
    }
  }

  void OnDrawGizmos()
  {
    //Debug.Log("going here");
    //DrawAxes();
  }

  public World Initialize(bool loadWorld = false)
  {
    simplex = new SimplexNoise(GameManager.gameSeed);
    octaves = Random.Range(4, 4);
    multiplier = Random.Range(10, 10);
    amplitude = Random.Range(0.6f, 1f);
    lacunarity = Random.Range(0.7f, .9f);
    dAmplitude = Random.Range(0.5f, .1f);


    if (loadWorld)
    {
      activeWorld = LoadWorld();
    }
    else
    {
      activeWorld = new World();
      activeWorld.PrepForCache(worldScale, worldSubdivisions);
    }
    
    //Seed the world heights
    //SetHeights();
    
    //CreateOcean();
    
    currentWorldObject = new GameObject("World");
    currentWorldTrans = currentWorldObject.transform;

    //currentWorld = new World(WorldSize.Small, WorldType.Verdant, Season.Spring, AxisTilt.Slight);

    worldRenderer = GetComponent<WorldRenderer>();
    //changed this to run TriPlates instead of HexPlates
    foreach (GameObject g in worldRenderer.TriPlates(activeWorld, regularTileSet))
    {
      g.transform.parent =currentWorldTrans;
    }

    //layermask = 1 << 8;   // Layer 8 is set up as "Chunk" in the Tags & Layers manager

    //labelDirections = true;

    //DrawHexIndices();

    return activeWorld;
  }

  World LoadWorld()
  {
    return BinaryHandler.ReadData<World>(World.cachePath);
  }

  void SetHeights() //@TODO we should be reading heights from hextile (based on the worldseed?)
  {
    //Alright, let's expand on the simplex with some height adjustments from the plate tectonics
    //Each plate has has two axes it's moving on with a small velocity, each tile shares this movement
    foreach (TriTile tt in activeWorld.triTiles)
    {
	    //Debug.Log (ht.height);
      tt.height = 1f + tt.height;
    }

    /*
    float s = Random.Range(-99999,99999);
    foreach (HexTile ht in activeWorld.tiles)
    {
      ht.hexagon.Scale(1f + (int)(100 * (0.7f * Mathf.Abs(simplex.coherentNoise(ht.hexagon.center.x, ht.hexagon.center.y, ht.hexagon.center.z, octaves, multiplier, amplitude, lacunarity, dAmplitude) //))) / 100f);
                           + 0.3f * Mathf.Abs(simplex.coherentNoise(s*ht.hexagon.center.x, s*ht.hexagon.center.y, s*ht.hexagon.center.z, octaves, multiplier, amplitude, lacunarity, dAmplitude)))))/100f);
      //Debug.Log(1f + (int)(100 * (0.7f * Mathf.Abs(simplex.coherentNoise(ht.hexagon.center.x, ht.hexagon.center.y, ht.hexagon.center.z, octaves, multiplier, amplitude, lacunarity, dAmplitude)
      //                      + 0.3f * Mathf.Abs(simplex.coherentNoise(s * ht.hexagon.center.x, s * ht.hexagon.center.y, s * ht.hexagon.center.z, octaves, multiplier, amplitude, lacunarity, dAmplitude)))))/100f);
      //Debug.Log(ht.hexagon.scale);
    }
    */
  }
  //@TODO: This is preliminary, it sets the ocean tiles using average scale 
  //by making any tile close to the average or below blue, then scaling the blue tiles up to the average.
  void CreateOcean()
  {
    foreach (TriTile ht in activeWorld.triTiles)
    {
      ht.type = TileType.Blue;
    }
    TileType typeToSet = TileType.Tan;
    foreach (TriTile ht in activeWorld.triTiles)
    {
      float rand = Random.Range(0, 1f);
      //@TODO: this is just a preliminary variation of the land types
      if (rand <= 0.4f)
        typeToSet = TileType.Brown;
      if (rand > 0.4f)
        typeToSet = TileType.Red;
      if (ht.height >= averageScale*0.99f)
      {
        ht.type = typeToSet;
      }
    }
    foreach (TriTile ht in activeWorld.triTiles)
    {
      if (ht.type == TileType.Blue)
      {
        ht.height *= (averageScale*0.99f / ht.height);
      }
    }
  }
  //So now with the land masses, we're going to make the "biomes" more coherent like we did in Zone -> SpreadGround and RefineGround
  void RefineTypes()
  {
    foreach (TriTile ht in activeWorld.triTiles)
    {
      //int i = 0;
      //foreach (HexTile h in ht.ne) ;
    }
  }
  void DrawAxes()
  {
    if (!labelDirections || activeWorld.tiles.Count == 0)
      return;

    //int currentTileX = 13, currentTileY = 0, currentTileXY = 0;  

    // === Draw axes on all tiles ===
    for (int i=0; i<activeWorld.tiles.Count; i++)
    {
      DrawHexAxes(activeWorld.tiles, activeWorld.origin, i);
    }

    /*
    // === Draw Bands Only ===
    // Y-band
    for (int y=0; y<activeWorld.circumferenceInTiles; y++)
    {
      if (currentTileY != -1)
      {
        DrawHexAxes(activeWorld.tiles, activeWorld.origin, currentTileY);
        currentTileY = activeWorld.tiles[currentTileY].GetNeighborID(Direction.Y);
      }
    }
    // XY-band
    for (int xy=0; xy<activeWorld.circumferenceInTiles; xy++)
    {
      if (currentTileXY != -1)
      {
        DrawHexAxes(activeWorld.tiles, activeWorld.origin, currentTileXY);
        currentTileXY = activeWorld.tiles[currentTileXY].GetNeighborID(Direction.XY);
      }
    }
    // X-band
    for (int x=0; x<activeWorld.circumferenceInTiles; x++)
    {
      if (currentTileX != -1)
      {
        DrawHexAxes(activeWorld.tiles, activeWorld.origin, currentTileX);
        currentTileX = activeWorld.tiles[currentTileX].GetNeighborID(Direction.X);
      }
    }
    */
  }

  void DrawHexAxes(List<HexTile> tiles, Vector3 worldOrigin, int index, float scale = .1f, bool suppressWarnings = true)
  {
    if (index == -1)
    {
      Debug.LogError("Invalid index: -1");
      return;
    }

    SerializableVector3 origin = new SerializableVector3();
    try
    {
      origin = (tiles[index].hexagon.center + (SerializableVector3)worldOrigin) * 1.05f;
    }
    catch(System.Exception e)
    {
      Debug.LogError("Error accessing tile "+ index+": "+e);
      return;
    }

    for (int dir = 0; dir<Direction.Count && dir<tiles[index].hexagon.neighbors.Length; dir++)
    {
      int y = tiles[index].GetNeighborID(dir);
      if (y != -1)
      {
        Gizmos.color = Direction.ToColor(dir);
        SerializableVector3 direction = tiles[tiles[index].GetNeighborID(dir)].hexagon.center - tiles[index].hexagon.center;

        float finalScale = scale;
        if (dir == Direction.X || dir == Direction.Y || dir == Direction.NegXY)   // Prime directions
          finalScale *= 2;

        Gizmos.DrawRay((Vector3)origin, (Vector3)direction*finalScale);
      }
    }
  }

  void DrawHexIndices()
  {
    foreach (HexTile ht in activeWorld.tiles)
    {
      Transform t = (Transform)Instantiate(textMeshPrefab, (ht.hexagon.center-activeWorld.origin)*1.01f, Quaternion.LookRotation(activeWorld.origin-ht.hexagon.center));
      TextMesh x = t.GetComponent<TextMesh>();
      x.text = ht.index.ToString();
      t.parent = currentWorldTrans;
    }
  }

  public IEnumerator TypeChange(RaycastHit hit)
  {
    Debug.Log("hit");
    float uv2x = 1.0f / worldRenderer.tileCountW;
    float uv1x = uv2x / 2;
    float uv1y = 1.0f / worldRenderer.tileCountH;
    Vector2 uv0 = Vector2.zero,
            uv1 = new Vector2(uv1x, uv1y),
            uv2 = new Vector2(uv2x, 0);
    //Debug.Log("hit");
    float last = 9999999;
    TriTile hitTile = new TriTile();
    foreach (TriTile ht in activeWorld.triTiles)
    {
      Vector3 center = new Vector3(ht.center.x, ht.center.y, ht.center.z);
      float test = (center - hit.point).sqrMagnitude;
      if (test < last)
      {
        last = test;
        hitTile = ht;
      }
    }
    //IntCoord oldCoord = regularTileSet.GetUVForType(hitTile.type);
    //Vector2 oldOffset = new Vector2((oldCoord.x * uv2.x), (oldCoord.y * uv1.y));
    
    //Debug.Log(hitTile.type);
    //Debug.Log(worldRenderer.uvTileWidth);
    //IntCoord newCoord = new IntCoord(regularTileSet.GetUVForType(hitTile.type).x, regularTileSet.GetUVForType(hitTile.type).y);
    //final offset
    
    //Vector2 uvOffset = newOffset - oldOffset;
    //Change uvs
    MeshCollider meshCollider = hit.collider as MeshCollider;
    Mesh mesh = meshCollider.sharedMesh;
    Vector3[] vertices = mesh.vertices;
    Vector2[] uvs = mesh.uv;
    int[] triangles = mesh.triangles;
    //change height
    if (heightToSet == 0)
    {
      heightToSet = averageScale;
    }
    vertices[triangles[hit.triangleIndex * 3]] /= vertices[triangles[hit.triangleIndex * 3]].magnitude;
    vertices[triangles[hit.triangleIndex * 3]] *= heightToSet;

    vertices[triangles[hit.triangleIndex * 3 + 1]] /= vertices[triangles[hit.triangleIndex * 3 + 1]].magnitude;
    vertices[triangles[hit.triangleIndex * 3 + 1]] *= heightToSet;

    vertices[triangles[hit.triangleIndex * 3 + 2]] /= vertices[triangles[hit.triangleIndex * 3 + 2]].magnitude;
    vertices[triangles[hit.triangleIndex * 3 + 2]] *= heightToSet;

    //change type
    if (switchToType != TileType.None)
    {
      Debug.Log(hitTile.type);
      hitTile.type = switchToType;
      IntCoord newCoord = regularTileSet.GetUVForType(switchToType);
      Vector2 newOffset = new Vector2((newCoord.x * uv2.x), (newCoord.y * uv1.y));
      uvs[triangles[hit.triangleIndex * 3]] = uv0 + newOffset;
      uvs[triangles[hit.triangleIndex * 3 + 1]] = uv1 + newOffset;
      uvs[triangles[hit.triangleIndex * 3 + 2]] = uv2 + newOffset;
    }
    mesh.vertices = vertices;
    mesh.uv = uvs;
   
    Transform hitTransform = hit.collider.transform;
    yield return null;
  }
}
