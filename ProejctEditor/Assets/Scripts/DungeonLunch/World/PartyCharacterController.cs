using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyCharacterController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Slider         hpBar;
    [SerializeField] private GameObject     hpBarRoot;

    private PartyMember _member;
    private float _moveSpeed = 2f;
    private bool _isMoving = true;

    public void Init(PartyMember member, Sprite sprite)
    {
        _member = member;
        spriteRenderer.sprite = sprite;
        member.OnStatsChanged += Refresh;
        Refresh();
    }

    void Update()
    {
        if (!_isMoving || _member == null || !_member.IsAlive) return;
        if (DungeonGameManager.instance.CurrentState != DungeonState.Explore) return;
        transform.Translate(Vector2.right * _moveSpeed * Time.deltaTime);
    }

    public void SetMoving(bool moving) => _isMoving = moving;

    private void Refresh()
    {
        if (_member == null) return;
        hpBar.value = _member.hp / _member.MaxHP;
        hpBarRoot.SetActive(_member.IsAlive);
        spriteRenderer.color = _member.IsAlive ? Color.white : new Color(0.3f, 0.3f, 0.3f);
    }

    void OnDestroy()
    {
        if (_member != null) _member.OnStatsChanged -= Refresh;
    }
}
