using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public PlayerInputActions PlayerInputActions;
    public UnityAction PlayerTookDamageAction;
    public UnityAction PlayerDiedAction;
    public int PlayersCurrentHealth => currentHealth;
    public float PlayersShootDistance => shootDistance;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Vector2 minBoundary;
    [SerializeField] private Vector2 maxBoundary;
    [SerializeField] private int maxHealth;
    [SerializeField] private float shootDistance = 2f;
    [SerializeField] private float shootTimeout = 1f;
    [SerializeField] private int shootDamage = 1;
    [SerializeField] private TrailRenderer bulletTrailRanderer;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform weaponBurrel;
    [SerializeField] private float stepInterval = 0.5f;

    private InputAction movement;
    private Rigidbody2D rb;
    private int currentHealth;
    private Enemy target;
    private Coroutine shootCoroutine;
    private bool canShoot = true;
    private bool isMoving;
    private const string leftStepSound = "LeftStep";
    private const string rightStepSound = "RightStep";
    private float stepTimer;
    private bool isLeftStep;


    private void Awake()
    {
        PlayerInputActions = new PlayerInputActions();
        movement = PlayerInputActions.Player.Move;
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        MovingHandler();
        StepSoundHandler();
    }
    private void MovingHandler()
    {
        Vector2 moveInput = movement.ReadValue<Vector2>();
        AnimationHandler(moveInput);
        Vector2 moveVelocity = moveInput * moveSpeed;
        Vector2 newPosition = rb.position + moveVelocity * Time.fixedDeltaTime;
        newPosition.x = Mathf.Clamp(newPosition.x, minBoundary.x, maxBoundary.x);
        newPosition.y = Mathf.Clamp(newPosition.y, minBoundary.y, maxBoundary.y);
        rb.MovePosition(newPosition);
    }

    private void AnimationHandler(Vector2 moveInput)
    {
        isMoving = moveInput != Vector2.zero;
        if (isMoving)
        {
            animator.SetFloat("X", moveInput.x);
            animator.SetFloat("Y", moveInput.y);
            spriteRenderer.flipX = moveInput.x > 0;
        }
        animator.SetBool("isMoving", isMoving);

    }

    private IEnumerator ShootCoroutine()
    {
        while (true)
        {
            if (target != null)
            {
                ShootToTarget();
            }
            yield return null;
        }   
    }

    private void ShootToTarget()
    {
        if(canShoot && target != null)
        {
            var tracer = Instantiate(bulletTrailRanderer, weaponBurrel.transform.position, Quaternion.identity);
            tracer.AddPosition(target.transform.position);
            target.TakeDamage(shootDamage);
            animator.SetTrigger("shoot");
            AudioManager.Instance.Play("Shoot");
            StartCoroutine(ShootTimeoutCoroutine());
        }
    }

    private IEnumerator ShootTimeoutCoroutine()
    {
        canShoot = false;
        yield return new WaitForSeconds(shootTimeout);
        canShoot = true;
    }

    private void PlayStepSound()
    {
        string stepSound = isLeftStep ? leftStepSound : rightStepSound;
        AudioManager.Instance.Play(stepSound);
        isLeftStep = !isLeftStep;
    }

    private void StepSoundHandler()
    {
        if (isMoving)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval)
            {
                PlayStepSound();
                stepTimer = 0.0f;
            }
        }
    }

    public void EnablePlayerControls() => movement.Enable();

    public void DisablePlayerControls() => movement.Disable();

    public void SetMaxHealth()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage()
    {
        currentHealth--;
        if(currentHealth <= 0)
        {
            StopAllCoroutines();
            PlayerDiedAction?.Invoke();
        } else
        {
            PlayerTookDamageAction?.Invoke();
        }
    }

    public void SetTarget(Enemy target)
    {
        this.target = target;
        if (target != null)
        {
            shootCoroutine = StartCoroutine(ShootCoroutine());
        }
        else
        {
            if (shootCoroutine != null)
            {
                StopCoroutine(shootCoroutine);
            }

        }
    }



}
