using Bark.Gestures;
using Bark.Tools;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using Player = GorillaLocomotion.Player;
using System.Collections.Generic;
using Bark.Modules;
using System.Linq;

namespace Bark.GUI
{
    public class MenuController : XRGrabInteractable
    {
        bool built;
        public Vector3
            initialMenuOffset = new Vector3(0, .035f, .65f),
            btnDimensions = new Vector3(.3f, .05f, .05f),
            menuDimensions,
            attachPointOffset,
            buttonOffset;
        public Rigidbody _rigidbody;
        public GameObject menuBase;
        private List<Transform> pages;
        private List<ButtonController> buttons;
        private List<BarkModule> modules;
        private Text helpText;

        protected override void Awake()
        {
            Logging.LogDebug("Awake");
            base.Awake();

            modules = new List<BarkModule>()
            {
                // Locomotion
                gameObject.AddComponent<Airplane>(),
                gameObject.AddComponent<GrapplingHooks>(),
                gameObject.AddComponent<Platforms>().Left(),
                gameObject.AddComponent<Platforms>().Right(),
                gameObject.AddComponent<DoubleJump>(),
                gameObject.AddComponent<Speed>(),

                //// Physics
                gameObject.AddComponent<NoClip>(),
                gameObject.AddComponent<LowGravity>(),

                //// Teleportation
                gameObject.AddComponent<Checkpoint>(),
                gameObject.AddComponent<Teleport>(),
                
                //// Other Players
                gameObject.AddComponent<Boxing>(),
                gameObject.AddComponent<Piggyback>(),
                gameObject.AddComponent<XRay>(),
            };
        }

        void Start()
        {
            Logging.LogDebug("Start");
            var tracker = gameObject.AddComponent<GestureTracker>();
            tracker.OnMeatBeat += () =>
            {
                if (!built)
                    BuildMenu();
                else
                    ResetPosition();
            };
        }

        void FixedUpdate()
        {
            if (transform.parent)
                this.transform.localScale = Vector3.one;
        }

        void ResetPosition()
        {
            _rigidbody.isKinematic = true;
            _rigidbody.velocity = Vector3.zero;
            transform.SetParent(Player.Instance.bodyCollider.transform);
            transform.localPosition = initialMenuOffset;
            transform.localRotation = Quaternion.identity;
            foreach (var button in buttons)
            {
                button.Interactable = true;
            }
        }

        void BuildMenu()
        {
            Logging.LogDebug("Building menu...");
            try
            {
                // Set initial position
                _rigidbody = gameObject.GetComponent<Rigidbody>();
                gameObject.layer = 4;

                // Make it so you can grab the menu
                menuBase = transform.Find("Bark").gameObject;
                menuBase.layer = 4;

                helpText = menuBase.transform.Find("Help Canvas").GetComponentInChildren<Text>();
                helpText.font = GameObject.FindObjectOfType<GorillaLevelScreen>().myText.font;
                helpText.text = $"{PluginInfo.Name} {PluginInfo.Version}";

                var collider = menuBase.AddComponent<BoxCollider>();
                menuDimensions = collider.bounds.extents;
                buttonOffset = menuBase.transform.localPosition + (collider.bounds.min.z * Vector3.forward);

                collider.isTrigger = true;
                this.colliders.Add(collider);
                SetupInteraction();
                SetupButtons();
                ResetPosition();


                Logging.LogDebug("Build successful.");
            }
            catch (Exception ex) { Logging.LogWarning(ex.Message); Logging.LogWarning(ex.StackTrace); return; }
            built = true;
        }

        public void SetupButtons()
        {
            var pageTemplate = this.menuBase.transform.Find("Page");
            int buttonsPerPage = pageTemplate.childCount - 2;
            int numPages = (modules.Count / buttonsPerPage) + 1;

            pages = new List<Transform>() { pageTemplate }; 
            for (int i = 0; i < numPages - 1; i++)
                pages.Add(Instantiate(pageTemplate, this.menuBase.transform));

            buttons = new List<ButtonController>();
            for (int i = 0; i < modules.Count; i++)
            {
                var module = modules[i];

                var page = pages[i / buttonsPerPage];
                var button = page.Find($"Button {i % buttonsPerPage}").gameObject;

                ButtonController btnController = button.AddComponent<ButtonController>();
                buttons.Add(btnController);
                btnController.OnPressed += (obj, pressed) =>
                {
                    module.enabled = pressed;
                    if(pressed)
                        helpText.text = module.DisplayName().ToUpper() + 
                            "\n\n" + module.Tutorial().ToUpper();
                };
                module.button = btnController;
                btnController.SetText(module.DisplayName().ToUpper());
            }

            foreach (Transform page in pages)
            {
                foreach (Transform button in page)
                {
                    if (button.name == "Button Left" && page != pages[0])
                    {
                        var btnController = button.gameObject.AddComponent<ButtonController>();
                        btnController.OnPressed += PreviousPage;
                        btnController.SetText("Prev Page");
                        continue;
                    }
                    else if (button.name == "Button Right" && page != pages[pages.Count - 1])
                    {
                        var btnController = button.gameObject.AddComponent<ButtonController>();
                        btnController.OnPressed += NextPage;
                        btnController.SetText("Next Page");
                        continue;
                    }
                    else if (!button.GetComponent<ButtonController>())
                        button.gameObject.SetActive(false);
                }
                page.gameObject.SetActive(false);
            }
            pageTemplate.gameObject.SetActive(true);

        }

        private int pageIndex = 0;
        public void PreviousPage(ButtonController button, bool isPressed)
        {
            button.IsPressed = false;
            pageIndex--;
            for (int i = 0; i < pages.Count; i++)
            {
                pages[i].gameObject.SetActive(i == pageIndex);
            }
        }
        public void NextPage(ButtonController button, bool isPressed)
        {
            button.IsPressed = false;
            pageIndex++;
            for (int i = 0; i < pages.Count; i++)
            {
                pages[i].gameObject.SetActive(i == pageIndex);
            }
        }
        public void SetupInteraction()
        {
            this.gravityOnDetach = true;
            this.movementType = XRBaseInteractable.MovementType.Instantaneous;
            this.retainTransformParent = false;
            this.throwOnDetach = true;
            this.interactionLayerMask = LayerMask.GetMask("Water");
            this.interactionManager = BarkInteractor.manager;
            this.onSelectExited.AddListener((args) =>
            {
                GetComponent<Rigidbody>().isKinematic = false;
                foreach (var button in buttons)
                {
                    button.Interactable = false;
                }
            });
            this.onSelectEntered.AddListener((args) =>
            {
                GetComponent<Rigidbody>().isKinematic = true;
                foreach (var button in buttons)
                {
                    button.Interactable = true;
                }
            });

        }

        public Material GetMaterial(string name)
        {
            foreach (var renderer in FindObjectsOfType<Renderer>())
            {
                string _name = renderer.material.name.ToLower();
                if (_name.Contains(name))
                {
                    return renderer.material;
                }
            }
            return null;
        }
    }
}
