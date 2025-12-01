using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class GameManager : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private DiceRoller diceRoller;

    [Header("Players")]
    [SerializeField] private PlayerToken playerTokenPrefab;
    [SerializeField] private Transform tokensParent;
    [SerializeField]
    private Color[] playerColors =
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow
    };

    private readonly string[] colorNames = { "Red", "Blue", "Green", "Yellow" };

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI currentPlayerText;
    [SerializeField] private Button rollDiceButton;

    [Header("Win Popup")]
    [SerializeField] private GameObject winPopup;
    [SerializeField] private TextMeshProUGUI winnerText;

    [Header("Gameplay Settings")]
    [SerializeField] private float cpuTurnDelay = 1.0f;
    [SerializeField] private float extraTurnDelay = 0.5f;
    [SerializeField] RectTransform pausePanelRect;
    [SerializeField] float toPosY, middlePosY;
    [SerializeField] float tweenDuration;

    private class PlayerInfo
    {
        public int index;
        public bool isHuman;
        public PlayerToken token;
    }

    private List<PlayerInfo> _players = new List<PlayerInfo>();
    private int _currentPlayerIndex = 0;
    private bool _turnInProgress = false;
    private bool _gameOver = false;

    public void RestartGame()
    {
        LevelManager.Instance.LoadScene("GameScene", "CrossFade");
    }

    public void ReturnToHome()
    {
        LevelManager.Instance.LoadScene("PlayerSelect", "CrossFade");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
    }

    private void Start()
    {
        SetupPlayers();
        UpdateCurrentPlayerUI();
        PrepareCurrentPlayerTurn();
    }

    private void SetupPlayers()
    {
        _players.Clear();

        int humanCount = Mathf.Clamp(GameSettings.PlayerCount, 2, 4);
        int totalPlayers = 4;

        for (int i = 0; i < totalPlayers; i++)
        {
            PlayerToken tokenInstance = Instantiate(playerTokenPrefab, tokensParent);

            Color color = (i < playerColors.Length) ? playerColors[i] : Color.white;
            tokenInstance.SetColor(color);

            tokenInstance.SetToTile(1, boardManager);

            bool isHuman = i < humanCount;

            _players.Add(new PlayerInfo
            {
                index = i,
                isHuman = isHuman,
                token = tokenInstance
            });
        }
    }

    private void UpdateCurrentPlayerUI()
    {
        PlayerInfo p = _players[_currentPlayerIndex];
        string colorName = colorNames[p.index];
        string typeText = p.isHuman ? "Human" : "CPU";

        if (currentPlayerText != null)
            currentPlayerText.text = $"{colorName} ({typeText})'s Turn";
    }

    private void PrepareCurrentPlayerTurn()
    {
        if (_gameOver) return;

        PlayerInfo p = _players[_currentPlayerIndex];

        if (p.isHuman)
        {
            if (rollDiceButton != null)
            {
                rollDiceButton.interactable = true;
                rollDiceButton.onClick.RemoveAllListeners();
                rollDiceButton.onClick.AddListener(() =>
                {
                    if (!_turnInProgress)
                        StartCoroutine(HandleTurn(p));
                });
            }
        }
        else
        {
            if (rollDiceButton != null)
                rollDiceButton.interactable = false;

            StartCoroutine(CpuTurnCoroutine(p));
        }
    }

    private IEnumerator CpuTurnCoroutine(PlayerInfo p)
    {
        yield return new WaitForSeconds(cpuTurnDelay);
        if (!_turnInProgress && !_gameOver)
        {
            yield return HandleTurn(p);
        }
    }

    private IEnumerator HandleTurn(PlayerInfo player)
    {
        if (_turnInProgress || _gameOver)
            yield break;

        _turnInProgress = true;

        if (rollDiceButton != null)
            rollDiceButton.interactable = false;

        // Roll dice with animation
        yield return StartCoroutine(diceRoller.RollDice());
        int roll = diceRoller.LastRoll;

        int currentTile = player.token.CurrentTile;
        int targetTile = currentTile;

        int proposed = currentTile + roll;
        if (proposed <= boardManager.MaxTile)
            targetTile = proposed;

        // Move step-by-step
        if (targetTile != currentTile)
        {
            yield return StartCoroutine(player.token.MoveToTile(targetTile, boardManager, true));
        }

        // Check for ladder or snake
        int linkDest = boardManager.ResolveLinkDestination(targetTile);
        if (linkDest != targetTile)
        {
            // Determine if it's a ladder or snake and play sound
            if (linkDest > targetTile)
            {
                AudioManager.Instance?.PlayLadderSound();
            }
            else
            {
                AudioManager.Instance?.PlaySnakeSound();
            }

            yield return StartCoroutine(player.token.MoveToTile(linkDest, boardManager, false));
            targetTile = linkDest;
        }

        // Check for WIN
        if (targetTile == boardManager.MaxTile)
        {
            HandleWin(player);
            _turnInProgress = false;
            yield break;
        }

        // Check if player rolled a 6 - they get another turn!
        if (roll == 6)
        {
            _turnInProgress = false;
            PlayerInfo p = _players[_currentPlayerIndex];
            string colorName = colorNames[p.index];
            string typeText = p.isHuman ? "Human" : "CPU";

            if (currentPlayerText != null)
                currentPlayerText.text = $"{colorNames[player.index]} rolled a 6! Roll again!";

            yield return new WaitForSeconds(extraTurnDelay);

            // Same player goes again
            PrepareCurrentPlayerTurn();
        }
        else
        {
            // Normal turn advancement
            AdvanceTurn();
            _turnInProgress = false;
        }
    }

    private void AdvanceTurn()
    {
        _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
        UpdateCurrentPlayerUI();
        PrepareCurrentPlayerTurn();
    }

    private void HandleWin(PlayerInfo winner)
    {
        _gameOver = true;

        // Pause all animations and sounds
        Time.timeScale = 0f;

        string colorName = colorNames[winner.index];
        string msg = $"{colorName} Wins!";

        // Play win sound
        AudioManager.Instance?.PlayWinSound();

        if (rollDiceButton != null)
        {
            rollDiceButton.interactable = false;
            rollDiceButton.onClick.RemoveAllListeners();
        }

        if (winPopup != null)
            winPopup.SetActive(true);

        if (winnerText != null)
            winnerText.text = msg;
    }

    public void PausePanelIntro()
    {
        pausePanelRect.DOAnchorPosY(middlePosY, tweenDuration);
    }

    async public void PausePanelOutro()
    {
        await pausePanelRect.DOAnchorPosY(toPosY, tweenDuration).SetUpdate(true).AsyncWaitForCompletion();
    }
}