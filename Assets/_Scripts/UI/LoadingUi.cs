using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadingUi : Singleton<LoadingUi>
{

    public float loadDuration;
    public Image loadingSlider;
    public TMP_Text laodingText;
    public GameObject loadingUI;

    private void Start()
    {
        LoadScene("Menu");
    }

    public void LoadScene(string _sceneName)
    {
        StartCoroutine(StartLoading());
        IEnumerator StartLoading()
        {
            loadingSlider.fillAmount = 0;
            loadingUI.SetActive(true);

            yield return StartCoroutine(DisplayLoadUI(loadDuration, 0.5f));
            yield return StartCoroutine(LoadSceneAsync(_sceneName));
            yield return StartCoroutine(DisplayLoadUI(loadDuration, 1f));

            loadingUI.SetActive(false);
        }
    }

    private IEnumerator DisplayLoadUI(float _duration, float _fillAmount, Func<bool> _breakCondition = null)
    {
        float startTime = Time.time;
        float initialSliderValue = loadingSlider.fillAmount;

        while (Time.time - startTime < _duration)
        {
            if (_breakCondition != null)
            {
                if (_breakCondition())
                    break;
            }

            float t = (Time.time - startTime) / _duration;
            loadingSlider.fillAmount = Mathf.Lerp(initialSliderValue, _fillAmount, t);
            laodingText.text = Mathf.RoundToInt(loadingSlider.fillAmount * 100) + "%";
            yield return null;
        }
        loadingSlider.fillAmount = _fillAmount;
        laodingText.text = Mathf.RoundToInt(loadingSlider.fillAmount * 100) + "%";
    }

    private IEnumerator LoadSceneAsync(string _sceneName)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_sceneName);
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
    }

}