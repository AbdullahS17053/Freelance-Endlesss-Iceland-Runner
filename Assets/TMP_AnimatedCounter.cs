using UnityEngine;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TMP_NumberAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    public float punchScale = 1.3f;
    public float duration = 0.25f;
    public Ease ease = Ease.OutBack;

    private TextMeshProUGUI tmp;
    private string previousText = "";

    void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        previousText = tmp.text;
    }

    void LateUpdate()
    {
        if (tmp.text == previousText) return;

        AnimateChangedCharacters(previousText, tmp.text);
        previousText = tmp.text;
    }

    void AnimateChangedCharacters(string oldText, string newText)
    {
        tmp.ForceMeshUpdate();
        TMP_TextInfo textInfo = tmp.textInfo;

        int max = Mathf.Max(oldText.Length, newText.Length);

        for (int i = 0; i < max; i++)
        {
            if (i >= oldText.Length || i >= newText.Length || oldText[i] != newText[i])
            {
                AnimateCharacter(i, textInfo);
            }
        }
    }

    void AnimateCharacter(int charIndex, TMP_TextInfo textInfo)
    {
        if (charIndex >= textInfo.characterCount) return;

        TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];
        if (!charInfo.isVisible) return;

        int meshIndex = charInfo.materialReferenceIndex;
        int vertexIndex = charInfo.vertexIndex;

        Vector3[] vertices = textInfo.meshInfo[meshIndex].vertices;

        Vector3 charCenter = (vertices[vertexIndex] + vertices[vertexIndex + 2]) / 2;

        // Punch animation
        DOTween.To(() => 1f, s =>
        {
            for (int i = 0; i < 4; i++)
            {
                vertices[vertexIndex + i] = charCenter + (vertices[vertexIndex + i] - charCenter) * s;
            }
            tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }, punchScale, duration)
        .SetEase(ease)
        .OnComplete(() =>
        {
            for (int i = 0; i < 4; i++)
            {
                vertices[vertexIndex + i] = charCenter + (vertices[vertexIndex + i] - charCenter);
            }
            tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        });
    }
}