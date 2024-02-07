using DG.Tweening;
using UnityEngine;
using System.Collections;
using Utilities.Audio;

public class ShapesSlash : MonoBehaviour
{

    #region Public Attributes

    public float duration;
    public SpriteRenderer slashSR;
    public SpriteRenderer shapeSR;
    private Vector3 originalScale;

    #endregion

    #region Public Methods

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        shapeSR.DOFade(0, 0.0001f);
        slashSR.DOFade(0, 0.0001f);
    }

    public void DisplaySlash(Vector3 _slashPos, Sprite _shapeSp)
    {
        shapeSR.sprite = _shapeSp;
        transform.position = _slashPos;
        transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        AudioController.Instance.PlayAudio(AudioName.STRIPED_WOOSH);

        StartCoroutine(Display());

        IEnumerator Display()
        {
            shapeSR.DOFade(1, 0.5f);
            slashSR.DOFade(1, 1);
            transform.DOScale(originalScale, 0.35f);

            yield return new WaitForSeconds(duration);

            shapeSR.DOFade(0, 0.5f);
            slashSR.DOFade(0, 1);
            transform.DOScale(new Vector3(0.3f, 0.3f, 0.3f), 0.5f);
        }
    }

    #endregion

}