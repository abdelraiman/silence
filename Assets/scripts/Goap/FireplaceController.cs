using UnityEngine;

public class FireplaceController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GoapAgent survivorRef; // Reference to the Survivor

    [Header("Fire Particle Systems")]
    [SerializeField] private ParticleSystem lowFireParticles;
    [SerializeField] private ParticleSystem mediumFireParticles;
    [SerializeField] private ParticleSystem highFireParticles;

    [Header("Fire Light Settings")]
    [SerializeField] private Light fireLight;
    [SerializeField] private float lowLightIntensity = 1f;
    [SerializeField] private float mediumLightIntensity = 3f;
    [SerializeField] private float highLightIntensity = 5f;
    [SerializeField] private bool flickerEnabled = true;
    [SerializeField] private float flickerAmount = 0.2f;

    [Header("Fire Sound")]
    [SerializeField] private AudioSource fireSound;

    private int lastWoodCount = -1;
    private float targetLightIntensity;

    private void Update()
    {
        if (survivorRef == null) return;

        int currentWood = survivorRef.fireplaceWood;

        // Only update if wood count changed
        if (currentWood != lastWoodCount)
        {
            UpdateFireplaceVisuals(currentWood);
            lastWoodCount = currentWood;
        }

        // Handle light flicker
        if (fireLight != null && flickerEnabled && targetLightIntensity > 0f)
        {
            fireLight.intensity = Mathf.Lerp(fireLight.intensity, targetLightIntensity, Time.deltaTime * 5f)
                                  + Random.Range(-flickerAmount, flickerAmount);
        }
    }

    private void UpdateFireplaceVisuals(int woodCount)
    {
        // Stop all particle systems first
        lowFireParticles.Stop();
        mediumFireParticles.Stop();
        highFireParticles.Stop();

        // Reset light and sound
        if (fireLight != null) fireLight.intensity = 0f;
        targetLightIntensity = 0f;
        if (fireSound != null) fireSound.volume = 0f;

        if (woodCount <= 0) return;

        // Low fire
        if (woodCount == 1)
        {
            lowFireParticles.Play();
            targetLightIntensity = lowLightIntensity;
            if (fireSound != null) fireSound.volume = 0.3f;
        }
        // Medium fire
        else if (woodCount == 2)
        {
            mediumFireParticles.Play();
            targetLightIntensity = mediumLightIntensity;
            if (fireSound != null) fireSound.volume = 0.6f;
        }
        // High fire
        else // 3+
        {
            highFireParticles.Play();
            targetLightIntensity = highLightIntensity;
            if (fireSound != null) fireSound.volume = 1f;
        }

        // Apply initial light intensity
        if (fireLight != null) fireLight.intensity = targetLightIntensity;
    }
}
