using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class Pen : MonoBehaviour
{
    PaintingManager paintingManager;
    [SerializeField]
    bool inPainting;
    [SerializeField]
    HandCollider handCollider;
    [SerializeField]
    GameObject laserCaster;
    private Vector2 previousUVcoord = new Vector2(-100, -100);

    HandPhysics handPhysics;
    public MeshRenderer forceFeedbackVisualizer;

    public Gradient visualForceFeedbackColor;
    // Start is called before the first frame update
    private void Awake()
    {
        paintingManager = GameObject.Find("PaintingManager").GetComponent<PaintingManager>();
        handPhysics = GameObject.Find("RightHand").GetComponent<HandPhysics>();
    }
    void Start()
    {
        
    }

    private void Update()
    {
        handCollider.inPainting = inPainting;
        if (inPainting)
        {
            float offset = handPhysics.OffsetPositionNorm;
            offset *= 10;
            forceFeedbackVisualizer.material.color = visualForceFeedbackColor.Evaluate(Mathf.Min(1,offset));
        }
        else
        {
            forceFeedbackVisualizer.material.color = visualForceFeedbackColor.Evaluate(0f);
        }
    }

    public Texture2D toTexture2D(RenderTexture rTex, Texture2D tex)
    {
        Graphics.CopyTexture(rTex, tex);
        return tex;
    }

    IEnumerator DrawPoint(Renderer rend, Vector2 pixelUV, float time = 0.001f)
    {
        float offset = handPhysics.OffsetPositionNorm;
        yield return new WaitForSecondsRealtime(time);
        if (!handCollider.inPainting)
            yield break;
        Texture2D tex = rend.material.mainTexture as Texture2D;
        RenderTexture rtex = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.ARGB32);

        Graphics.Blit(tex, rtex);
        paintingManager.Draw(pixelUV, rtex, tex, offset);

        rend.material.mainTexture = toTexture2D(rtex, tex);
        RenderTexture.ReleaseTemporary(rtex);
    }
    private void DrawingProcess(RaycastHit hit)
    {


        Renderer rend = hit.transform.GetComponent<Renderer>();

        if (rend == null || rend.material == null || rend.material.mainTexture == null)
        {
            
        }
        else
        {
            Vector2 pixelUV = hit.textureCoord;
            if (previousUVcoord.x >= 0)
            {
                //if ((pixelUV - previousUVcoord).magnitude < 0.00005)
                //{
                //    return;
                //}
            }
            if (previousUVcoord.x < 0)
            {
                StartCoroutine(DrawPoint(rend, pixelUV));
            }
            else
            {
                float dis = Vector2.Distance(pixelUV, previousUVcoord);
                float lerpSize = paintingManager.GetLerpSize();

                if (dis > lerpSize)
                {

                    Vector2 dir = (pixelUV - previousUVcoord).normalized;
                    int num = (int)(dis / lerpSize);
                    float time = 0.001f;
                    for (int i = 0; i < num; i++)
                    {
                        Vector2 newPoint = previousUVcoord + dir * (i + 1) * lerpSize;
                        StartCoroutine(DrawPoint(rend, newPoint, time));
                        time += 0.00001f;
                    }
                }
                else
                {
                    StartCoroutine(DrawPoint(rend, pixelUV));
                }
            }



            previousUVcoord = pixelUV;


        }


    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (inPainting)
        {
            Ray raycast = new Ray(laserCaster.transform.position, laserCaster.transform.forward);
            RaycastHit hit;
            int layer_mask = LayerMask.GetMask("Canvas");
            bool bHit = Physics.Raycast(raycast, out hit, 100f, layer_mask) ;
            if (bHit)
            {
                if ((laserCaster.transform.position - hit.point).magnitude > 0.1f)
                {

                }
                else
                {
                    if (hit.transform.tag == "Canvas")
                    {
                        if (paintingManager.handMode)
                        {
                            DrawingProcess(hit);
                        }
                    }
                }


            }

        }
        else
        {
            previousUVcoord = new Vector2(-100, -100);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform.tag == "Canvas")
        {
            inPainting = true;
        }
        

        // low friction if touching static object, high friction if touching dynamic
        //SetPhysicMaterial(touchingDynamic ? physicMaterial_highfriction : physicMaterial_lowfriction);



    }
    private void OnTriggerExit(Collider other)
    {
        inPainting = false;
    }

}
