using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerToken : MonoBehaviour
{
    [SerializeField] private float moveDurationPerTile = 0.3f;
    [SerializeField] private float delayBetweenTiles = 0.1f;
    private SpriteRenderer _spriteRenderer;

    public int CurrentTile { get; private set; } = 1;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetColor(Color color)
    {
        _spriteRenderer.color = color;
    }

    public void SetToTile(int tileNumber, BoardManager board)
    {
        CurrentTile = tileNumber;
        transform.position = board.GetWorldPositionForTile(tileNumber);
    }

    public IEnumerator MoveToTile(int targetTile, BoardManager board, bool stepByStep = true)
    {
        int startTile = CurrentTile;
        if (targetTile == startTile)
            yield break;

        if (stepByStep)
        {
            // Move tile by tile (for dice roll movement)
            int direction = targetTile > startTile ? 1 : -1;

            while (CurrentTile != targetTile)
            {
                int nextTile = CurrentTile + direction;

                // Play sound for each tile step
                AudioManager.Instance?.PlayMoveSound();

                Vector3 startPos = transform.position;
                Vector3 endPos = board.GetWorldPositionForTile(nextTile);

                float elapsed = 0f;

                // Animate movement to next tile
                while (elapsed < moveDurationPerTile)
                {
                    float t = elapsed / moveDurationPerTile;
                    transform.position = Vector3.Lerp(startPos, endPos, t);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                transform.position = endPos;
                CurrentTile = nextTile;

                // Add a small delay before moving to next tile (except on last tile)
                if (CurrentTile != targetTile)
                {
                    yield return new WaitForSeconds(delayBetweenTiles);
                }
            }
        }
        else
        {
            // Slide directly (for snakes and ladders) - NO stepping through tiles
            Vector3 startPos = transform.position;
            Vector3 endPos = board.GetWorldPositionForTile(targetTile);

            float elapsed = 0f;
            float slideDuration = moveDurationPerTile * 1.5f;

            while (elapsed < slideDuration)
            {
                float t = elapsed / slideDuration;
                transform.position = Vector3.Lerp(startPos, endPos, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = endPos;
            CurrentTile = targetTile;
        }
    }
}