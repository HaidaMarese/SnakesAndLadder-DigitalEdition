using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerSelectManager : MonoBehaviour
{
    [SerializeField] private Button button2Players;
    [SerializeField] private Button button3Players;
    [SerializeField] private Button button4Players;
    [SerializeField] private Button Play;
    [SerializeField] private TextMeshProUGUI selectedText;

    private int _selectedPlayerCount = 2;

    private void Start()
    {
        // Default selection
        SetPlayerCount(2);

        button2Players.onClick.AddListener(() => SetPlayerCount(2));
        button3Players.onClick.AddListener(() => SetPlayerCount(3));
        button4Players.onClick.AddListener(() => SetPlayerCount(4));

        Play.onClick.AddListener(OnStartGame);
    }

    private void SetPlayerCount(int count)
    {
        _selectedPlayerCount = count;
        GameSettings.PlayerCount = count;

        if (selectedText != null)
            selectedText.text = $"Selected: {count} Player{(count > 1 ? "s" : "")}";
    }

    private void OnStartGame()
    {
        LevelManager.Instance.LoadScene("GameScene", "CrossFade");
        //SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
}