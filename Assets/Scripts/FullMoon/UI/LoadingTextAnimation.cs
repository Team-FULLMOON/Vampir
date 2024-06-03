using TMPro;
using UnityEngine;

namespace FullMoon.UI
{
    public class LoadingTextAnimation : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private float waveSpeed = 2f;
        [SerializeField] private float waveHeight = 5f;
        [SerializeField] private float waveDifference = 0.5f;

        private void Update()
        {
            if (loadingText != null)
            {
                loadingText.ForceMeshUpdate();
                TMP_TextInfo textInfo = loadingText.textInfo;

                AnimateWave(textInfo);
                UpdateGeometry(textInfo);
            }
        }

        private void UpdateGeometry(TMP_TextInfo textInfo)
        {
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
                meshInfo.mesh.vertices = meshInfo.vertices;
                loadingText.UpdateGeometry(meshInfo.mesh, i);
            }
        }

        private void AnimateWave(TMP_TextInfo textInfo)
        {
            for (int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                if (!charInfo.isVisible)
                    continue;

                var vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                Vector3 charMidBaseline = (vertices[charInfo.vertexIndex + 0] + vertices[charInfo.vertexIndex + 2]) / 2;

                for (int j = 0; j < 4; j++)
                {
                    Vector3 offset = vertices[charInfo.vertexIndex + j] - charMidBaseline;
                    offset.y += Mathf.Sin(Time.time * waveSpeed + charMidBaseline.x * waveDifference) * waveHeight;
                    vertices[charInfo.vertexIndex + j] = charMidBaseline + offset;
                }
            }
        }
    }
}