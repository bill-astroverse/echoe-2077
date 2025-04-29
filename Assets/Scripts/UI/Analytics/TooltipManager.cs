using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }
    
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Hide tooltip at start
        tooltipPanel.SetActive(false);
    }
    
    private void Update()
    {
        // Follow mouse position
        if (tooltipPanel.activeSelf)
        {
            Vector2 mousePos = Input.mousePosition;
            tooltipPanel.transform.position = new Vector3(mousePos.x + 20, mousePos.y - 20, 0);
        }
    }
    
    public void ShowTooltip(string text)
    {
        tooltipText.text = text;
        tooltipPanel.SetActive(true);
    }
    
    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}
