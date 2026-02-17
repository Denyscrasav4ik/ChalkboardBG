using System.Collections;
using System.IO;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ChalkboardBG
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class ChalkboardBG : BaseUnityPlugin
    {
        public const string ModGUID = "denyscrasav4ik.basicallyukrainian.chalkboardbg";
        public const string ModName = "Chalkboard BG For Mode Select";
        public const string ModVersion = "2.0.0";
        private Harmony? harmonyInstance = null!;

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            harmonyInstance = new Harmony(ModGUID);
            harmonyInstance.PatchAll();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "MainMenu")
            {
                StartCoroutine(ApplyChangesWhenReady());
            }
        }

        private IEnumerator ApplyChangesWhenReady()
        {
            yield return null;

            ReplaceAllNullBGSprites();
            ChangeSceneTMPTextToWhite();
            MoveChildrenDown(FindGameObjectByName("PickEndlessMap"));
            MoveTitleUnderEndlessOverview();
            AdjustText(FindGameObjectByName("PickMode"), true);
            AdjustText(FindGameObjectByName("PickChallenge"), false);
            AdjustText(FindGameObjectByName("PickFieldTrip"), false);
            AdjustChallengeButtons();
            MoveHideSeekElementsDown();
            AdjustHideSeekToggles();
            ReplaceHideSeekBoxes();
            MakeHighScoreButtonsTransparent();
            AdjustHighScoreTextPositions();
        }

        private void ReplaceHideSeekBoxes()
        {
            GameObject hideSeekMenu = FindGameObjectByName("HideSeekMenu");
            if (hideSeekMenu == null)
                return;

            string imagePath = Path.Combine(
                Application.streamingAssetsPath,
                "Modded",
                ModGUID,
                "WhiteCheckBox.png"
            );

            if (!File.Exists(imagePath))
                return;

            byte[] fileData = File.ReadAllBytes(imagePath);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(fileData);

            Sprite newSprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            foreach (Transform child in hideSeekMenu.GetComponentsInChildren<Transform>(true))
            {
                if (child.name != "Box")
                    continue;

                Image image = child.GetComponent<Image>();
                if (image != null)
                    image.sprite = newSprite;
            }
        }

        private void ReplaceAllNullBGSprites()
        {
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            string spriteFileName = "ChalkWMath480_256c.png";

            if (Chainloader.PluginInfos.ContainsKey("Ukrainization"))
                spriteFileName = "ChalkWMath480_256c_UA.png";

            string imagePath = Path.Combine(
                Application.streamingAssetsPath,
                "Modded",
                ModGUID,
                spriteFileName
            );

            if (!File.Exists(imagePath))
                return;

            byte[] fileData = File.ReadAllBytes(imagePath);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(fileData);

            Sprite newSprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            foreach (GameObject obj in allObjects)
            {
                if (obj.name != "BG")
                    continue;

                if (obj.hideFlags != HideFlags.None)
                    continue;
                if (IsUnderParent(obj.transform, "Data"))
                    continue;
                if (IsUnderParent(obj.transform, "About"))
                    continue;
                Image image = obj.GetComponent<Image>();
                if (image == null || image.sprite != null)
                    continue;

                image.sprite = newSprite;
            }
        }

        private GameObject FindGameObjectByName(string name)
        {
            foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (obj.name == name && obj.hideFlags == HideFlags.None)
                    return obj;
            }
            return null;
        }

        private void ChangeSceneTMPTextToWhite()
        {
            TMP_Text[] texts = Resources.FindObjectsOfTypeAll<TMP_Text>();

            foreach (TMP_Text text in texts)
            {
                if (text == null)
                    continue;

                GameObject obj = text.gameObject;

                if (obj.hideFlags != HideFlags.None)
                    continue;

                if (obj.scene.name == null || obj.scene.name == "DontDestroyOnLoad")
                    continue;

                if (IsUnderParent(obj.transform, "ElevatorScreen"))
                    continue;
                if (IsUnderParent(obj.transform, "Menu"))
                    continue;
                if (IsUnderParent(obj.transform, "Options"))
                    continue;
                if (IsUnderParent(obj.transform, "Tooltip"))
                    continue;
                if (IsUnderParent(obj.transform, "NameEntry"))
                    continue;
                if (IsUnderParent(obj.transform, "About"))
                    continue;

                text.color = Color.white;
            }
        }

        private bool IsUnderParent(Transform child, string parentName)
        {
            Transform current = child;

            while (current != null)
            {
                if (current.name == parentName)
                    return true;

                current = current.parent;
            }

            return false;
        }

        private void AdjustText(GameObject target, bool moveUp)
        {
            if (target == null)
                return;

            Transform textTransform = target.transform.Find("ModeText");
            if (textTransform == null)
                return;

            TMP_Text text = textTransform.GetComponent<TMP_Text>();
            RectTransform rt = textTransform.GetComponent<RectTransform>();

            if (text != null && rt != null)
            {
                rt.sizeDelta = new Vector2(rt.sizeDelta.x * 0.85f, rt.sizeDelta.y);

                if (moveUp)
                    rt.localPosition = new Vector2(rt.localPosition.x, rt.localPosition.y + 5f);

                text.fontSize *= 0.9f;
            }
        }

        private void MakeHighScoreButtonsTransparent()
        {
            GameObject endlessOverview = FindGameObjectByName("EndlessMapOverview");
            if (endlessOverview == null)
                return;

            for (int i = 1; i <= 10; i++)
            {
                Transform highScore = endlessOverview.transform.Find($"HighScore{i}");
                if (highScore == null)
                    continue;

                Transform buttonTransform = highScore.Find("Button");
                if (buttonTransform == null)
                    continue;

                Image image = buttonTransform.GetComponent<Image>();
                if (image != null)
                {
                    Color c = image.color;
                    c.a = 0f;
                    image.color = c;
                }
            }
        }

        private void AdjustHighScoreTextPositions()
        {
            GameObject endlessOverview = FindGameObjectByName("EndlessMapOverview");
            if (endlessOverview == null)
                return;

            for (int i = 1; i <= 10; i++)
            {
                Transform highScore = endlessOverview.transform.Find($"HighScore{i}");
                if (highScore == null)
                    continue;

                if (i <= 5)
                {
                    Transform rank = highScore.Find("Rank");
                    Transform name = highScore.Find("Name");

                    if (rank != null)
                        rank.localPosition += new Vector3(30f, 0f, 0f);
                    if (name != null)
                        name.localPosition += new Vector3(30f, 0f, 0f);
                }
                else
                {
                    Transform score = highScore.Find("Score");
                    if (score != null)
                        score.localPosition += new Vector3(-10f, 0f, 0f);
                }
            }
        }

        private void AdjustHideSeekToggles()
        {
            GameObject hideSeekMenu = FindGameObjectByName("HideSeekMenu");
            if (hideSeekMenu == null)
                return;

            bool isUkrainizationLoaded = Chainloader.PluginInfos.ContainsKey("Ukrainization");

            Transform mapToggle = hideSeekMenu.transform.Find("MapToggle");
            Transform inventoryToggle = hideSeekMenu.transform.Find("InventoryToggle");
            Transform timeToggle = hideSeekMenu.transform.Find("TimeToggle");

            if (mapToggle != null)
            {
                RectTransform rt = mapToggle.GetComponent<RectTransform>();
                if (rt != null)
                {
                    float offsetX = isUkrainizationLoaded ? -100f : -120f;
                    rt.localPosition += new Vector3(offsetX, 0f, 0f);
                }

                Transform toggleText = mapToggle.Find("ToggleText");
                if (toggleText != null)
                {
                    RectTransform textRt = toggleText.GetComponent<RectTransform>();
                    if (textRt != null)
                        textRt.localPosition += new Vector3(10f, 0f, 0f);
                }
            }

            if (inventoryToggle != null)
            {
                RectTransform rt = inventoryToggle.GetComponent<RectTransform>();
                if (rt != null)
                    rt.localPosition += new Vector3(165f, 40f, 0f);
            }

            if (timeToggle != null)
            {
                RectTransform rt = timeToggle.GetComponent<RectTransform>();
                if (rt != null)
                    rt.localPosition += new Vector3(70f, 0f, 0f);
            }
        }

        private void MoveHideSeekElementsDown()
        {
            GameObject hideSeekMenu = FindGameObjectByName("HideSeekMenu");
            if (hideSeekMenu == null)
                return;

            float moveAmount = -20f;

            Transform mainNew = hideSeekMenu.transform.Find("MainNew");
            Transform seedInput = hideSeekMenu.transform.Find("SeedInput");

            if (mainNew != null)
            {
                RectTransform rt = mainNew.GetComponent<RectTransform>();
                if (rt != null)
                    rt.localPosition += new Vector3(0f, moveAmount, 0f);
            }

            if (seedInput != null)
            {
                RectTransform rt = seedInput.GetComponent<RectTransform>();
                if (rt != null)
                    rt.localPosition += new Vector3(0f, moveAmount, 0f);
            }
        }

        private void AdjustChallengeButtons()
        {
            GameObject pickChallenge = FindGameObjectByName("PickChallenge");
            if (pickChallenge == null)
                return;

            Transform speedy = pickChallenge.transform.Find("Speedy");
            Transform grapple = pickChallenge.transform.Find("Grapple");

            if (speedy != null)
            {
                RectTransform rt = speedy.GetComponent<RectTransform>();
                if (rt != null)
                    rt.localPosition += new Vector3(10f, 0f, 0f);
            }

            if (grapple != null)
            {
                RectTransform rt = grapple.GetComponent<RectTransform>();
                if (rt != null)
                    rt.localPosition += new Vector3(-10f, 0f, 0f);
            }
        }

        private void MoveTitleUnderEndlessOverview()
        {
            GameObject endlessOverview = FindGameObjectByName("EndlessMapOverview");
            if (endlessOverview == null)
                return;

            Transform title = endlessOverview.transform.Find("Title");
            if (title == null)
                return;

            RectTransform rt = title.GetComponent<RectTransform>();
            if (rt != null)
                rt.localPosition += new Vector3(0f, -20f, 0f);
        }

        private void MoveChildrenDown(GameObject pickEndlessMap)
        {
            float moveAmount = -20f;

            foreach (Transform child in pickEndlessMap.transform)
            {
                if (child.name == "BG")
                    continue;

                TMP_Text tmpText = child.GetComponent<TMP_Text>();
                if (tmpText != null || child.name.Contains("CategoryButton"))
                    child.localPosition += new Vector3(0f, moveAmount, 0f);
            }
        }
    }
}
