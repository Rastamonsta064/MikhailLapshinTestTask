using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private EnemiesFactory enemiesFactory;
    [SerializeField] private PlayerController player;
    [SerializeField] private int minKillsToVictoryCount;
    [SerializeField] private int maxKillsToVictoryCount;
    [SerializeField] private SpriteRenderer playerBaseSpriteRenderer;
    [SerializeField] private float playerBaseDamageEffectDuration;
    [Header("UI")]
    [SerializeField] private GameObject currentPlayerHealthPanel;
    [SerializeField] private TMP_Text currentPlayerHealthValue;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverPanelTitle;
    [SerializeField] private Button restartButton;

    private Coroutine updatePlayerTargetCoroutine;
    private int killsToVictoryCount;
    private const string victory = "Победа";
    private const string loose = "Поражение";
    private const string winSound = "Win";
    private const string looseSound = "Loose";
    private Color damageColor = Color.red;
    private Color regularColor = Color.white;


    private void Start()
    {
        AudioManager.Instance.Play("BackgroundMusic");
        StartGame();
    }

    private void OnEnable()
    {
        enemiesFactory.EnemyKilledAction += EnemyKilledActionHandler;
        enemiesFactory.EnemyReachedFinishLineAction += EnemyReachedFinishLineAction;
        player.PlayerTookDamageAction += PlayerTookDamageActionHandler;
        player.PlayerDiedAction += PlayerDiedActionHandler;
        restartButton.onClick.AddListener(RestartButtonClickedHandler);
    }

    private void OnDisable()
    {
        enemiesFactory.EnemyKilledAction -= EnemyKilledActionHandler;
        enemiesFactory.EnemyReachedFinishLineAction -= EnemyReachedFinishLineAction;
        player.PlayerTookDamageAction -= PlayerTookDamageActionHandler;
        player.PlayerDiedAction -= PlayerDiedActionHandler;
        restartButton.onClick.RemoveListener(RestartButtonClickedHandler);
    }

    public void StartGame()
    {
        killsToVictoryCount = Random.Range(minKillsToVictoryCount, maxKillsToVictoryCount);
        enemiesFactory.StartEnemiesSpawn();
        player.SetMaxHealth();
        player.EnablePlayerControls();
        player.transform.position = new Vector2(0,-3);
        currentPlayerHealthPanel.SetActive(true);
        currentPlayerHealthValue.text = player.PlayersCurrentHealth.ToString();
        gameOverPanel.SetActive(false);
        updatePlayerTargetCoroutine = StartCoroutine(UpdatePlayersTarget());
    }

    private void GameOver(bool isVictory)
    {
        if(updatePlayerTargetCoroutine != null)
        {
            StopCoroutine(updatePlayerTargetCoroutine);
        }
        player.DisablePlayerControls();
        currentPlayerHealthPanel.SetActive(false);
        enemiesFactory.StopEnemiesSpawn();
        gameOverPanel.SetActive(true);
        gameOverPanelTitle.text = isVictory ? victory : loose;
        AudioManager.Instance.Play(isVictory ? winSound : looseSound);
    }

    private void EnemyKilledActionHandler()
    {
        player.SetTarget(null);
        killsToVictoryCount--;
        if(killsToVictoryCount <= 0)
        {
            GameOver(true);
        }
    }

    private void EnemyReachedFinishLineAction()
    {
        StartCoroutine(BaseDamageEffectCoroutine());
        player.TakeDamage();
    }
    private void PlayerTookDamageActionHandler() => currentPlayerHealthValue.text = player.PlayersCurrentHealth.ToString();

    private void PlayerDiedActionHandler() => GameOver(false);

    private IEnumerator BaseDamageEffectCoroutine()
    {
        playerBaseSpriteRenderer.color = damageColor;
        yield return new WaitForSeconds(playerBaseDamageEffectDuration);
        playerBaseSpriteRenderer.color = regularColor;
    }

    private IEnumerator UpdatePlayersTarget()
    {
        Enemy enemy;
        while (true)
        {
            enemy = enemiesFactory.GetNearestEnemy(player.transform.position);
            if(enemy != null)
            {
                float distanceToTarget = Vector2.Distance(player.transform.position, enemy.transform.position);
                if (distanceToTarget <= player.PlayersShootDistance)
                {
                    player.SetTarget(enemy);
                }else
                {
                    player.SetTarget(null);
                }
            }
            yield return null;
        }
    }

    private void RestartButtonClickedHandler()
    {
        AudioManager.Instance.Play("UIButtonClick");
        StartGame();
    }
    
}
