using UnityEngine;

/// <summary>
/// 포물선 궤적으로 날아가는 포탄.
/// CannonController에서 Spawn()으로 생성.
/// </summary>
public class Projectile : MonoBehaviour
{
    public AttributeType attribute;
    public float attributeAmount = 10f;

    private Vector3 velocity;
    private float gravity = -15f;
    private float lifetime = 5f;
    private float timer;
    private bool landed;

    // 착탄 이펙트용
    private Color trailColor;

    public static Projectile Spawn(Vector3 position, Vector3 velocity, AttributeType attribute, float amount, Color color)
    {
        // 구체 생성
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = $"Projectile_{attribute}";
        go.transform.position = position;
        go.transform.localScale = Vector3.one * 0.15f;

        // 색상
        var mr = go.GetComponent<MeshRenderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        mat.SetColor("_EmissionColor", color * 2f);
        mr.material = mat;

        // 콜라이더를 트리거로
        var col = go.GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 1.5f; // 판정 약간 넓게

        // Rigidbody (트리거 감지용, 물리는 직접 처리)
        var rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        var proj = go.AddComponent<Projectile>();
        proj.velocity = velocity;
        proj.attribute = attribute;
        proj.attributeAmount = amount;
        proj.trailColor = color;

        return proj;
    }

    void Update()
    {
        if (landed) return;

        timer += Time.deltaTime;
        if (timer > lifetime)
        {
            Destroy(gameObject);
            return;
        }

        // 포물선: 중력 적용
        velocity.y += gravity * Time.deltaTime;
        Vector3 movement = velocity * Time.deltaTime;

        transform.position += movement;

        // 바닥 충돌 (y < 0)
        if (transform.position.y <= 0.1f)
        {
            OnLand(transform.position);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (landed) return;

        // 플레이어 무시
        if (other.CompareTag("Player")) return;

        // 몬스터 명중 체크 (추후 MonsterAttributeState 연동)
        // 현재는 Damageable이 있는 대상에 반응
        var damageable = other.GetComponentInParent<Damageable>();
        if (damageable != null)
        {
            OnHitMonster(damageable, other.transform.position);
            return;
        }
    }

    void OnHitMonster(Damageable target, Vector3 hitPos)
    {
        landed = true;

        // MonsterAttributeState에 속성 누적
        target.GetComponentInParent<MonsterAttributeState>()?.ApplyAttribute(attribute, attributeAmount);

        // 착탄 피드백: 스케일 펑 이펙트
        SpawnImpactEffect(hitPos);

        Destroy(gameObject, 0.05f);
    }

    void OnLand(Vector3 pos)
    {
        landed = true;

        // 바닥 착탄 이펙트
        SpawnImpactEffect(pos);

        Destroy(gameObject, 0.05f);
    }

    void SpawnImpactEffect(Vector3 pos)
    {
        // 간단한 착탄 이펙트: 크기 커졌다 사라지는 구
        var fx = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fx.name = "Impact";
        fx.transform.position = pos;
        fx.transform.localScale = Vector3.one * 0.1f;

        var fxCol = fx.GetComponent<Collider>();
        if (fxCol != null) Object.Destroy(fxCol);

        var fxMr = fx.GetComponent<MeshRenderer>();
        var fxMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        fxMat.color = trailColor;
        fxMat.SetColor("_EmissionColor", trailColor * 3f);
        fxMr.material = fxMat;

        var fxAnim = fx.AddComponent<ImpactEffect>();
        // 0.4초 후 제거
        Destroy(fx, 0.4f);
    }
}

/// <summary>
/// 착탄 이펙트: 커졌다 사라지는 애니메이션.
/// </summary>
public class ImpactEffect : MonoBehaviour
{
    private float timer;
    private const float Duration = 0.4f;

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / Duration;
        float scale = Mathf.Lerp(0.1f, 0.8f, t);
        transform.localScale = Vector3.one * scale;

        var mr = GetComponent<MeshRenderer>();
        if (mr != null)
        {
            var c = mr.material.color;
            c.a = Mathf.Lerp(1f, 0f, t);
            mr.material.color = c;
        }
    }
}
