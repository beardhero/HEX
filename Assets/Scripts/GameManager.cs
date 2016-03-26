/*
 * Copyright (c) 2015 Colin James Currie.
 * All rights reserved.
 * Contact: cj@cjcurrie.net
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public enum GameState {None, Caching, MainMenu, WorldMap, ZoneMap, WorldDuel};

public class GameManager : MonoBehaviour
{
  // === Const & Inspector Cache ===
  public GameState beginningState = GameState.WorldMap;
  public const string gameSeed = "sixtynine";

  // === Static Cache ===
  static GameState state;
  public static Transform myTrans;
  public static GameState State {get{return state;} set{}}
  public static Camera cam;
  public static MainUI mainUI;

  //For World/Zone
  public static World currentWorld;
  public static GameObject worldManagerObj;
  public static WorldManager worldManager;
  static CreateWorldCache worldCacher;

  public static Zone currentZone;
  public static ZoneManager zoneManager;
  public static ZoneRenderer zoneRenderer;
  public static List<GameObject> currentZoneObjects;
  public static ZoneViewCamera zoneCameraControls;

  // For combat
  public static GameObject combatManagerObj;
  public static CombatManager combatManager;
  public static RoundManager roundManager;
  
  // *** Main Initializer ***
  void Awake ()
  {
    myTrans = transform;
    cam = Camera.main;
    if (Camera.main)
      zoneCameraControls = Camera.main.GetComponent<ZoneViewCamera>();

    // @TODO: Make these a singleton pattern
    currentZone = new Zone(1); // Required so Hex doesn't null ref currentZone
    Hex.Initialize();

    // Ideally, the only place state is manually set.
    state = beginningState;
    bool loading;
    switch (state)
    {
      case GameState.WorldDuel:
        loading = false; //@TODO: still caching on duel
        InitializeWorld(loading);

        InitializeCombat();
        combatManager.BeginDuel();
      break;

      case GameState.WorldMap:
        loading = true;
        InitializeWorld(loading);
      break;

      case GameState.ZoneMap:
        InitializeZone();
      break;

      case GameState.Caching:
        loading = false;
        InitializeWorld(loading);

        worldCacher = worldManagerObj.GetComponent<CreateWorldCache>();
        worldCacher.BuildCache(worldManager.activeWorld);
      break;

      default:
        Debug.LogError("Please set a state in GameManager.beginningState before playing.");
      break;
    }
  }

  void InitializeWorld(bool loading)
  {
    worldManagerObj = GameObject.FindWithTag("World Manager");
    worldManager = worldManagerObj.GetComponent<WorldManager>();
    currentWorld = worldManager.Initialize(loading);
  }

  void InitializeCombat()
  {
    combatManagerObj = GameObject.FindWithTag("Combat Manager");
    combatManager = combatManagerObj.GetComponent<CombatManager>();
    combatManager.Initialize(currentWorld);
  }

  void InitializeZone()
  {
    zoneManager = GameObject.FindWithTag("Zone Manager").GetComponent<ZoneManager>();
    zoneRenderer = zoneManager.GetComponent<ZoneRenderer>();

    // --- Input

    // --- Network

    // --- Zone
    if (currentZoneObjects != null && currentZoneObjects.Count > 0)
    {
      foreach (GameObject g in currentZoneObjects)
        Destroy (g);
    }
    int safety = 100;
    bool buildingZone = true;
    int minimumSize = 1750;

    Triangle tri = new Triangle(new Vector3(0, 0, 0), new Vector3(18, 0, 24), new Vector3(0, 0, 36));

    while (buildingZone)
    {
      currentZone = new Zone(tri);

      if (currentZone.landArea > minimumSize)
      {
        Debug.Log("Zone generated with a land mass of "+currentZone.landArea+" hex.");
        buildingZone = false;
      }
      else if (currentZone.landArea>0)
      {
        Debug.Log("Land mass is too low. New level being generated....");
      }
      else
      {
        Debug.Log("Underwater level detected. New level being generated....");
      }

      safety--;
      if (safety < 0)
        break;
    }

    currentZoneObjects = zoneRenderer.RenderZone(currentZone, zoneManager.regularTileSet);
    zoneManager.Initialize(currentZone);
  }

  void OnGUI()
  {
    //mainUI.OnMainGUI(); TURN BACK ON LATER
  }

  public static void OnTapInput(Vector2 tap)
  {
    switch (state)
    {
      case GameState.ZoneMap:
        roundManager.OnTapInput(tap);
      break;
    }
  }
}