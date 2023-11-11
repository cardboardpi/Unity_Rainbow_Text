using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class RainbowText_V3 : MonoBehaviour
{
    [SerializeField] private Gradient textGradient;
    [SerializeField] private float gradientSpeed = .1f;
    [SerializeField] private string stringToAffect = "";

    private TMP_Text m_TextComponent;
    private float _totalTime;
    private int startAt;
    private int endAt;

    void Awake()
    {
        m_TextComponent = GetComponent<TMP_Text>();
    }


    void Start()
    {
        StartCoroutine(AnimateVertexColors());
    }

    //Checks whether the string that is our stringToAffect is in the text component, if so set the starting and ending points
    private void CheckText()
    {
        string mainText = m_TextComponent.text;
        string[] separator = {stringToAffect};
        
        
        if (mainText.Contains(stringToAffect))
        {
            string[] stringHolder = mainText.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            int amount = stringHolder.Length - 1;
            
            startAt = mainText.IndexOf(stringToAffect);
            endAt = startAt + stringToAffect.Length-1;
        }
        else
        {
            //Error case
            startAt = 0;
            endAt = stringToAffect.Length - 1;
        }
        
    }

    private bool InBetween(int checkValue, int start, int end)
    {
        return checkValue >= start && checkValue <= end;
    }



    IEnumerator AnimateVertexColors()
    {
        CheckText();
        
        // Force the text object to update right away so we can have geometry to modify right from the start.
        m_TextComponent.ForceMeshUpdate();

        TMP_TextInfo textInfo = m_TextComponent.textInfo;
        int currentCharacter = startAt;

        Color32[] newVertexColors;
        Color32 color0 = textGradient.Evaluate(0f);
        Color32 color1 = m_TextComponent.color;
        

        while (true)
        {
            int characterCount = textInfo.characterCount;

            // If No Characters then just yield and wait for some text to be added
            if (characterCount == 0)
            {
                yield return new WaitForSeconds(0.25f);
                continue;
            }

            // Get the index of the material used by the current character.
            int materialIndex = textInfo.characterInfo[currentCharacter].materialReferenceIndex;

            // Get the vertex colors of the mesh used by this text element (character or sprite).
            newVertexColors = textInfo.meshInfo[materialIndex].colors32;

            // Get the index of the first vertex used by this text element.
            int vertexIndex = textInfo.characterInfo[currentCharacter].vertexIndex;

            // Only change the vertex color if the text element is visible.
            if (textInfo.characterInfo[currentCharacter].isVisible && InBetween(currentCharacter, startAt, endAt))
            {
                var offset = currentCharacter / characterCount;
                color1 = textGradient.Evaluate((_totalTime + offset) % 1f);  
                _totalTime += Time.deltaTime;

                newVertexColors[vertexIndex + 0] = color0;
                newVertexColors[vertexIndex + 1] = color0;
                newVertexColors[vertexIndex + 2] = color1;
                newVertexColors[vertexIndex + 3] = color1;
                
                color0 = color1;

                // New function which pushes (all) updated vertex data to the appropriate meshes when using either the Mesh Renderer or CanvasRenderer.
                m_TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                // This last process could be done to only update the vertex data that has changed as opposed to all of the vertex data but it would require extra steps and knowing what type of renderer is used.
                // These extra steps would be a performance optimization but it is unlikely that such optimization will be necessary.
            }

            currentCharacter = (currentCharacter + 1) % (endAt+1);
            if (!InBetween(currentCharacter, startAt, endAt))
                currentCharacter = startAt;

            yield return new WaitForSeconds(gradientSpeed);
        }
    }
}
