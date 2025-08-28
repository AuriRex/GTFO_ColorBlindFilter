using UnityEngine;

namespace ColorBlindFilter;

public class ApplyShader : MonoBehaviour
{
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, ColorBlindnessValues.Material);
    }
}