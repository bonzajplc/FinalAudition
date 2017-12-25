using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DensityFieldManager : MonoBehaviour
{
    [Range(8, 256)]
    public int Resolution = 128;

    [Range(0.1f, 2)]
    public float upscaleSampleScale = 1.0f;

    public bool useFiltering = true;
    public enum UpscaleFilterType
    {
        box, tent
    };
    public UpscaleFilterType upscaleFilter;

    [HideInInspector]
    public RenderTexture densityTexture_;
    [HideInInspector]
    public RenderTexture densityTextureDownscaled_;
    [HideInInspector]
    public RenderTexture colorTexture_;

    public ComputeShader UpdateDensityCS;

    public int updateDensityFieldCSKernel_ { get; private set; }
    public int generateDensityFieldCSKernel_ { get; private set; }
    public int downscaleDensityFieldBoxCSKernel_ { get; private set; }
    public int upscaleDensityFieldBoxCSKernel_ { get; private set; }
    public int upscaleDensityFieldTentCSKernel_ { get; private set; }

    public int maxParticles = 65000;

    [HideInInspector]
    public int numPointCloudParticles_ = 0;
    public ComputeBuffer particleBuffer_; //set by density field

    [Range(1, 128)]
    public int maxDensityObjects = 16;
    [HideInInspector]
    public DensityObject[] densityObjects = null;
    [HideInInspector]
    public ComputeBuffer densityObjectBuffer;

    struct DensityObjectCompute
    {
        public int type;
        public Vector3 pos;
        public Vector3 parameters;
    }
    DensityObjectCompute[] densityObjectsComputeArray;

    private void Awake()
    {
        densityTexture_ = new RenderTexture(Resolution, Resolution, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
        densityTexture_.wrapMode = TextureWrapMode.Clamp;
        densityTexture_.enableRandomWrite = true;
        densityTexture_.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        densityTexture_.volumeDepth = Resolution;
        densityTexture_.Create();

        densityTextureDownscaled_ = new RenderTexture(Resolution/2, Resolution/2, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
        densityTextureDownscaled_.wrapMode = TextureWrapMode.Clamp;
        densityTextureDownscaled_.enableRandomWrite = true;
        densityTextureDownscaled_.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        densityTextureDownscaled_.volumeDepth = Resolution/2;
        densityTextureDownscaled_.Create();

        colorTexture_ = new RenderTexture(Resolution, Resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        colorTexture_.wrapMode = TextureWrapMode.Clamp;
        colorTexture_.enableRandomWrite = true;
        colorTexture_.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        colorTexture_.volumeDepth = Resolution;
        colorTexture_.Create();

        updateDensityFieldCSKernel_ = UpdateDensityCS.FindKernel("cs_updateDensityField");
        UpdateDensityCS.SetTexture(updateDensityFieldCSKernel_, "densityTexture_", densityTexture_);
        UpdateDensityCS.SetTexture(updateDensityFieldCSKernel_, "colorTexture_", colorTexture_);

        generateDensityFieldCSKernel_ = UpdateDensityCS.FindKernel("cs_generateDensityField");
        UpdateDensityCS.SetTexture(generateDensityFieldCSKernel_, "densityTexture_", densityTexture_);
        UpdateDensityCS.SetTexture(generateDensityFieldCSKernel_, "colorTexture_", colorTexture_);

        downscaleDensityFieldBoxCSKernel_ = UpdateDensityCS.FindKernel("cs_downscaleDensityFieldBox");
        UpdateDensityCS.SetTexture(downscaleDensityFieldBoxCSKernel_, "densityTextureReadOnly_", densityTexture_);
        UpdateDensityCS.SetTexture(downscaleDensityFieldBoxCSKernel_, "densityTextureDownscaled_", densityTextureDownscaled_);

        upscaleDensityFieldBoxCSKernel_ = UpdateDensityCS.FindKernel("cs_upscaleDensityFieldBox");
        UpdateDensityCS.SetTexture(upscaleDensityFieldBoxCSKernel_, "densityTexture_", densityTexture_);
        UpdateDensityCS.SetTexture(upscaleDensityFieldBoxCSKernel_, "densityTextureDownscaledReadOnly_", densityTextureDownscaled_);

        upscaleDensityFieldTentCSKernel_ = UpdateDensityCS.FindKernel("cs_upscaleDensityFieldTent");
        UpdateDensityCS.SetTexture(upscaleDensityFieldTentCSKernel_, "densityTexture_", densityTexture_);
        UpdateDensityCS.SetTexture(upscaleDensityFieldTentCSKernel_, "densityTextureDownscaledReadOnly_", densityTextureDownscaled_);

        particleBuffer_ = new ComputeBuffer(maxParticles, (1 + 1 + 1) * sizeof(float));  // uint, float, uint
        numPointCloudParticles_ = 0;
    }

    private void Start()
    {
        densityObjectBuffer = new ComputeBuffer(maxDensityObjects, sizeof(float) * (1 + 3 + 3), ComputeBufferType.Default);
        densityObjectsComputeArray = new DensityObjectCompute[maxDensityObjects];

        densityObjects = FindObjectsOfType<DensityObject>();
    }

    private void Update()
    {
        UpdateDensityObjects();

        UpdateDensityCS.SetFloat("gridSizeRcp_", 1.0f / (Resolution - 1.0f));
        UpdateDensityCS.Dispatch(generateDensityFieldCSKernel_, Resolution / 8, Resolution / 8, Resolution / 8);

        if( numPointCloudParticles_ != 0 )
        {
            UpdateDensityCS.SetBuffer(updateDensityFieldCSKernel_, "particles_", particleBuffer_);

            UpdateDensityCS.SetInt("resolution_", Resolution);
            UpdateDensityCS.Dispatch(updateDensityFieldCSKernel_, numPointCloudParticles_ / 64, 1, 1);
        }

        if (useFiltering)
        {
            int halfRes = Resolution / 2;
            UpdateDensityCS.SetFloat("halfGridSizeRcp_", 1.0f / (Resolution / 2 - 1.0f));

            UpdateDensityCS.Dispatch(downscaleDensityFieldBoxCSKernel_, halfRes / 8, halfRes / 8, halfRes / 8);

            UpdateDensityCS.SetFloat("upscaleSampleScale_", upscaleSampleScale);
            if (upscaleFilter == DensityFieldManager.UpscaleFilterType.box)
                UpdateDensityCS.Dispatch(upscaleDensityFieldBoxCSKernel_, Resolution / 8, Resolution / 8, Resolution / 8);
            else if (upscaleFilter == DensityFieldManager.UpscaleFilterType.tent)
                UpdateDensityCS.Dispatch(upscaleDensityFieldTentCSKernel_, Resolution / 8, Resolution / 8, Resolution / 8);
        }
    }

    public void UpdateDensityObjects()
    {
#if UNITY_EDITOR
        if (densityObjectsComputeArray == null)
            return;
#endif
        int i = 0;
        foreach( var densityObject in densityObjects )
        {
            densityObjectsComputeArray[i].type = (int)densityObject.objectType;
            densityObjectsComputeArray[i].parameters.x = densityObject.param0;
            densityObjectsComputeArray[i].parameters.y = densityObject.param1;
            densityObjectsComputeArray[i].parameters.z = densityObject.param2;
            densityObjectsComputeArray[i].pos = transform.worldToLocalMatrix.MultiplyPoint( densityObject.transform.position );
            densityObjectsComputeArray[i].pos.x += 0.5f;
            densityObjectsComputeArray[i].pos.y += 0.5f;
            densityObjectsComputeArray[i].pos.z += 0.5f;
            i++;
        }

        densityObjectBuffer.SetData(densityObjectsComputeArray);
        UpdateDensityCS.SetBuffer(generateDensityFieldCSKernel_, "densityObjects_", densityObjectBuffer);
        UpdateDensityCS.SetInt("numDensityObjects_", i);
    }

    private void OnDestroy()
    {
        if(densityObjectBuffer!=null)
            densityObjectBuffer.Release();
        if (particleBuffer_ != null)
            particleBuffer_.Release();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);

        DrawWireLines();
    }

    void DrawWireLines()
    {
        float fResX = 1.0f / (Resolution / 8.0f);
        float fResY = 1.0f / (Resolution / 8.0f);
        float fResZ = 1.0f / (Resolution / 8.0f);

        for (int i = 1; i < Resolution / 8; i++)
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(i * fResX - 0.5f, -0.5f, -0.5f)), transform.TransformPoint(new Vector3(i * fResX - 0.5f, -0.5f, 0.5f)));

        for (int i = 1; i < Resolution / 8; i++)
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(-0.5f, -0.5f, i * fResZ - 0.5f)), transform.TransformPoint(new Vector3(0.5f, -0.5f, i * fResZ - 0.5f)));

        for (int i = 1; i < Resolution / 8; i++)
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(-0.5f, i * fResY - 0.5f, 0.5f)), transform.TransformPoint(new Vector3(0.5f, i * fResY - 0.5f, 0.5f)));
    }
}
