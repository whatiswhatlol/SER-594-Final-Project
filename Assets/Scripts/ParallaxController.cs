// ParallaxController.cs
// Complex, N-layer parallax with edit-mode preview, depth scaling, damping, pixel-snap, and optional infinite scrolling.
//
// Usage:
// 1) Add this to an empty GameObject (e.g., "ParallaxRoot").
// 2) Assign your Camera (or tick "Use Main Camera").
// 3) Add Layers (or click "Auto-Fill From Children" in the custom inspector).
// 4) Optionally enable Infinite Scroll per axis; segment size can be taken from renderer bounds.
// 5) Use the Editor "Preview Drag" to fine-tune without hitting Play.

using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[DisallowMultipleComponent]
public class ParallaxController: MonoBehaviour
{
    [Header("Camera")]
    public bool useMainCamera = true;
    public Camera cam;

    [Header("Global Parallax")]
    [Tooltip("Scales movement applied to ALL layers before per-layer overrides.")]
    public Vector2 globalMultiplier = new Vector2(0.5f, 0.5f);

    [Header("Depth Scaling (auto compute per layer)")]
    public bool autoDepthScaling = true;
    [Tooltip("Strength of depth falloff (1 = linear-ish). Higher = stronger attenuation with depth.")]
    public float depthStrength = 1f;
    [Tooltip("Offset added to (layerZ - cameraZ) before evaluating depth factor.")]
    public float depthOffset = 0f;
    [Tooltip("Clamp of computed depth factor.")]
    public Vector2 depthClamp = new Vector2(0.0f, 2.0f);

    [Header("Dynamics")]
    public bool damping = true;
    [Range(0.0f, 1.0f)]
    public float dampingTime = 0.1f;

    [Tooltip("Run parallax updates while editing in the Scene view.")]
    public bool runInEditMode = true;

    [Tooltip("If enabled, camera FOV/Size changes will also influence parallax slightly.")]
    public bool affectZoom = false;
    [Range(0f, 2f)]
    public float zoomInfluence = 0.2f;

    [Serializable]
    public class Layer
    {
        [Header("Target")]
        public Transform target;

        [Header("Multipliers")]
        [Tooltip("If false, effective multiplier = global * (depthFactor if autoDepthScaling).")]
        public bool overrideMultiplier = false;
        public Vector2 multiplier = Vector2.one;

        [Header("Axis Locks")]
        public bool lockX = false;
        public bool lockY = false;

        [Header("Infinite Scroll")]
        public bool infiniteScrollX = false;
        public bool infiniteScrollY = false;
        [Tooltip("Estimate from Renderer bounds if available.")]
        public bool useRendererBounds = true;
        public float segmentSizeX = 10f;
        public float segmentSizeY = 10f;

        [Header("Advanced")]
        [Tooltip("Attempt to offset material UV instead of moving transform (SpriteRenderer or material with _MainTex).")]
        public bool useMaterialUV = false;
        public string textureSTProperty = "_MainTex_ST"; // common for sprites in SRP
        [Tooltip("Snap to pixel grid to avoid sub-pixel shimmer.")]
        public bool pixelSnap = false;
        public float pixelsPerUnit = 100f;

        [HideInInspector] public Vector3 startPos;
        [HideInInspector] public Vector2 computedEffective; // cached per-frame
        [HideInInspector] public float depthFactor = 1f;

        // runtime:
        [NonSerialized] public Vector3 _vel; // SmoothDamp velocity
        [NonSerialized] public Vector3 _uvStartST; // store original ST for UV mode
        [NonSerialized] public Vector2 _accumUV;   // accumulated uv offset
        [NonSerialized] public Renderer _renderer;
        [NonSerialized] public MaterialPropertyBlock _mpb;
        [NonSerialized] public int _stID;
    }

    [SerializeField]
    public List<Layer> layers = new List<Layer>();

    // cached:
    Vector3 _startCamPos;
    Vector3 _lastCamPos;

    float _startOrthoSize;
    float _startFov;

    // Editor preview (non-serialized on purpose; inspector drives it)
#if UNITY_EDITOR
    [NonSerialized] public Vector2 editorPreviewOffset = Vector2.zero;
    [NonSerialized] public bool editorPreviewEnabled = false;
#endif

    // ------------ Unity lifecycle ------------
    void Reset()
    {
        useMainCamera = true;
        globalMultiplier = new Vector2(0.5f, 0.5f);
        damping = true;
        dampingTime = 0.1f;
        autoDepthScaling = true;
        depthStrength = 1f;
        depthOffset = 0f;
        depthClamp = new Vector2(0f, 2f);
        affectZoom = false;
        runInEditMode = true;
        CaptureStarts();
    }

    void OnEnable()
    {
        EnsureCamera();
        CacheCameraStarts();
        EnsureLayerCaches();
        CaptureStarts();
#if UNITY_EDITOR
        EditorApplication.update -= EditorTick;
        EditorApplication.update += EditorTick;
#endif
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.update -= EditorTick;
#endif
    }

    void OnValidate()
    {
        depthClamp.x = Mathf.Min(depthClamp.x, depthClamp.y);
        dampingTime = Mathf.Max(0f, dampingTime);
        zoomInfluence = Mathf.Max(0f, zoomInfluence);
        // keep caches valid in edit mode
        EnsureCamera();
        EnsureLayerCaches();
    }

    void LateUpdate()
    {
        if (!Application.isPlaying)
        {
            if (!runInEditMode) return;
            // In edit mode, LateUpdate runs only when scene changes; we also tick in EditorTick to be responsive.
        }

        UpdateParallax(usePreview: false);
    }

#if UNITY_EDITOR
    // extra editor tick for smooth preview and edit-mode interaction
    void EditorTick()
    {
        if (Application.isPlaying) return;
        if (!runInEditMode) return;
        if (this == null) return; // object destroyed

        UpdateParallax(usePreview: editorPreviewEnabled);
    }
#endif

    // ------------ Core logic ------------
    void EnsureCamera()
    {
        if (useMainCamera || cam == null)
            cam = Camera.main;
    }

    void CacheCameraStarts()
    {
        if (cam != null)
        {
            _startCamPos = cam.transform.position;
            _lastCamPos = _startCamPos;
            _startOrthoSize = cam.orthographic ? cam.orthographicSize : 0f;
            _startFov = cam.orthographic ? 0f : cam.fieldOfView;
        }
    }

    void EnsureLayerCaches()
    {
        foreach (var L in layers)
        {
            if (L == null || L.target == null) continue;

            if (L.useMaterialUV)
            {
                if (L._renderer == null) L._renderer = L.target.GetComponent<Renderer>();
                if (L._renderer != null)
                {
                    if (L._mpb == null) L._mpb = new MaterialPropertyBlock();
                    L._stID = Shader.PropertyToID(L.textureSTProperty);
                    // capture initial ST if available
                    L._renderer.GetPropertyBlock(L._mpb);
                    // _MainTex_ST is a Vector4 (x scale, y scale, x offset, y offset)
                    var st = L._mpb.GetVector(L._stID);
                    L._uvStartST = st; // x,y = scale; z,w = offset
                }
            }
        }
    }

    public void CaptureStarts()
    {
        EnsureCamera();
        foreach (var L in layers)
        {
            if (L == null || L.target == null) continue;
            L.startPos = L.target.position;

            if (L.useRendererBounds && L._renderer == null)
                L._renderer = L.target.GetComponent<Renderer>();

            if (L.useRendererBounds && L._renderer != null)
            {
                var b = L._renderer.bounds.size;
                if (L.infiniteScrollX && b.x > 0.01f) L.segmentSizeX = b.x;
                if (L.infiniteScrollY && b.y > 0.01f) L.segmentSizeY = b.y;
            }
        }
        CacheCameraStarts();
    }

    Vector2 EffectiveMultiplier(Layer L)
    {
        if (L.overrideMultiplier) return L.multiplier;

        float depth = 0f;
        if (autoDepthScaling && cam != null)
        {
            // Positive depth means layer is farther away from camera (in front of camera if using -Z forward).
            float raw = (L.target.position.z - cam.transform.position.z) + depthOffset;

            // Map depth to a factor: closer => factor ~ 1+; farther => factor -> smaller
            // Use a smooth function that decreases with |depth|: 1 / (1 + strength*|depth|)
            float f = 1f / (1f + Mathf.Abs(raw) * Mathf.Max(0.0001f, depthStrength));
            f = Mathf.Clamp(f, depthClamp.x, depthClamp.y);
            L.depthFactor = f;
        }
        else
        {
            L.depthFactor = 1f;
        }

        return globalMultiplier * L.depthFactor;
    }

    float GetZoomFactor()
    {
        if (cam == null || !affectZoom || zoomInfluence <= 0f) return 0f;

        if (cam.orthographic)
        {
            float ratio = (cam.orthographicSize - _startOrthoSize) / Mathf.Max(0.0001f, _startOrthoSize);
            return ratio * zoomInfluence;
        }
        else
        {
            float ratio = (cam.fieldOfView - _startFov) / Mathf.Max(0.0001f, _startFov);
            return ratio * zoomInfluence;
        }
    }

    void UpdateParallax(bool usePreview)
    {
        if (cam == null) { EnsureCamera(); if (cam == null) return; }

        // Camera position (with editor preview offset if enabled)
        Vector3 camPos = cam.transform.position;
#if UNITY_EDITOR
        if (usePreview)
            camPos += new Vector3(editorPreviewOffset.x, editorPreviewOffset.y, 0f);
#endif

        // Only the delta since the last frame (not from _startCamPos!)
        Vector3 camDelta = camPos - _lastCamPos;

        // dt for smooth damping in edit vs play
        float dt = Application.isPlaying ? Time.deltaTime : (1f / 60f);

        foreach (var L in layers)
        {
            if (L == null || L.target == null) continue;

            // Compute effective multiplier (depth scaling + global)
            L.computedEffective = EffectiveMultiplier(L);

            // Incremental offset for this frame
            Vector3 move = new Vector3(
                camDelta.x * L.computedEffective.x,
                camDelta.y * L.computedEffective.y,
                0f);

            // Target desired position = current + move
            Vector3 desired = L.target.position + move;

            // Axis locks
            if (L.lockX) desired.x = L.target.position.x;
            if (L.lockY) desired.y = L.target.position.y;

            // Smooth damping or direct set
            Vector3 next;
            if (damping)
            {
                next = Vector3.SmoothDamp(L.target.position, desired,
                    ref L._vel, dampingTime, Mathf.Infinity, dt);
            }
            else
            {
                next = desired;
            }

            // Pixel snap
            if (L.pixelSnap && L.pixelsPerUnit > 0f)
                next = PixelSnap(next, L.pixelsPerUnit);

            // Apply
            L.target.position = next;

            // Infinite scrolling
            if (L.infiniteScrollX) InfiniteShiftX(L, camPos.x);
            if (L.infiniteScrollY) InfiniteShiftY(L, camPos.y);
        }

        // Update last camera position for next frame
        _lastCamPos = camPos;
    }

    static Vector3 PixelSnap(Vector3 worldPos, float ppu)
    {
        worldPos.x = Mathf.Round(worldPos.x * ppu) / ppu;
        worldPos.y = Mathf.Round(worldPos.y * ppu) / ppu;
        return worldPos;
    }

    void InfiniteShiftX(Layer L, float camX)
    {
        float seg = Mathf.Max(0.0001f, L.useRendererBounds && L._renderer != null ? L._renderer.bounds.size.x : L.segmentSizeX);
        float half = seg * 0.5f;

        // Shift the object by full segment lengths so that it always stays within ~half segment of the camera
        while (camX - L.target.position.x > half) L.target.position += new Vector3(seg, 0f, 0f);
        while (L.target.position.x - camX > half) L.target.position -= new Vector3(seg, 0f, 0f);
    }

    void InfiniteShiftY(Layer L, float camY)
    {
        float seg = Mathf.Max(0.0001f, L.useRendererBounds && L._renderer != null ? L._renderer.bounds.size.y : L.segmentSizeY);
        float half = seg * 0.5f;

        while (camY - L.target.position.y > half) L.target.position += new Vector3(0f, seg, 0f);
        while (L.target.position.y - camY > half) L.target.position -= new Vector3(0f, seg, 0f);
    }

#if UNITY_EDITOR
    // ------------- Editor helpers -------------
    public void EditorSetPreview(Vector2 offset, bool enabled)
    {
        editorPreviewOffset = offset;
        editorPreviewEnabled = enabled;
        SceneView.RepaintAll();
    }

    public void AutoFillFromChildren(bool includeSelf = false)
    {
        var list = new List<Layer>();
        foreach (Transform t in transform)
        {
            if (t == null) continue;
            list.Add(new Layer
            {
                target = t,
                multiplier = Vector2.one,
                overrideMultiplier = false,
                segmentSizeX = 10f,
                segmentSizeY = 10f
            });
        }
        if (includeSelf)
        {
            list.Add(new Layer { target = transform, overrideMultiplier = true, multiplier = Vector2.zero });
        }
        layers = list;
        EnsureLayerCaches();
        CaptureStarts();
        EditorUtility.SetDirty(this);
    }

    public void BakeMultipliersFromZ(float scale = 1f)
    {
        EnsureCamera();
        if (cam == null) return;
        foreach (var L in layers)
        {
            if (L == null || L.target == null) continue;
            float raw = Mathf.Abs((L.target.position.z - cam.transform.position.z) + depthOffset);
            float f = 1f / (1f + raw * Mathf.Max(0.0001f, depthStrength));
            f = Mathf.Clamp(f, depthClamp.x, depthClamp.y);
            L.overrideMultiplier = true;
            L.multiplier = globalMultiplier * f * scale;
        }
        EditorUtility.SetDirty(this);
    }

    void OnDrawGizmosSelected()
    {
        if (cam == null) return;
        Gizmos.color = new Color(0.1f, 0.8f, 1f, 0.45f);
        Gizmos.DrawWireSphere(_startCamPos, 0.25f);
        foreach (var L in layers)
        {
            if (L == null || L.target == null) continue;
            Gizmos.color = new Color(1f - L.depthFactor, 0.3f, L.depthFactor, 0.65f);
            Gizmos.DrawLine(cam.transform.position, L.target.position);
            if (L.infiniteScrollX || L.infiniteScrollY)
            {
                Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.5f);
                var pos = L.target.position;
                var size = new Vector3(L.segmentSizeX, L.segmentSizeY, 0.01f);
                Gizmos.DrawWireCube(pos, new Vector3(Mathf.Max(0.1f, size.x), Mathf.Max(0.1f, size.y), size.z));
            }
        }
    }
#endif
}
