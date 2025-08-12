using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI")]
    public RectTransform highlightPanel;       // The panel you want to move
    public List<TextMeshProUGUI> items;
    public Vector3 targetPosition;    // Target local position in the parent
    public GameObject HowToPlayPanel;

    [Header("Movement")]
    public float smoothTime = 0.08f;                // highlightPanel easing
    private Vector3 vel;

    private int index = 0;
    private Vector3 targetLocalPos;
    // Start is called before the first frame update
    void Start()
    {
        if (items == null || items.Count == 0) return;

        Select(index, immediate: true);
    }

    // Update is called once per frame
    void Update()
    {
        if (items == null || items.Count == 0) return;

        // Up (W / UpArrow)
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            index = (index - 1 + items.Count) % items.Count;
            Select(index, immediate: false);
        }

        // Down (S / DownArrow)
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            index = (index + 1) % items.Count;
            Select(index, immediate: false);
        }

        // Enter
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            PerformAction(index);
        }

        // Smoothly move highlightPanel
        if (highlightPanel != null)
        {
            highlightPanel.localPosition = Vector3.SmoothDamp(
                highlightPanel.localPosition,
                targetLocalPos,
                ref vel,
                smoothTime
            );
        }
    }

    void Select(int newIndex, bool immediate)
    {

        // Move target to selected item's local position (align Y, keep current X/Z)
        if (highlightPanel != null && items[newIndex] != null)
        {
            var itemRT = items[newIndex].GetComponent<RectTransform>();
            var hParent = highlightPanel.parent as RectTransform;
            var iParent = itemRT.parent as RectTransform;

            // Assumes highlightPanel and items share the same parent. If not, convert space:
            Vector3 itemLocal = itemRT.localPosition;
            if (hParent != iParent)
            {
                // Convert to world then to highlightPanel parent local space
                Vector3 world = itemRT.TransformPoint(Vector3.zero);
                itemLocal = hParent.InverseTransformPoint(world);
            }

            targetLocalPos = new Vector3(highlightPanel.localPosition.x, itemLocal.y, highlightPanel.localPosition.z);

            if (immediate)
                highlightPanel.localPosition = targetLocalPos;
        }
    }

    void PerformAction(int selected)
    {
        switch (selected)
        {
            case 0:
                Debug.Log("Action 0: Start Game");
                SceneManager.LoadScene("Level1");
                break;
            case 1:
                Debug.Log("Action 1: How To Play");
                gameObject.SetActive(false);
                HowToPlayPanel.SetActive(true);
                break;
            case 2:
                Debug.Log("Action 2: Quit");
                Application.Quit();
                break;
            default:
                Debug.Log($"Action {selected}: Not assigned");
                break;
        }
    }
}
