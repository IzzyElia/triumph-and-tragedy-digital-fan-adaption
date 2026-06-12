using System;
using Godot;
using TT2026.IzzysUI.Tooltips;

namespace TT2026.IzzysUI;

public static class Prefabs
{
    private static string ContextWindowUID = "uid://bxhldg3rpedfh";
    private static string TooltipActionUID = "uid://m20mep2qxn2j";
    private static string ObjectSelectorUID = "uid://d1qvq8v8k8b8w";
    public static PackedScene ContextWindowPrefab = null;
    public static PackedScene TooltipActionPrefab = null;
    public static PackedScene ObjectSelectorPrefab = null;
    static Prefabs()
    {
        ContextWindowPrefab = LoadResource<PackedScene>(ContextWindowUID, "Context Window");
        TooltipActionPrefab = LoadResource<PackedScene>(TooltipActionUID, "Tooltip Action");
        ObjectSelectorPrefab = LoadResource<PackedScene>(ObjectSelectorUID, "Object Selector");
    }

    static T LoadResource<T>(string uid, string context) where T : Resource
    {
        try
        {
            T resource = ResourceLoader.Load<T>(uid);
            if (resource == null) throw new ArgumentException();
            return resource;
        }
        catch (InvalidCastException)
        {
            GD.PrintErr(
                $"{LibraryGlobals.Name}.{typeof(Prefabs).Name}: Failed to load {context} (uid {uid}), as it is not a {typeof(T).Name}");
        }
        catch (ArgumentException)
        {
            GD.PrintErr($"{LibraryGlobals.Name}.{typeof(Prefabs).Name}: Failed to load {context}, is the UID correct? Double check in {typeof(Prefabs).Name}.cs");
        }

        return null;
    }
}