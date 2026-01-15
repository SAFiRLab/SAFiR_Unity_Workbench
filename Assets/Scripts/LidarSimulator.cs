using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class LidarSimulator : MonoBehaviour
{
    public int horizontalResolution = 1800;
    public int verticalBeams = 16;
    public float verticalMinAngle = -15f;
    public float verticalMaxAngle = 15f;
    public float range = 100f;
    public float rotationFrequency = 10f;
    public LayerMask lidarLayer;

    [Header("Noise Parameters")]
    public float distanceJitterStdDev = 0.02f;  // meters
    public float angularJitterStdDev = 0.2f;    // degrees
    [Range(0f, 1f)] public float dropoutProbability = 0.01f;
    [Range(0f, 1f)] public float ghostProbability = 0.005f;

    private float horizontalAngle = 0f;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;
    private int maxParticles;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();

        var main = ps.main;
        maxParticles = horizontalResolution * verticalBeams;
        main.maxParticles = maxParticles;

        particles = new ParticleSystem.Particle[maxParticles];
        ps.Emit(maxParticles);
        ps.GetParticles(particles);
    }

    void Update()
    {
        int i = 0;
        float stepAngle = 360f * rotationFrequency * Time.deltaTime;

        for (int h = 0; h < horizontalResolution; h++)
        {
            float angleH = horizontalAngle + (h * (360f / horizontalResolution));
            for (int v = 0; v < verticalBeams; v++)
            {
                // Apply angular jitter
                float jitteredAngleH = angleH + RandomGaussian() * angularJitterStdDev;
                float jitteredAngleV = Mathf.Lerp(verticalMinAngle, verticalMaxAngle, v / (float)(verticalBeams - 1)) + RandomGaussian() * angularJitterStdDev;

                Quaternion rot = Quaternion.Euler(jitteredAngleV, jitteredAngleH, 0);
                Vector3 dir = rot * Vector3.forward;

                Vector3 origin = transform.position;

                // Simulate dropout
                if (Random.value < dropoutProbability)
                {
                    particles[i].startSize = 0f; // hide particle
                    i++;
                    continue;
                }

                if (Physics.Raycast(origin, dir, out RaycastHit hit, range, lidarLayer))
                {
                    float noisyDistance = Vector3.Distance(origin, hit.point) + RandomGaussian() * distanceJitterStdDev;

                    if (noisyDistance > 2.5f && noisyDistance <= range)
                    {
                        Vector3 noisyPoint = origin + dir.normalized * noisyDistance;
                        particles[i].position = noisyPoint;
                        particles[i].startColor = Color.red;
                        particles[i].startSize = 0.05f;
                    }
                    else
                    {
                        particles[i].startSize = 0f; // hide if too close
                    }
                }
                else if (Random.value < ghostProbability)
                {
                    // Simulate ghost return
                    Vector3 ghostPoint = origin + dir * Random.Range(2f, range);
                    particles[i].position = ghostPoint;
                    particles[i].startColor = new Color(1f, 1f, 0f, 0.3f); // faded yellow
                    particles[i].startSize = 0.03f;
                }
                else
                {
                    particles[i].startSize = 0f;
                }

                i++;
            }
        }

        horizontalAngle += stepAngle;
        if (horizontalAngle >= 360f)
            horizontalAngle -= 360f;

        ps.SetParticles(particles, particles.Length);
    }

    // Gaussian noise using Box-Muller transform
    float RandomGaussian()
    {
        float u1 = Random.value;
        float u2 = Random.value;
        return Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * u2);
    }
}
