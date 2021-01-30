using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BrushSwitchingButton : MonoBehaviour
{
    // Start is called before the first frame update

    public bool isErasing = false;
    public PaintingManager paintingManager;
    public List<Button> buttonsInGroup;


    private Button m_button;
    private bool selecting = false;
    private Color normalColor;
    private Color hoverColor;


    void Start()
    {
        m_button = GetComponent<Button>();
        normalColor = m_button.colors.normalColor;
        hoverColor = m_button.colors.normalColor;
        hoverColor.a *= 1.2f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!selecting)
        {
            var colors = m_button.colors;
            colors.normalColor = normalColor;
            m_button.colors = colors;
        }
    }

    IEnumerator Deselecting()
    {
        yield return new WaitForSeconds(0.05f);
        selecting = false;

    }

    public void Hover()
    {
        StopAllCoroutines();
        var colors = m_button.colors;
        colors.normalColor = hoverColor;
        m_button.colors = colors;
        selecting = true;
        StartCoroutine(Deselecting());



    }
    public void Press()
    {
        if (!m_button.interactable)
        {
            return;
        }

        m_button.interactable = false;
        for (int i = 0;i<buttonsInGroup.Count;i++)
        {
            buttonsInGroup[i].interactable = true;
        }
        paintingManager.isErasing = isErasing;


    }
}
