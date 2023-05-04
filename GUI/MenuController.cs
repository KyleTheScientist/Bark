using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using Bark.Gestures;
using Bark.Modules;
using Bark.Modules.Misc;
using Bark.Modules.Movement;
using Bark.Modules.Physics;
using Bark.Modules.Multiplayer;
using Bark.Modules.Teleportation;
using Bark.Tools;
using Photon.Pun;
using Player = GorillaLocomotion.Player;

namespace Bark.GUI
{
    public class MenuController : XRGrabInteractable
    {
        public static MenuController Instance;
        public bool Built { get; private set; }
        public Vector3
            initialMenuOffset = new Vector3(0, .035f, .65f),
            btnDimensions = new Vector3(.3f, .05f, .05f),
            menuDimensions,
            attachPointOffset,
            buttonOffset;
        public Rigidbody _rigidbody;
        private List<Transform> pages;
        private List<ButtonController> buttons;
        private List<BarkModule> modules;
        public Text helpText;

        protected override void Awake()
        {
            Instance = this;
            try
            {
                Logging.LogDebug("Awake");
                base.Awake();

                gameObject.AddComponent<PositionValidator>();
                var tracker = gameObject.AddComponent<GestureTracker>();
                tracker.OnMeatBeat += () =>
                {
                    if (!Built)
                        BuildMenu();
                    else
                        ResetPosition();
                };

                modules = new List<BarkModule>()
                {
                    // Locomotion
                    gameObject.AddComponent<Airplane>(),
                    gameObject.AddComponent<GrapplingHooks>(),
                    gameObject.AddComponent<Platforms>().Left(),
                    gameObject.AddComponent<Platforms>().Right(),
                    gameObject.AddComponent<Speed>(),
                    gameObject.AddComponent<Wallrun>(),

                    //// Physics
                    gameObject.AddComponent<Freeze>(),
                    gameObject.AddComponent<LowGravity>(),
                    gameObject.AddComponent<NoCollide>(),
                    gameObject.AddComponent<NoSlip>(),
                    gameObject.AddComponent<SlipperyHands>(),

                    //// Teleportation
                    gameObject.AddComponent<Checkpoint>(),
                    gameObject.AddComponent<Teleport>(),
                
                    //// Other Players
                    gameObject.AddComponent<Boxing>(),
                    gameObject.AddComponent<Piggyback>(),
                    gameObject.AddComponent<XRay>(),
                };

                if (PhotonNetwork.LocalPlayer.NickName.ToUpper() == "THERATTIDEVR")
                {
                    modules.Add(gameObject.AddComponent<RatSword>());
                }

            }
            catch (Exception e) { Logging.LogException(e); }
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
                button.RemoveBlocker(ButtonController.Blocker.MENU_FALLING);
            }
        }

        void BuildMenu()
        {
            Logging.LogDebug("Building menu...");
            try
            {
                helpText = this.gameObject.transform.Find("Help Canvas").GetComponentInChildren<Text>();
                helpText.text = "Enable a module to see its tutorial.";
                this.gameObject.transform.Find("Version Canvas").GetComponentInChildren<Text>().text =
                    $"{PluginInfo.Name} {PluginInfo.Version}";

                var collider = this.gameObject.AddComponent<BoxCollider>();
                _rigidbody = gameObject.GetComponent<Rigidbody>();
                _rigidbody.isKinematic = true;
                menuDimensions = collider.bounds.extents;
                buttonOffset = this.gameObject.transform.localPosition + (collider.bounds.min.z * Vector3.forward);

                collider.isTrigger = true;
                this.colliders.Add(collider);
                SetupInteraction();
                SetupButtons();
                ResetPosition();


                Logging.LogDebug("Build successful.");
            }
            catch (Exception ex) { Logging.LogWarning(ex.Message); Logging.LogWarning(ex.StackTrace); return; }
            Built = true;
        }

        bool includeDebugButtons = false;
        public void SetupButtons    ()
        {
            var pageTemplate = this.gameObject.transform.Find("Page");
            int buttonsPerPage = pageTemplate.childCount - 2; // Excludes the prev/next page btns
            int numPages = ((modules.Count - 1) / buttonsPerPage) + 1;
            if (includeDebugButtons)
                numPages++;

            pages = new List<Transform>() { pageTemplate };
            for (int i = 0; i < numPages - 1; i++)
                pages.Add(Instantiate(pageTemplate, this.gameObject.transform));

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
                    if (pressed)
                        helpText.text = module.DisplayName().ToUpper() +
                            "\n\n" + module.Tutorial().ToUpper();
                };
                module.button = btnController;
                btnController.SetText(module.DisplayName().ToUpper());
            }

            AddDebugButtons();

            foreach (Transform page in pages)
            {
                foreach (Transform button in page)
                {
                    if (button.name == "Button Left" && page != pages[0])
                    {
                        var btnController = button.gameObject.AddComponent<ButtonController>();
                        btnController.OnPressed += PreviousPage;
                        btnController.SetText("Prev Page");
                        buttons.Add(btnController);
                        continue;
                    }
                    else if (button.name == "Button Right" && page != pages[pages.Count - 1])
                    {
                        var btnController = button.gameObject.AddComponent<ButtonController>();
                        btnController.OnPressed += NextPage;
                        btnController.SetText("Next Page");
                        buttons.Add(btnController);
                        continue;
                    }
                    else if (!button.GetComponent<ButtonController>())
                        button.gameObject.SetActive(false);

                }
                page.gameObject.SetActive(false);
            }
            pageTemplate.gameObject.SetActive(true);

        }

        private void AddDebugButtons()
        {
            AddDebugButton("Rip Textures", (btn, isPressed) =>
            {
                TextureRipper.Rip();
                 GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(68, false, 0.1f);
            });
        }

        int debugButtons = 0;
        private void AddDebugButton(string title, Action<ButtonController, bool> onPress)
        {
            if (!includeDebugButtons) return;
            var page = pages.Last();
            var button = page.Find($"Button {debugButtons}").gameObject;
            var btnController = button.gameObject.AddComponent<ButtonController>();
            btnController.OnPressed += onPress;
            btnController.SetText(title);
            buttons.Add(btnController);
            debugButtons++;
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
            this.movementType = MovementType.Instantaneous;
            this.retainTransformParent = false;
            this.throwOnDetach = true;
            this.interactionLayerMask = LayerMask.GetMask("Water");
            this.interactionManager = BarkInteractor.manager;
            this.onSelectExited.AddListener((args) =>
            {
                GetComponent<Rigidbody>().isKinematic = false;
                AddBlockerToAllButtons(ButtonController.Blocker.MENU_FALLING);
            });
            this.onSelectEntered.AddListener((args) =>
            {
                GetComponent<Rigidbody>().isKinematic = true;
                RemoveBlockerFromAllButtons(ButtonController.Blocker.MENU_FALLING);
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

        public void AddBlockerToAllButtons(ButtonController.Blocker blocker)
        {
            foreach (var button in buttons)
            {
                button.AddBlocker(blocker);
            }
        }

        public void RemoveBlockerFromAllButtons(ButtonController.Blocker blocker)
        {
            foreach (var button in buttons)
            {
                button.RemoveBlocker(blocker);
            }
        }
    }
}