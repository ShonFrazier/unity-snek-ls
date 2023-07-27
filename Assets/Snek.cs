using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SnekCell
{
  public Vector3Int GridPosition;
  Color Color;

  public GameObject GameObject;
  public int RemainingLife;

  public SnekCell(int remainingLife, Vector3Int gridPosition, Texture2D texture, Color color)
  {
    RemainingLife = remainingLife;
    GridPosition = gridPosition;
    Color = color;

    var textureRect = new Rect(0, 0, texture.width, texture.height);

    GameObject = new GameObject($"SnekCell {gridPosition}");
    var sr = GameObject.AddComponent<SpriteRenderer>();
    sr.sprite = Sprite.Create(texture, textureRect, new Vector2(0.5f, 0.5f));
    sr.color = color;
    sr.transform.localScale = 7f * Constants.xy;
  }
}

public class Snek
{
  GameController GameController;
  public List<SnekCell> Cells;

  private Vector3Int _Direction = Vector3Int.zero;
  public Vector3Int Direction
  {
    get
    {
      return _Direction;
    }

    set
    {
      if (value != _Direction * -1)
      {
        _Direction = value;
      }
    }
  }
  public bool IsAlive { get; private set; }
  public int Score { get; private set; } = 0;

  Texture2D Texture;
  Color Color;

  public SnekCell HeadCell
  {
    get
    {
      return Cells.Aggregate((liveliest, cell) => cell.RemainingLife > liveliest.RemainingLife ? cell : liveliest);
    }
  }

  SnekCell CreateSnekCell(Vector3Int gridPosition, Texture2D texture, Color color)
  {
    return new SnekCell(Score + 1, gridPosition, texture, color);
  }

  public Snek(GameController gameController, Vector3Int gridPosition, Vector3Int direction, Texture2D texture, Color color)
  {
    GameController = gameController;
    Cells = new List<SnekCell>
        {
            CreateSnekCell(gridPosition, texture, color)
        };

    Direction = direction;
    IsAlive = true;
    Texture = texture;
    Color = color;
  }

  public void Die()
  {
    IsAlive = false;
  }

  public void Eat()
  {
    Score += 1;
    Cells.ForEach(cell => cell.RemainingLife += 1);
  }

  // Start is called before the first frame update
  public void Start()
  {

  }

  // Update is called once per game cycle
  public void Update()
  {
    List<SnekCell> destroyGameObjects = new List<SnekCell>();

    if (!IsAlive)
    {
      Debug.Log("We are dead, destroy all cells");
      destroyGameObjects = Cells;
    }
    else
    {
      // get currentHeadPosition
      var currentHeadPosition = HeadCell.GridPosition;

      // calculate nextHeadPosition
      // query GC about nextHeadPosition, getting a modified nextHeadPosition
      var nextHeadPosition = GameController.RequestMoveTo(currentHeadPosition + Direction);

      // iterate snekcells decrementing life;
      Cells.ForEach(cell => cell.RemainingLife -= 1);

      // Destroy objects for cells who have 'died'
      destroyGameObjects = Cells.Where(cell => cell.RemainingLife <= 0).ToList();

      Cells = Cells.Where(cell => cell.RemainingLife > 0).ToList();

      // create new cell at modified nextHeadPosition
      Cells.Add(CreateSnekCell(nextHeadPosition, Texture, Color));
    }

    destroyGameObjects.ForEach(cell => Object.Destroy(cell.GameObject));
  }
}
