using UnityEngine;
using TMPro;

public class playerHealthScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private float maxHP;
    [SerializeField] private float bulletDamage;
    [SerializeField] private float healthPackHeal;
    [SerializeField] private float passiveHealTimer; // in seconds
    [SerializeField] private float healPerInterval; 
    [SerializeField] private float healingInterval; // in seconds

    private bool inCombat;
    private float timeSinceLastHit = 0f;
    private float timeBetweenHeals = 0f;
    public float actualHP;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        actualHP = maxHP;
        healthText.text = "HP: " + actualHP;
        inCombat = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (actualHP <= 0)
        {
            Destroy(gameObject);
        }
        if (inCombat)
        {
            CombatTimer();
        }
        else
        {
            if (actualHP < maxHP)
            {
                PassiveHealing();
            }
        }
    }

    private void CombatTimer()
    {
        // If you spent enough time, set the flag
        if (timeSinceLastHit > passiveHealTimer)
        {
            inCombat = false;
        }
        // Otherwise, keep adding time
        else
        {
            timeSinceLastHit += Time.deltaTime;
        }
    }

    private void PassiveHealing()
    {
        // If you spent enough time, set the flag
        if (timeBetweenHeals > healingInterval)
        {
            actualHP += healPerInterval;
            if (actualHP > maxHP)
            {
                actualHP = maxHP;
            }
            healthText.text = "HP: " + actualHP;
            timeBetweenHeals = 0f;
        }
        // Otherwise, keep adding time
        else
        {
            timeBetweenHeals += Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Enemy Bullet")
        {
            Destroy(collision.gameObject);
            actualHP -= bulletDamage;
            healthText.text = "HP: " + actualHP;
            inCombat = true;
            timeSinceLastHit = 0f;
        }
        if (collision.gameObject.tag == "Health Item")
        {
            Destroy(collision.gameObject);
            actualHP += healthPackHeal;
            if (actualHP > maxHP)
            {
                actualHP = maxHP;
            }
            healthText.text = "HP: " + actualHP;
        }
    }
}
