using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class LevelUi : MonoBehaviour
{

    [SerializeField] private TMP_Text index;
    [SerializeField] private Button button;
    [SerializeField] private List<Image> stars;

    private int levelIndex;

    public void Enable(int _starsAwarded, Action<int> _onclickAction)
    {
        for (int i = 0; i < stars.Count; i++)
            stars[i].enabled = i < _starsAwarded;

        button.interactable = true;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onclickAction?.Invoke(levelIndex));
    }
    
    public void Disable(int _index)
    {
        levelIndex = _index;
        button.interactable = false;
        index.text = (_index + 1).ToString();
        for (int i = 0; i < stars.Count; i++)
            stars[i].enabled = false;
    }

}