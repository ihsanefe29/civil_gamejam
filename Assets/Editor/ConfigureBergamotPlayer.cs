#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ConfigureBergamotPlayer
{
    [MenuItem("Civil Gamejam/Configure Walking Bergamot Player")]
    public static void Configure()
    {
        Sprite[] frames = AssetDatabase.LoadAllAssetsAtPath("Assets/karin_Assets/Walking Bergamot New.aseprite")
            .OfType<Sprite>().OrderBy(sprite => sprite.name).ToArray();
        GameObject player = PrefabUtility.LoadPrefabContents("Assets/prefabs/char.prefab");
        SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
        BergamotWalkAnimator animator = player.GetComponent<BergamotWalkAnimator>();
        if (animator == null) animator = player.AddComponent<BergamotWalkAnimator>();
        renderer.sprite = frames[0];
        animator.Configure(frames);
        PrefabUtility.SaveAsPrefabAsset(player, "Assets/prefabs/char.prefab");
        PrefabUtility.UnloadPrefabContents(player);
        Debug.Log("Walking Bergamot New configured as the player.");
    }
}
#endif
