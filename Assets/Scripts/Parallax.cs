using UnityEngine;

public class Parallax: MonoBehaviour
{
    [System.Serializable]
    public class Layer
    {
        public Transform prefab;       // The original sprite object
        public float parallaxFactor;   // How fast it moves relative to camera (X only)
    }

    public Layer[] layers;

    private Transform cam;

    private void Start()
    {
        cam = Camera.main.transform;

        foreach (var layer in layers)
        {
            if (layer.prefab == null) continue;

            // Get sprite width in world units
            SpriteRenderer sr = layer.prefab.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                Debug.LogError($"{layer.prefab.name} has no SpriteRenderer!");
                continue;
            }

            float width = sr.bounds.size.x;

            // Create two extra copies (left and right) for seamless repeat
            Transform left = Instantiate(layer.prefab, layer.prefab.position - Vector3.right * width, Quaternion.identity, layer.prefab.parent);
            Transform right = Instantiate(layer.prefab, layer.prefab.position + Vector3.right * width, Quaternion.identity, layer.prefab.parent);

            // Add ParallaxRepeater to all three
            AddRepeater(layer.prefab, width, layer.parallaxFactor);
            AddRepeater(left, width, layer.parallaxFactor);
            AddRepeater(right, width, layer.parallaxFactor);
        }
    }

    private void AddRepeater(Transform t, float width, float factor)
    {
        ParallaxRepeater repeater = t.gameObject.AddComponent<ParallaxRepeater>();
        repeater.Init(cam, width, factor);
    }
}

public class ParallaxRepeater : MonoBehaviour
{
    private Transform cam;
    private float spriteWidth;
    private float parallaxFactor;
    private Vector3 startPos;

    public void Init(Transform cam, float width, float factor)
    {
        this.cam = cam;
        this.spriteWidth = width;
        this.parallaxFactor = factor;
        this.startPos = transform.position;
    }

    private void LateUpdate()
    {
        if (cam == null) return;

        // X parallax, Y follows camera exactly
        float camX = cam.position.x * parallaxFactor;
        float camY = cam.position.y;

        transform.position = new Vector3(startPos.x + camX, camY, startPos.z);

        // Smooth repeat
        float diff = cam.position.x - transform.position.x;
        if (diff > spriteWidth) startPos.x += spriteWidth * 2f;
        else if (diff < -spriteWidth) startPos.x -= spriteWidth * 2f;
    }
}
