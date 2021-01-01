using System.Collections.Generic;
using UnityEngine;

public class RayTracingMaster : MonoBehaviour
{
    [Header("Spheres")]
    public Vector2 SphereRadius = new Vector2(3.0f, 8.0f);
    public uint SpheresMax = 100;
    public float SpherePlacementRadius = 100.0f;
    private float _lastFieldOfView;
    private uint _currentSample = 0;

    private Camera _camera;
    public Light DirectionalLight;
    private RenderTexture _target;
    public Texture SkyboxTexture;
    private Material _addMaterial;
    public ComputeShader RayTracingShader;
    private ComputeBuffer _sphereBuffer;
    private List<Transform> _transformsToWatch = new List<Transform>();

    struct Sphere
    {
        public Vector3 position;
        public float radius;
        public Vector3 albedo, specular;
    }

    private void Awake()
    {
        _transformsToWatch.Add(transform);
        _transformsToWatch.Add(DirectionalLight.transform);
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        float tempFieldValue = _camera.fieldOfView;
        if (tempFieldValue != _lastFieldOfView)
        {
            _currentSample = 0;
            _lastFieldOfView = tempFieldValue;
        }

        foreach (Transform t in _transformsToWatch)
        {
            if (t.hasChanged)
            {
                t.hasChanged = false;
                _currentSample = 0;
            }
        }
    }

    private void OnEnable()
    {
        _currentSample = 0;
        List<Sphere> spheres = new List<Sphere>();
        for (int i = 0; i < SpheresMax; i++)
        {
            Vector2 randomPos = Random.insideUnitSphere * SpherePlacementRadius;
            Color color = Random.ColorHSV();

            Sphere sphere = new Sphere();
            sphere.position = new Vector3(randomPos.x * Random.Range(1, 2), sphere.radius * Random.Range(1, 10), randomPos.y * Random.Range(1, 2));
            sphere.radius = (SphereRadius.x * Random.Range(1, 5)) + (Random.value * (SphereRadius.y - SphereRadius.x));
            foreach (Sphere other in spheres)
            {
                float minDist = sphere.radius + other.radius;
                if (Vector3.SqrMagnitude(sphere.position - other.position) < (minDist * minDist))
                {
                    goto SkipSphere;
                }
            }

            bool metal = Random.value < (0.5f);
            if (metal)
            {
                sphere.albedo = Vector4.zero;
                sphere.specular = new Vector4(color.r, color.g, color.b);
            }
            else
            {
                sphere.albedo = new Vector4(color.r, color.g, color.b);
                sphere.specular = new Vector4(0.04f, 0.04f, 0.04f);
            }

            spheres.Add(sphere);
        SkipSphere:
            continue;
        }

        if (_sphereBuffer != null)
        {
            _sphereBuffer.Release();
        }
        int sCount = spheres.Count;
        if (sCount > 0)
        {
            _sphereBuffer = new ComputeBuffer(spheres.Count, 40);
            _sphereBuffer.SetData(spheres);
        }
    }

    private void OnDisable()
    {
        if (_sphereBuffer != null)
        {
            _sphereBuffer.Release();
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        SetShaderParameters();
        Render(destination);
    }

    private void SetShaderParameters()
    {
        RayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));
        RayTracingShader.SetVector("_DirectionalLight", new Vector4(DirectionalLight.transform.forward.x, DirectionalLight.transform.forward.y, DirectionalLight.transform.forward.z, DirectionalLight.intensity));
        RayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
        RayTracingShader.SetTexture(0, "_SkyboxTexture", SkyboxTexture);
        if (_sphereBuffer != null)
        {
            RayTracingShader.SetBuffer(0, "_Spheres", _sphereBuffer);
        }
    }

    private void Render(RenderTexture destination)
    {
        if (_target == null || _target.width != Screen.width || _target.height != Screen.height)
        {
            if (_target != null)
            {
                _target.Release();
            }
            _currentSample = 0;
            _target = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _target.enableRandomWrite = true;
            _target.Create();
        }

        RayTracingShader.Dispatch(0, Mathf.CeilToInt(Screen.width / 8.0f), Mathf.CeilToInt(Screen.height / 8.0f), 1);
        if (_addMaterial == null)
        {
            _addMaterial = new Material(Shader.Find("Hidden/AddShader"));
        }
        RayTracingShader.SetTexture(0, "Result", _target);

        _addMaterial.SetFloat("_Sample", _currentSample);
        _currentSample++;
        Graphics.Blit(_target, destination, _addMaterial);
    }
}
