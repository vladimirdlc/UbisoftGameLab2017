using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;

[RequireComponent(typeof(Camera))]
public class VHSPostProcessEffect : PostEffectsBase
{
    Material m;
    public Shader shader;
    public MovieTexture VHS;
    public float intensity = 1;

    float yScanline, xScanline;

    protected override void Start()
    {
        m = new Material(shader);
        m.SetTexture("_VHSTex", VHS);
        VHS.loop = true;
        VHS.Play();
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        yScanline += Time.deltaTime * 0.1f;
        xScanline -= Time.deltaTime * 0.1f;

        if (yScanline >= 1)
        {
            yScanline = Random.value;
        }
        if (xScanline <= 0 || Random.value < 0.05)
        {
            xScanline = Random.value;
        }
        m.SetFloat("_yScanline", yScanline * intensity);
        m.SetFloat("_xScanline", xScanline * intensity);
        Graphics.Blit(source, destination, m);
    }
}