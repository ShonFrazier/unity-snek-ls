using System;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEditor;
using UnityEngine;
using System.Linq;

public class GameController : MonoBehaviour
{
  GameState GameState;

  List<SnekInfo> StartingSnekInfo;

  Food CreateFood(Vector3Int gridPosition, Texture2D texture)
  {
    var food = new Food(gridPosition, texture, Color.red);
    var grid = GetComponent<GridLayout>();

    var foodWorldPos = grid.CellToWorld(gridPosition);
    foodWorldPos.Scale(Constants.xy);
    food.GameObject.transform.position = foodWorldPos;

    Debug.Log($"food.GameObject.transform.position: {food.GameObject.transform.position}");

    return food;
  }

  List<Snek> CreateSneks(List<SnekInfo> snekInfoList)
  {
    return snekInfoList.Select(si => new Snek(this, si.Position, si.Direction, si.Texture, si.Color)).ToList();
  }

  public record SnekInfo
  {
    public Vector3Int Position;
    public Vector3Int Direction;
    public Texture2D Texture;
    public Color Color;
  }

  public Vector3Int RequestMoveTo(Vector3Int gridPosition) => GameState.GridExtent.Wrap(gridPosition);

  // Start is called before the first frame update
  void Start()
  {
    var camera = GetComponent<Camera>();
    var grid = GetComponent<GridLayout>();

    GameState = new GameState(camera, grid);

    StartingSnekInfo = new List<SnekInfo>
    {
      new SnekInfo
      {
        Position = GameState.GridExtent.RandomPosition(),
        Direction = Vector3Int.left,
        Texture = GameState.whiteTexture,
        Color = Color.green
      }
    };

    GameState.Food = CreateFood(GameState.GridExtent.RandomPosition(), GameState.whiteTexture);
    GameState.Sneks = CreateSneks(StartingSnekInfo);

    Debug.Log("Start() exit");
  }

  int ticksPerSecond = 12;
  float deltaAccumulator = 0;
  void Update()
  {
    float secPerTick = 1f / ticksPerSecond;

    deltaAccumulator += Time.deltaTime;
    bool timeToMove = deltaAccumulator >= secPerTick;

    if (!timeToMove)
    {
      return;
    }

    if (GameState.Sneks.Count <= 0)
    {
      GameState.Sneks = CreateSneks(StartingSnekInfo);
    }

    deltaAccumulator -= secPerTick;

    float dirX = Input.GetAxis("Horizontal");
    float dirY = Input.GetAxis("Vertical");

    var primarySnek = GameState.Sneks[0];

    if (dirX > 0)
    {
      primarySnek.Direction = Vector3Int.right;
    }
    else if (dirX < 0)
    {
      primarySnek.Direction = Vector3Int.left;
    }

    if (dirY > 0)
    {
      primarySnek.Direction = Vector3Int.up;
    }
    else if (dirY < 0)
    {
      primarySnek.Direction = Vector3Int.down;
    }

    var didEat = false;
    var food = GameState.Food;
    var sneks = GameState.Sneks;
    sneks.ForEach(snek => snek.Update());

    //   filter
    GameState.Sneks = GameState.Sneks.Where(snek => snek.IsAlive).ToList();

    var grid = GetComponent<GridLayout>();

    sneks.ForEach(snek =>
      {
        var currentHead = snek.HeadCell;
        // check collisions with other sneks - die
        sneks.ForEach(other =>
          {
            snek.Cells.ForEach(otherCell =>
              {
                var worldPosition = grid.CellToWorld(otherCell.GridPosition);
                worldPosition.Scale(Constants.xy);
                otherCell.GameObject.transform.position = worldPosition;

                if (other == snek && otherCell == snek.HeadCell)
                {
                  return; // Skip!
                }

                if (currentHead.GridPosition == otherCell.GridPosition)
                {
                  snek.Die();
                }
              }
            );
          }
        );

        if (!didEat && snek.IsAlive && currentHead.GridPosition == food.GridPosition)
        {
          snek.Eat();
          didEat = true;
        }

        // check collision with food - eat
      }
    );

    if (didEat)
    {
      Destroy(food.GameObject);
      GameState.Food = CreateFood(GameState.GridExtent.RandomPosition(), GameState.whiteTexture);
    }
  }
}

public class GridExtent
{
  public RangeInt xRange { get; private set; }
  public RangeInt yRange { get; private set; }

  int Width
  {
    get
    {
      return xRange.length;
    }
  }

  int Height
  {
    get
    {
      return yRange.length;
    }
  }

  public GridExtent(int xMin, int xMax, int yMin, int yMax)
  {
    xRange = new RangeInt(xMin, xMax - xMin);
    yRange = new RangeInt(yMin, yMax - yMin);
  }

  public GridExtent(Vector3Int v1, Vector3Int v2) : this(Math.Min(v1.x, v2.x), Math.Max(v1.x, v2.x), Math.Min(v1.y, v2.y), Math.Max(v1.y, v2.y))
  {
  }

  public GridExtent Equalized
  {
    get
    {
      int xEqualized = Math.Min(Math.Abs(xRange.start), Math.Abs(xRange.end));
      int yEqualized = Math.Min(Math.Abs(yRange.start), Math.Abs(yRange.end));

      return new GridExtent(-xEqualized, xEqualized, -yEqualized, yEqualized);
    }
  }

  public Vector3Int RandomPosition()
  {
    var rng = new System.Random();
    int x = (int)Math.Floor(rng.NextDouble() * xRange.length + xRange.start);
    int y = (int)Math.Floor(rng.NextDouble() * yRange.length + yRange.start);

    return new Vector3Int(x, y, 0);
  }

  public Vector3Int Wrap(Vector3Int v)
  {
    Vector3Int newV = new Vector3Int(v.x, v.y, v.z);
    while (newV.x > xRange.end)
    {
      newV.x -= xRange.length + 1;
    }
    while (newV.x < xRange.start)
    {
      newV.x += xRange.length + 1;
    }

    while (newV.y > yRange.end)
    {
      newV.y -= yRange.length + 1;
    }
    while (newV.y < yRange.start)
    {
      newV.y += yRange.length + 1;
    }

    return newV;
  }

  public override string ToString()
  {
    return $"x: [{xRange.start}, {xRange.end}], y: [{yRange.start}, {yRange.end}]";
  }
}

public class Constants
{
  public static Vector3 xy { get; private set; } = new Vector3(1, 1, 0);
}

public class GameState
{
  public Food Food;
  public GameObject FoodGameObject;
  public List<Snek> Sneks;

  public Texture2D whiteTexture = Texture2D.whiteTexture;

  public GridExtent GridExtent;
  public Vector3 GridPlane = new Vector3(0, 0, 40);

  public GameState(Camera camera, GridLayout grid)
  {
    var screenWidth = Screen.width;
    var screenHeight = Screen.height;

    var screenTopLeft = new Vector3(0, 0, 0);
    var screenBottomRight = new Vector3(screenWidth, screenHeight, 0);

    Debug.Log($"screenTopLeft     {screenTopLeft}");
    Debug.Log($"screenBottomRight {screenBottomRight}");

    var worldTopLeft = camera.ScreenToWorldPoint(screenTopLeft);
    var worldBottomRight = camera.ScreenToWorldPoint(screenBottomRight);

    worldTopLeft.Scale(Constants.xy);
    worldBottomRight.Scale(Constants.xy);

    Debug.Log($"worldTopLeft     {worldTopLeft}");
    Debug.Log($"worldBottomRight {worldBottomRight}");

    Vector3Int gridTopLeft = grid.WorldToCell(worldTopLeft);
    Vector3Int gridBottomRight = grid.WorldToCell(worldBottomRight);

    GridPlane.z = gridTopLeft.z;

    Debug.Log($"gridTopLeft     {gridTopLeft}");
    Debug.Log($"gridBottomRight {gridBottomRight}");

    GridExtent = new GridExtent(gridTopLeft, gridBottomRight).Equalized;
  }
}