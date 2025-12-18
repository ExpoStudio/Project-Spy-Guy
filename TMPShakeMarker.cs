using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class TMPShakeMarker : MonoBehaviour
{
    public TMP_Text textComponent;

    void Start()
    {
        ApplyShakeTagColors();
    }

    void ApplyShakeTagColors()
    {
        // 1. Strip <shake> tags for display
        string rawText = textComponent.text;
        string cleanedText = Regex.Replace(rawText, "</?shake>", "");
        textComponent.text = cleanedText;

        // 2. Find shake ranges
        List<(int start, int end)> shakeRanges = new List<(int, int)>();
        var matches = Regex.Matches(rawText, "<shake>(.*?)</shake>");
        int offset = 0;

        foreach (Match match in matches)
        {
            int start = match.Index - offset;
            int length = match.Groups[1].Value.Length;
            shakeRanges.Add((start, start + length - 1));
            offset += "<shake></shake>".Length;
        }

        // 3. Update mesh colors
        textComponent.ForceMeshUpdate();
        var textInfo = textComponent.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible)
                continue;

            bool shouldShake = false;
            foreach (var range in shakeRanges)
            {
                if (i >= range.start && i <= range.end)
                {
                    shouldShake = true;
                    break;
                }
            }

            var meshInfo = textInfo.meshInfo[charInfo.materialReferenceIndex];
            int vertexIndex = charInfo.vertexIndex;
            Color32 color = shouldShake ? new Color32(255, 0, 0, 255) : new Color32(255, 255, 255, 255);

            for (int j = 0; j < 4; j++)
            {
                meshInfo.colors32[vertexIndex + j] = color;
            }
        }

        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }
}
