using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DiceRoller : MonoBehaviour
{
    [SerializeField] private Sprite[] diceFaces;
    [SerializeField] private float rollDuration = 0.6f;
    [SerializeField] private float faceChangeInterval = 0.05f;

    private SpriteRenderer _spriteRenderer;
    public int LastRoll { get; private set; } = 1;
    public bool IsRolling { get; private set; }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (diceFaces != null && diceFaces.Length > 0)
        {
            _spriteRenderer.sprite = diceFaces[0];
            LastRoll = 1;
        }
    }

    public IEnumerator RollDice()
    {
        GetComponent<AudioSource>().Play();
        if (IsRolling || diceFaces == null || diceFaces.Length < 6)
            yield break;

        IsRolling = true;
        float elapsed = 0f;

        while (elapsed < rollDuration)
        {
            int randomFace = Random.Range(0, diceFaces.Length);
            _spriteRenderer.sprite = diceFaces[randomFace];

            elapsed += faceChangeInterval;
            yield return new WaitForSeconds(faceChangeInterval);
        }

        // Final result
        LastRoll = Random.Range(1, 7);
        _spriteRenderer.sprite = diceFaces[LastRoll - 1];

        IsRolling = false;
    }
}