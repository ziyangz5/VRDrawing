using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LaserPointer : MonoBehaviour
{

    public bool active = true;
    public Color color;
    public float thickness = 0.002f;
    public Color activeColor = Color.green;
    public Color clickColor = Color.green;
    public GameObject holder;
    public GameObject pointer;
    public LineRenderer laserLine;
    bool isActive = false;
    public bool addRigidBody = false;
    public Transform reference;
    public Material LaserMaterial;
    public GameObject laserCaster;
    public Transform handTransform;
    public PaintingManager paintingManager;

    
    

    Transform previousContact = null;
    private Vector2 previousUVcoord = new Vector2(-100,-100);

    private void Start()
    {
        holder = new GameObject();
        holder.transform.parent = this.transform;
        holder.transform.localPosition = Vector3.zero;
        holder.transform.localRotation = Quaternion.identity;
        LaserMaterial.SetColor("_Color", color);
        
    }

    public Texture2D toTexture2D(RenderTexture rTex, Texture2D tex)
    {
        Graphics.CopyTexture(rTex,tex);
        return tex;
    }

    IEnumerator DrawPoint(Renderer rend,Vector2 pixelUV,float time = 0.001f)
    {
        yield return new WaitForSecondsRealtime(time);
        Texture2D tex = rend.material.mainTexture as Texture2D;
        RenderTexture rtex = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.ARGB32);
        
        Graphics.Blit(tex, rtex);
        paintingManager.Draw(pixelUV, rtex, tex,1);

        rend.material.mainTexture = toTexture2D(rtex, tex);
        RenderTexture.ReleaseTemporary(rtex);
    }
    private void DrawingProcess(RaycastHit hit)
    {
        laserLine.material.color = activeColor;
        if (SteamVR_Actions.default_InteractUI.GetState(SteamVR_Input_Sources.RightHand))
        {
            laserLine.material.color = clickColor;
            
            Renderer rend = hit.transform.GetComponent<Renderer>();

            if (rend == null || rend.material == null || rend.material.mainTexture == null)
            {
            }
            else
            {
                Vector2 pixelUV = hit.textureCoord;
                if (previousUVcoord.x >= 0)
                {
                    if ((pixelUV - previousUVcoord).magnitude < 0.005)
                    {
                        return;
                    }
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
                }



                previousUVcoord = pixelUV;


            }

        }
        else
        {
            previousUVcoord = new Vector2(-100, -100);
        }
    }

    private void FixedUpdate()
    {
        if (!isActive)
        {
            isActive = true;
            this.transform.GetChild(0).gameObject.SetActive(true);
        }
        float default_dist = 100f;

        Ray raycast = new Ray(laserCaster.transform.position, laserCaster.transform.forward);
        RaycastHit hit;
        //int layer_mask = LayerMask.GetMask("Canvas");
        bool bHit = Physics.Raycast(raycast, out hit);
        Vector3 endPosition = laserCaster.transform.position + (laserCaster.transform.forward * default_dist);
        if (!bHit)
        {
            previousContact = null;
        }
        if (bHit && hit.distance < 100f)
        {
            endPosition = hit.point;
        }

        if (bHit)
        {
            if (hit.transform.tag == "Canvas")
            {
                if (!paintingManager.handMode)
                {
                    DrawingProcess(hit);
                }
                else
                {
                    laserLine.enabled = false;
                }
            }
            else if(hit.transform.tag == "UI")//TODO: General UI Interaction Interface
            {
                laserLine.enabled = true;
                hit.transform.SendMessage("Hover");
                if (SteamVR_Actions.default_InteractUI.GetState(SteamVR_Input_Sources.RightHand))
                {
                    hit.transform.SendMessage("Press");
                }
                
            }
            
        }
        else
        {
            laserLine.enabled = true;
            laserLine.material.color = color;
        }
        laserLine.SetPosition(0, laserCaster.transform.position);
        laserLine.SetPosition(1, endPosition);


    }
}