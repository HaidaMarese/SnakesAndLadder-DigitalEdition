using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BootManager : MonoBehaviour
{
    public float delayTime = 4f;

    private void Start()
    {
        StartCoroutine(LoadSceneAfterDelay(delayTime));
    }

    IEnumerator LoadSceneAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        LevelManager.Instance.LoadScene("PlayerSelect", "CrossFade");
    }
}
