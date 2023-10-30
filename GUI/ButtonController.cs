using Bark.Gestures;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using Bark;
using Bark.Tools;

public class ButtonController : MonoBehaviour
{
    public enum Blocker
    {
        MENU_FALLING, NOCLIP_BOUNDARY, PIGGYBACKING, BUTTON_PRESSED
    }

    private Dictionary<Blocker, string> blockerText = new Dictionary<Blocker, string>()
    {
        {Blocker.MENU_FALLING, ""},
        {Blocker.NOCLIP_BOUNDARY, "YOU ARE TOO CLOSE TO A WALL TO ACTIVATE THIS"},
        {Blocker.PIGGYBACKING, $"NO COLLIDE CANNOT BE TOGGLED WHILE PIGGYBACK IS ACTIVE"},
        {Blocker.BUTTON_PRESSED, ""},
    };

    private float buttonPushDistance = 0.03f; // Distance the button travels when pushed
    public Action<ButtonController, bool> OnPressed;
    private float cooldown = .1f, lastPressed = 0;
    public Canvas canvas;
    public Text text;
    private List<Blocker> blockers = new List<Blocker>();
    private Transform buttonModel;
    private Material material;
    private bool _isPressed;

    public bool IsPressed
    {
        get { return _isPressed; }
        set
        {
            _isPressed = value;
            material.color = value ? Color.red : Color.white;
        }
    }
    public bool Interactable
    {
        get { return blockers.Count == 0; }
        private set
        {
            if (value)
                material.color = IsPressed ? Color.red : Color.white;
            else
                material.color = IsPressed ? new Color(.5f, .3f, .3f) : Color.gray;
        }
    }

    protected void Awake()
    {

        string progress = "";
        try
        {
            buttonModel = transform.GetChild(0);
            this.material = buttonModel.GetComponent<Renderer>().material;
            this.gameObject.layer = BarkInteractor.InteractionLayer;
            this.text = GetComponentInChildren<Text>();
            this.text.fontSize = 26;
            var observer = this.gameObject.AddComponent<CollisionObserver>();
            observer.OnTriggerEntered += Press;
            observer.OnTriggerExited += Unpress;
        }
        catch (Exception e) { Logging.Exception(e); Logging.Debug("Reached", progress); }
    }
    protected void Press(GameObject self, Collider collider)
    {
        try
        {
            if (!Interactable && blockerText[blockers[0]].Length > 0)
            {
                Plugin.menuController.helpText.text = blockerText[blockers[0]];
                return;
            }
            if (!Interactable || 
                (collider.gameObject != GestureTracker.Instance.leftPointerInteractor.gameObject &&
                collider.gameObject != GestureTracker.Instance.rightPointerInteractor.gameObject)
            ) return;

            if (Time.time - lastPressed < cooldown) return;

            lastPressed = Time.time;
            IsPressed = !IsPressed;
            OnPressed?.Invoke(this, IsPressed);
            bool isLeft = collider.name.ToLower().Contains("left");
            GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, isLeft, 0.05f);
            var hand = isLeft ? GestureTracker.Instance.leftController : GestureTracker.Instance.rightController;
            GestureTracker.Instance.HapticPulse(isLeft);
            Plugin.menuController.AddBlockerToAllButtons(Blocker.BUTTON_PRESSED);
            Invoke(nameof(RemoveCooldownBlocker), .1f);
            buttonModel.localPosition = Vector3.up * -buttonPushDistance;
        }
        catch (Exception e) { Logging.Exception(e); }
    }

    protected void Unpress(GameObject self, Collider collider)
    {
        if (!collider.name.Contains("Pointer")) return;
        buttonModel.localPosition = Vector3.zero;
    }

    void RemoveCooldownBlocker()
    {
        Plugin.menuController.RemoveBlockerFromAllButtons(Blocker.BUTTON_PRESSED);
    }

    public void SetText(string text)
    {
        this.text.text = text.ToUpper();
    }

    public void AddBlocker(Blocker blocker)
    {
        try
        {
            if (blockers.Contains(blocker)) return;
            Interactable = false;
            blockers.Add(blocker);
        }
        catch (Exception e) { Logging.Exception(e); }
    }

    public void RemoveBlocker(Blocker blocker)
    {
        try
        {
            if (blockers.Contains(blocker))
            {
                blockers.Remove(blocker);
                Interactable = blockers.Count == 0;
            }
        }
        catch (Exception e) { Logging.Exception(e); }
    }
}