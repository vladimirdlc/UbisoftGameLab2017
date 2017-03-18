using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class GlowComposite : MonoBehaviour
{
	[Range (0, 10)]
	public float Intensity = 2;

	private Material _compositeMat;
    public Shader glowShader;

	void OnEnable()
	{
		_compositeMat = new Material(glowShader);
    }

	void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		_compositeMat.SetFloat("_Intensity", Intensity);
        Graphics.Blit(src, dst, _compositeMat, 0);
	}
}
