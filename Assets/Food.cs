using UnityEngine;

public class Food
{
  public Color Color { get; private set; }
  public Vector3Int GridPosition { get; private set; }
  public GameObject GameObject;

  public Food(Vector3Int gridPosition, Texture2D texture, Color color)
  {
    GridPosition = gridPosition;
    Color = color;

    var textureRect = new Rect(0, 0, texture.width, texture.height);

    GameObject = new GameObject($"Food {gridPosition}");
    var sr = GameObject.AddComponent<SpriteRenderer>();
    sr.sprite = Sprite.Create(texture, textureRect, new Vector2(0.5f, 0.5f));
    sr.color = color;
    sr.transform.localScale = 7f * Constants.xy;
  }
}
