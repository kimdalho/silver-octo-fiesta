using UnityEngine;

public class IndirectLight_Tube : MonoBehaviour
{
    public float length = 2f;
    public Color lightColor = new Color(1f, 0.95f, 0.8f);
    [Range(0f, 10f)] public float emission = 3f;

    void Start()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.transform.SetParent(transform);
        go.transform.localPosition = new Vector3(length / 2f, 0, 0);
        go.transform.localRotation = Quaternion.Euler(0, 0, 90f);
        go.transform.localScale = new Vector3(0.05f, length / 2f, 0.05f);

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", lightColor * emission);
        mat.color = lightColor;
        go.GetComponent<MeshRenderer>().material = mat;

        Destroy(go.GetComponent<Collider>());
    }
}
