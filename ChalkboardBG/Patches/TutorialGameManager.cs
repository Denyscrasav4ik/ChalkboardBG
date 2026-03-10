using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[HarmonyPatch(typeof(TutorialGameManager), "BeginPlay")]
class TutorialGameManagerPatch
{
    static void Postfix()
    {
        GameObject canvas = Resources.FindObjectsOfTypeAll<GameObject>()
            .FirstOrDefault(o => o.name == "DefaultCanvas");

        if (canvas == null) return;

        Transform bg = canvas.transform.Find("BG");
        if (bg == null) return;

        Image img = bg.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = null;
        }
    }
}
