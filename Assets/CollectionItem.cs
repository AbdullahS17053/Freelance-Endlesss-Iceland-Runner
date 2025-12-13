using UnityEngine;

public class CollectionItem : MonoBehaviour
{
    public bool coin;
    public ParticleSystem collectionParticle;

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

            // Disable or destroy the item
            gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }
}
