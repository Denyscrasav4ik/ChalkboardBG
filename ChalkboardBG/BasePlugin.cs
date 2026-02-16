using System.Collections;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class PickModeSpriteReplacer : BaseUnityPlugin
{
    public const string ModGUID = "denyscrasav4ik.basicallyukrainian.chalkboardbg";
    public const string ModName = "Chalkboard BG For Mode Select";
    public const string ModVersion = "1.0.0";

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
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
        GameObject pickMode = null;
        GameObject pickEndlessMap = null;

        while (pickMode == null)
        {
            pickMode = FindGameObjectByName("PickMode");
            yield return null;
        }
        Logger.LogInfo("PickMode found.");
        ReplaceBackgroundSprite(pickMode);
        ChangeAllTMPTextToWhite(pickMode);
        AdjustPickModeText(pickMode);

        while (pickEndlessMap == null)
        {
            pickEndlessMap = FindGameObjectByName("PickEndlessMap");
            yield return null;
        }
        Logger.LogInfo("PickEndlessMap found.");
        ReplaceBackgroundSprite(pickEndlessMap);
        ChangeAllTMPTextToWhite(pickEndlessMap);
        MovePickEndlessChildrenDown(pickEndlessMap);
    }

    private GameObject FindGameObjectByName(string name)
    {
        foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (obj.name == name && obj.hideFlags == HideFlags.None)
            {
                return obj;
            }
        }
        return null;
    }

    private void ReplaceBackgroundSprite(GameObject obj)
    {
        Transform bgTransform = obj.transform.Find("BG");
        if (bgTransform == null)
        {
            Logger.LogError("BG child not found under " + obj.name);
            return;
        }

        Image bgImage = bgTransform.GetComponent<Image>();
        if (bgImage == null)
        {
            Logger.LogError("No Image component found on " + obj.name + "/BG");
            return;
        }

        string spriteFileName = "ChalkWMath480_256c.png";

        if (Chainloader.PluginInfos.ContainsKey("Ukrainization"))
        {
            spriteFileName = "ChalkWMath480_256c_UA.png";
            Logger.LogInfo("Ukrainization detected — using Ukrainian sprite.");
        }

        string imagePath = Path.Combine(
            Application.streamingAssetsPath,
            "Modded",
            ModGUID,
            spriteFileName
        );

        if (!File.Exists(imagePath))
        {
            Logger.LogError($"Sprite file not found: {imagePath}");
            return;
        }

        byte[] fileData = File.ReadAllBytes(imagePath);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.LoadImage(fileData);

        Sprite newSprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );

        bgImage.sprite = newSprite;
        Logger.LogInfo(
            $"{obj.name} background sprite replaced successfully with {spriteFileName}."
        );
    }

    private void ChangeAllTMPTextToWhite(GameObject obj)
    {
        TMP_Text[] texts = obj.GetComponentsInChildren<TMP_Text>(true);

        foreach (TMP_Text text in texts)
        {
            text.color = Color.white;
        }

        Logger.LogInfo($"Changed {texts.Length} TMP text elements to white in {obj.name}.");
    }

    private void AdjustPickModeText(GameObject pickMode)
    {
        Transform modeTextTransform = pickMode.transform.Find("ModeText");
        if (modeTextTransform == null)
        {
            Logger.LogWarning("ModeText not found under PickMode.");
            return;
        }

        TMP_Text modeText = modeTextTransform.GetComponent<TMP_Text>();
        RectTransform rt = modeTextTransform.GetComponent<RectTransform>();
        if (modeText != null && rt != null)
        {
            rt.sizeDelta = new Vector2(rt.sizeDelta.x * 0.85f, rt.sizeDelta.y);
            rt.localPosition = new Vector2(rt.localPosition.x, rt.localPosition.y + 5f);

            modeText.fontSize *= 0.9f;

            Logger.LogInfo("PickMode/ModeText adjusted: narrower and smaller font.");
        }
    }

    private void MovePickEndlessChildrenDown(GameObject pickEndlessMap)
    {
        float moveAmount = -20f;

        foreach (Transform child in pickEndlessMap.transform)
        {
            if (child.name == "BG")
                continue;

            TMP_Text tmpText = child.GetComponent<TMP_Text>();
            if (tmpText != null || child.name.Contains("CategoryButton"))
            {
                child.localPosition += new Vector3(0f, moveAmount, 0f);
            }
        }

        Logger.LogInfo("Moved objects down in PickEndlessMap.");
    }
}
