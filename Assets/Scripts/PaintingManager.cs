using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintingManager : MonoBehaviour
{
    public bool handMode;
    public GameObject canvas;
    public List<GameObject> layers;
    public List<Shader> brushShaders;
    public List<Texture> brushTextures;
    public List<Texture> brushBaseTextures;
    public float brushSize = 5.0f;
    public bool isErasing = false;
    public Valve.VR.InteractionSystem.HandPhysics handPhysics;
    public AnimationCurve pressureCurve;

    [SerializeField]
    private Color brushColor = Color.black;
    [SerializeField]
    private int currentBrushIdx = 0;

    private int currentLayerIdx = 0;
    private Material brushMat;

    // Start is called before the first frame update
    void Start()
    {
        SetBrushMaterial();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetBrushMaterial()
    {
        brushMat = new Material(brushShaders[currentBrushIdx]);
        brushMat.SetTexture("_BrushTex", brushTextures[currentBrushIdx]);
        brushMat.SetTexture("_BrushBaseTex", brushBaseTextures[currentBrushIdx]);
        brushMat.SetColor("_Color", brushColor);
        
    }
    public RenderTexture Draw(Vector2 uvCoordinate, RenderTexture layerTexture, Texture2D layerTexture2D,float offset)
    {
        float tempSize = brushSize;
        offset *= 10;
        tempSize = pressureCurve.Evaluate(offset) * tempSize;

        
        brushMat.SetFloat("_Size", tempSize);
        brushMat.SetVector("_UV", uvCoordinate);
        brushMat.SetTexture("_OrgTex", layerTexture2D);
        if (isErasing)
        {
            brushMat.SetInt("_AlphaBlendOp", (int)UnityEngine.Rendering.BlendOp.ReverseSubtract);
        }
        else
        {
            brushMat.SetInt("_AlphaBlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
        }
        Graphics.Blit(layerTexture, layerTexture,brushMat);
        return layerTexture;
    }

    public float GetLerpSize()
    {
        return (brushTextures[currentBrushIdx].width + brushTextures[currentBrushIdx].height) / 2.5f / brushSize * canvas.transform.localScale.x * canvas.transform.localScale.x*0.03f*0.5f;
    }
}
