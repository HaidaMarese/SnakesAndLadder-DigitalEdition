using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BoardLink
{
    [Tooltip("Start tile number (bottom of ladder or head of snake).")]
    public int from;
    [Tooltip("Destination tile number (top of ladder or tail of snake).")]
    public int to;
}

public class BoardManager : MonoBehaviour
{
    [SerializeField] private SpriteRenderer boardSpriteRenderer;
    [SerializeField] private int rows = 10;
    [SerializeField] private int columns = 10;

    [Header("Snakes & Ladders")]
    [SerializeField] private List<BoardLink> ladders = new List<BoardLink>();
    [SerializeField] private List<BoardLink> snakes = new List<BoardLink>();

    private Dictionary<int, Vector3> _tilePositions;
    private Dictionary<int, int> _links;

    public int MaxTile => rows * columns;

    private void Awake()
    {
        BuildTilePositions();
        BuildLinks();
    }

    private void BuildTilePositions()
    {
        _tilePositions = new Dictionary<int, Vector3>(rows * columns);

        Bounds b = boardSpriteRenderer.bounds;
        float cellWidth = b.size.x / columns;
        float cellHeight = b.size.y / rows;

        Vector3 bottomLeft = new Vector3(
            b.min.x + cellWidth * 0.5f,
            b.min.y + cellHeight * 0.5f,
            0f);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int baseIndex = row * columns + 1;
                int tileNumber = (row % 2 == 0)
                    ? baseIndex + col        // left to right
                    : baseIndex + (columns - 1 - col); // right to left

                Vector3 worldPos = new Vector3(
                    bottomLeft.x + col * cellWidth,
                    bottomLeft.y + row * cellHeight,
                    0f);

                _tilePositions[tileNumber] = worldPos;
            }
        }
    }

    private void BuildLinks()
    {
        _links = new Dictionary<int, int>();

        foreach (var ladder in ladders)
        {
            if (ladder.from >= 1 && ladder.to <= MaxTile && ladder.to > ladder.from)
            {
                _links[ladder.from] = ladder.to;
            }
        }

        foreach (var snake in snakes)
        {
            if (snake.from >= 1 && snake.to <= MaxTile && snake.to < snake.from)
            {
                _links[snake.from] = snake.to;
            }
        }
    }

    public Vector3 GetWorldPositionForTile(int tileNumber)
    {
        if (tileNumber < 1) tileNumber = 1;
        if (tileNumber > MaxTile) tileNumber = MaxTile;

        if (_tilePositions.TryGetValue(tileNumber, out var pos))
            return pos;

        Debug.LogWarning($"Tile {tileNumber} not found; returning (0,0,0).");
        return Vector3.zero;
    }

    /// <summary>
    /// If tile has snake or ladder start, returns destination tile. Otherwise returns same tile.
    /// </summary>
    public int ResolveLinkDestination(int tileNumber)
    {
        if (_links != null && _links.TryGetValue(tileNumber, out int dest))
            return dest;

        return tileNumber;
    }
}