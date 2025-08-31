using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int totalBoxs;
    public int finishedBoxs;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            ResetStage();
    }

    public void CheckFinish()
    {
        if(finishedBoxs == totalBoxs)
        {
            print("YOU WIN!");
            StartCoroutine(LoadNextStage());
        }
    }

    void ResetStage()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator LoadNextStage()
    {
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
