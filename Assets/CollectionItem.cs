using MoreMountains.InfiniteRunnerEngine;
using UnityEngine;

public class CollectionItem : MonoBehaviour
{
    public bool coin;
    public ParticleSystem collectionParticle;
    public Material CoinYellowMat;
    public Material CoinSilverMat;
    public MeshRenderer coinRenderer;

    void OnEnable()
    {
        if (!coin) return;

        GameplayManager.instance.OnTimerActiveChanged += OnTimerChanged;

        // Apply current state immediately
        OnTimerChanged(GameplayManager.instance.timerActive);
    }

    public void destroyCoin()
    {
        GameplayManager.instance.OnTimerActiveChanged -= OnTimerChanged;
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (coin)
        {
            GameplayManager.instance.OnTimerActiveChanged -= OnTimerChanged;
        }
    }

    void OnTimerChanged(bool timerActive)
    {
        coinRenderer.sharedMaterial =
            timerActive ? CoinSilverMat : CoinYellowMat;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Update score or coins
            if (coin)
            {
                AudioManager.instance.PlayCoinCollect();
                GameplayManager.instance.AddCoin();
            }
            else
            {
                AudioManager.instance.PlayGemCollect();
                GameplayManager.instance.AddGem();
            }

            // Spawn particle effect at this item's position
            if (collectionParticle != null)
            {
                // Instantiate a new particle system at the item's position
                ParticleSystem particle = Instantiate(collectionParticle, transform.position, Quaternion.identity);
                particle.Play();

                // Optional: Destroy the particle system after it finishes
                Destroy(particle.gameObject, particle.main.duration + particle.main.startLifetime.constantMax);
            }

            if (coin)
            {
                destroyCoin();
            }
            else
            {
                GameplayManager.instance.AddTimer();
                Destroy(gameObject);
            }
        }
    }
}
