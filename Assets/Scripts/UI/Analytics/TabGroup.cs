using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabGroup : MonoBehaviour
{
    [SerializeField] private List<Button> tabButtons;
    [SerializeField] private List<GameObject> tabContents;
    [SerializeField] private Color activeTabColor;
    [SerializeField] private Color inactiveTabColor;
    
    private void Start()
    {
        // Set up tab buttons
        for (int i = 0; i < tabButtons.Count; i++)
        {
            int index = i; // Capture index for lambda
            tabButtons[i].onClick.AddListener(() => SelectTab(index));
        }
        
        // Select first tab by default
        SelectTab(0);
    }
    
    public void SelectTab(int index)
    {
        if (index < 0 || index >= tabButtons.Count || index >= tabContents.Count)
        {
            return;
        }
        
        // Update button colors
        for (int i = 0; i < tabButtons.Count; i++)
        {
            tabButtons[i].GetComponent<Image>().color = (i == index) ? activeTabColor : inactiveTabColor;
        }
        
        // Show selected content, hide others
        for (int i = 0; i < tabContents.Count; i++)
        {
            tabContents[i].SetActive(i == index);
        }
    }
}
