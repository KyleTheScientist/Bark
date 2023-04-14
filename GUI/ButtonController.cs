using Bark.Gestures;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System;
using UnityEngine.UI;

public class ButtonController : XRBaseInteractable
{
    private float buttonPushDistance = 0.03f; // Distance the button travels when pushed
    private Vector3 buttonRestPosition; // Initial position of the button
    public Action<ButtonController, bool> OnPressed;
    private float cooldown = .1f, lastPressed = 0;
    private AudioSource audioSource;
    public bool Interactable = true;
    public Canvas canvas;
    public Text text;


    private bool _isPressed;
    public bool IsPressed {
        get { return _isPressed; }
        set { 
            _isPressed = value; 
            this.GetComponent<Renderer>().material.color = value ? Color.red : Color.white;
            if (value)
                transform.localPosition = buttonRestPosition + transform.forward * buttonPushDistance;
            else
                transform.localPosition = buttonRestPosition;
            GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, false, 0.05f);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        buttonRestPosition = transform.localPosition;
        this.interactionManager = BarkInteractor.manager;
        this.interactionLayerMask = LayerMask.GetMask("Water");
        this.gameObject.layer = 4;
        this.text = GetComponentInChildren<Text>();
		this.text.font = GameObject.FindObjectOfType<GorillaLevelScreen>().myText.font;
		this.text.rectTransform.localScale *= 2.75f;
        this.gameObject.AddComponent<CollisionObserver>().OnTriggerEntered += Press;
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    protected void Press(GameObject self, Collider colliders)
    {
        if (!Interactable || !colliders.name.Contains("Pointer")) return;
        if (Time.time - lastPressed < cooldown) return;
        lastPressed = Time.time;
        IsPressed = !IsPressed;
        OnPressed?.Invoke(this, IsPressed);
    }

    protected override void OnHoverExited(XRBaseInteractor interactor)
    {
        base.OnSelectExited(interactor);
    }

    public void SetText(string text)
    {
        this.text.text = text.ToUpper();
    }
}