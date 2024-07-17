using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    public UnityAction<Enemy> EnemyKilledAction;
    public UnityAction<Enemy> EnemyReachedFinishLineAction;

    private float speed;
    [SerializeField] private int maxHealth;
    private int currenthealth;
    private Rigidbody2D rb;

    private void Awake() => rb = GetComponent<Rigidbody2D>();

    private void OnDisable() => rb.velocity = Vector2.zero;

    public void Init(float speed)
    {
        this.speed = speed;
        currenthealth = maxHealth;
        rb.velocity = new Vector2(0, -speed);
    }

    public void TakeDamage(int damage)
    {
        currenthealth -= damage;
        if(currenthealth <= 0)
        {
            EnemyKilledAction?.Invoke(this);
        }
    }

    public void ReachFinishLine() => EnemyReachedFinishLineAction?.Invoke(this);

}
