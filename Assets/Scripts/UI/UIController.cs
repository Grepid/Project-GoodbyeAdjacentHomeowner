using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIController : MonoBehaviour
{
    private enum Error { CanvasNull, NoElement, NoPrefab }
    public static UIController Instance { get; private set; }
    public static GameObject UIcanvas { get; private set; }
    public static Stack<UIElement> ActiveElementStack { get; private set; } = new Stack<UIElement>();

    static int interactables;

    public GameObject DialoguePrefab;
    public GameObject[] UIPrefabs;


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
            return;
        }
    }

    private static void PostError(Error error)
    {
        switch (error)
        {
            case Error.CanvasNull:
                Debug.LogWarning($"Cannot perform action, Canvas is null");
                break;
            case Error.NoElement:
                Debug.LogWarning($"There was no valid element present in this action");
                break;
            case Error.NoPrefab:
                Debug.LogWarning($"The prefab was either null or could not be found.");
                break;
        }
    }

    public static void SetUIcanvas(GameObject canvas)
    {
        if (canvas == UIcanvas) return;
        if (UIcanvas != null)
        {
            Debug.LogWarning("Trying to set a new UICanvas when one already exists");
        }
        UIcanvas = canvas;
    }
    private static bool TryLocateCanvas()
    {
        var cv = GameObject.FindGameObjectWithTag("UICanvas");
        if (cv == null) return false;
        SetUIcanvas(cv);
        return true;
    }
    private static bool TestCanvas()
    {
        if (UIcanvas == null)
        {
            if (!TryLocateCanvas())
            {
                PostError(Error.CanvasNull);
                return false;
            }
            return true;
        }
        return true;
    }
    public static UIElement CreateElement(GameObject elementPrefab)
    {
        if (!TestCanvas()) return null;
        if (elementPrefab == null) PostError(Error.NoElement);
        var obj = Instantiate(elementPrefab, UIcanvas.transform);
        var el = obj.GetComponent<UIElement>();
        if (el == null)
        {
            PostError(Error.NoElement);
            return null;
        }
        if (el.interactable)
        {
            interactables++;
        }
        ReCalibrate();
        return el;
    }

    public static UIElement CreateElement(string prefabName)
    {
        GameObject prefab = Array.Find(Instance.UIPrefabs, p => p.name.Equals(prefabName));
        return CreateElement(prefab);
    }

    public static void AddElementToStack(UIElement element)
    {
        if (element == null) return;
        ActiveElementStack.Push(element);
    }
    public static void PopStack()
    {
        if (ActiveElementStack.Count == 0) return;
        var el = ActiveElementStack.Pop();
        DestroyElement(el);
    }
    public static void DestroyElement(UIElement element)
    {
        var els = ActiveElementStack.ToList();
        if (!els.Contains(element))
        {
            PostError(Error.NoElement);
            return;
        }
        els.Remove(element);
        ActiveElementStack.Clear();
        //Might need to flip the list before pushing elements. Trial by fire :3
        foreach (var el in els)
        {
            ActiveElementStack.Push(el);
        }
        if (element.interactable) interactables--;
        Destroy(element.gameObject);
        ReCalibrate();
    }

    private static void ReCalibrate()
    {
        FindCursorStatus();
    }

    private static void FindCursorStatus()
    {
        if (interactables > 0)
        {
            Player.SetCursor(true);
        }
        else
        {
            Player.SetCursor(false);
        }
    }
}
public static class UIUtills
{
    public static void CloseGame()
    {
        Application.Quit();
    }
}
