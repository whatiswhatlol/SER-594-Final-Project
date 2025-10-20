using UnityEngine;

public class OneShotFlash : MonoBehaviour
{
    public float duration = 0.08f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0.2f, 1, 1.6f);
    public Gradient colorOverLife;
    public float scaleMultiplier = 1f;

    SpriteRenderer sr;
    Vector3 initialScale;
    float t;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (colorOverLife == null || colorOverLife.colorKeys.Length == 0)
        {
            Gradient g = new Gradient();
            var c = new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.95f, 0.6f), 0f),
                new GradientColorKey(new Color(1f, 1f, 1f), 1f)
            };
            var a = new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            };
            g.SetKeys(c, a);
            colorOverLife = g;
        }
    }

    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        t += Time.deltaTime / duration;
        float k = Mathf.Clamp01(t);
        transform.localScale = initialScale * (scaleCurve.Evaluate(k) * scaleMultiplier);
        if (sr) sr.color = colorOverLife.Evaluate(k);
        if (t >= 1f) Destroy(gameObject);
    }
}
