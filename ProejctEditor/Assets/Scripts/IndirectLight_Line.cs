using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class IndirectLight_Line : MonoBehaviour
{
    public float length = 2f;
    public Color lightColor = new Color(1f, 0.95f, 0.8f);
    [Range(0f, 10f)] public float emission = 3f;

    void Start()
    {
        var lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, Vector3.zero);
        lr.SetPosition(1, new Vector3(length, 0, 0));
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", lightColor * emission);
        mat.color = lightColor;
        lr.material = mat;
    }
}
