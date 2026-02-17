using System.Collections;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace ChalkboardBG.Patches
{
    [HarmonyPatch(typeof(EndlessGameManager))]
    internal class EndlessGameManagerPatch
    {
        private static bool initialized = false;

        [HarmonyPatch("RestartLevel")]
        [HarmonyPostfix]
        private static void RestartLevelPostfix(EndlessGameManager __instance)
        {
            if (!initialized)
            {
                __instance.StartCoroutine(InitEndlessScreenComponents(__instance));
                initialized = true;
            }
        }

        private static IEnumerator InitEndlessScreenComponents(EndlessGameManager instance)
        {
            for (int i = 0; i < 3; i++)
                yield return null;

            MakeAllTextWhite(instance);
        }

        private static void MakeAllTextWhite(EndlessGameManager instance)
        {
            TextMeshProUGUI[] texts = instance.GetComponentsInChildren<TextMeshProUGUI>(true);

            foreach (TextMeshProUGUI text in texts)
            {
                text.color = Color.white;
            }
        }

        [HarmonyPatch(typeof(CoreGameManager), "ReturnToMenu")]
        [HarmonyPrefix]
        private static void ResetInitialization()
        {
            initialized = false;
        }
    }
}
