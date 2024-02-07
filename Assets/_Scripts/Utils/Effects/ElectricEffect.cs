using DG.Tweening;
using UnityEngine;

public class ElectricEffect : MonoBehaviour
{

    public void DisplayEffect(Transform targetDest)
    {
        var visuals = transform.GetChild(0).GetComponent<SpriteRenderer>();
        visuals.DOFade(1, 0.5f);
        DOVirtual.DelayedCall(0.5f, () => visuals.DOFade(0, 0.25f).OnComplete(() =>
        Destroy(gameObject)));

        transform.GetChild(1).gameObject.SetActive(true);
        Vector3 direction = targetDest.position - transform.position;

        // Calculate distance considering both X and Y components
        float distance = new Vector2(direction.x, direction.y).magnitude;
        transform.localScale = new Vector3(distance * 0.425f, transform.localScale.y, transform.localScale.z);

        // Calculate the look rotation towards the endPoint
        var lookrotation = targetDest.position - transform.position;
        var newRotation = Quaternion.LookRotation(lookrotation, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, 1);
        transform.Rotate(0f, 90f, 0f);
    }

}