using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public static class BoardAnimations
{
    public static Sequence PlayStoningSegmentsEffect(List<Transform> segments)
    {
        List<Material> toAnimate = segments
            .Select(seg => seg.GetComponent<MeshRenderer>())
            .Where(renderer => renderer != null)
            .Select(renderer => renderer.material).ToList();

        Sequence sequence = DOTween.Sequence();

        foreach (Material material in toAnimate)
        {
            sequence.Insert(0, material.DOFloat(4f, "_Height", 5f)).SetEase(Ease.OutCirc);
        }

        return sequence;
    }

    public static Sequence PlayDissolveEffect(List<Transform> segments, int clearedLine, int rowIndex,
        ParticleSystem lineEffect)
    {
        List<Material> toAnimate = segments.Select(seg => seg.GetComponent<MeshRenderer>().material).ToList();

        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() => { lineEffect.transform.localPosition = new Vector3(0, 0.2f * rowIndex, 0); });
        sequence.AppendCallback(lineEffect.Play);

        foreach (Material material in toAnimate)
        {
            sequence.Insert(0.2f,
                material.DOFloat(1f, "_DissolveAmount", 0.14f));
        }

        return sequence;
    }
}