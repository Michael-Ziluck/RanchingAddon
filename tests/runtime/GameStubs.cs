using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityEngine
{
    public class Object
    {
        public bool Destroyed;
        public static implicit operator bool(Object? instance) => instance != null && !instance.Destroyed;
    }
    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static float Distance(Vector3 first, Vector3 second) =>
            (float)Math.Sqrt((first.x - second.x) * (first.x - second.x) + (first.y - second.y) * (first.y - second.y) + (first.z - second.z) * (first.z - second.z));
    }
    public class Transform { public Vector3 position; }
    public class GameObject : Object
    {
        public string name = "";
        public readonly Transform transform = new Transform();
        public readonly Dictionary<Type, Object> Components = new Dictionary<Type, Object>();
    }
    public class MonoBehaviour : Object
    {
        public GameObject gameObject = new GameObject();
        public Transform transform => gameObject.transform;
        public T GetComponent<T>() where T : Object => gameObject.Components.TryGetValue(typeof(T), out Object component) ? (T)component : null!;
    }
    public static class Time { public static double timeAsDouble; }
}
namespace BepInEx.Configuration
{
    public class ConfigEntry<T> { public T Value; public ConfigEntry(T setting) => Value = setting; }
    public class AcceptableValueRange<T> { public AcceptableValueRange(T minimum, T maximum) { } }
    public class ConfigDescription { public ConfigDescription(string description, object range) { } }
    public class ConfigFile
    {
        public readonly Dictionary<string, object> Entries = new Dictionary<string, object>();
        public ConfigEntry<T> Bind<T>(string section, string key, T setting, object description)
        {
            var entry = new ConfigEntry<T>(setting);
            Entries[section + "/" + key] = entry;
            return entry;
        }
    }
}
namespace BepInEx
{
    [AttributeUsage(AttributeTargets.Class)]
    public class BepInPlugin : Attribute { public BepInPlugin(string guid, string name, string version) { } }
    [AttributeUsage(AttributeTargets.Class)]
    public class BepInDependency : Attribute { public BepInDependency(string guid, string version) { } }
    public class TestLogger
    {
        public void LogInfo(object message) { }
        public void LogError(object message) => throw new Exception(message.ToString());
    }
    public class BaseUnityPlugin : UnityEngine.MonoBehaviour
    {
        public Configuration.ConfigFile Config = new Configuration.ConfigFile();
        public TestLogger Logger = new TestLogger();
    }
    public class PluginInfo { public BaseUnityPlugin Instance = new BaseUnityPlugin(); }
}
namespace BepInEx.Bootstrap
{
    public static class Chainloader
    {
        public static readonly Dictionary<string, BepInEx.PluginInfo> PluginInfos = new Dictionary<string, BepInEx.PluginInfo>();
    }
}
namespace ServerSync
{
    // Configuration sync is an external boundary here; no network behavior is modeled.
    public class ConfigSync
    {
        public string DisplayName = "", CurrentVersion = "", MinimumRequiredVersion = "";
        public ConfigSync(string guid) { }
        public void AddConfigEntry<T>(BepInEx.Configuration.ConfigEntry<T> entry) { }
        public void AddLockingConfigEntry(BepInEx.Configuration.ConfigEntry<bool> entry) { }
    }
}
public class ZDO
{
    public readonly Dictionary<string, float> Floats = new Dictionary<string, float>();
    public readonly Dictionary<int, long> Longs = new Dictionary<int, long>();
    public readonly Dictionary<int, float> Timers = new Dictionary<int, float>();
    public long Owner = 7;
    public int Writes;
    public float GetFloat(string key) => Floats.TryGetValue(key, out float seconds) ? seconds : 0;
    public float GetFloat(int key) => Timers.TryGetValue(key, out float seconds) ? seconds : 0;
    public long GetLong(int key, long fallback) => Longs.TryGetValue(key, out long ticks) ? ticks : fallback;
    public long GetOwner() => Owner;
    public void Set(string key, float seconds) { Floats[key] = seconds; Writes++; }
}
public class ZNetView : UnityEngine.MonoBehaviour
{
    public bool Valid = true, Owner = true;
    public readonly ZDO Record = new ZDO();
    public bool IsValid() => Valid;
    public bool IsOwner() => Owner;
    public ZDO GetZDO() => Record;
}
public static class ZDOVars { public const int s_spawnTime = 1, s_growStart = 2; }
public class ZNet : UnityEngine.Object
{
    public static ZNet instance = new ZNet();
    public DateTime Now = new DateTime(2026, 1, 1);
    public double Seconds = 1000;
    public DateTime GetTime() => Now;
    public double GetTimeSeconds() => Seconds;
}
public static class Utils { public static string GetPrefabName(UnityEngine.GameObject gameObject) => gameObject.name; }
public class Character : UnityEngine.MonoBehaviour
{
    public string Hover = "";
    public string GetHoverName() => gameObject.name;
    [MethodImpl(MethodImplOptions.NoInlining)]
    public string GetHoverText() => Hover;
}
public class Player : Character
{
    public static Player m_localPlayer = null!;
    public static readonly List<Player> Players = new List<Player>();
    public static Player GetClosestPlayer(UnityEngine.Vector3 position, float radius)
    {
        Player? closest = null;
        float distance = radius;
        foreach (Player player in Players)
        {
            float candidateDistance = UnityEngine.Vector3.Distance(position, player.transform.position);
            if (candidateDistance >= distance) continue;
            closest = player;
            distance = candidateDistance;
        }
        return closest!;
    }
}
public class BaseAI : UnityEngine.MonoBehaviour
{
    public TimeSpan Age = TimeSpan.FromSeconds(500);
    public TimeSpan GetTimeSinceSpawned() => Age;
}
public class Growup : UnityEngine.MonoBehaviour
{
    public UnityEngine.GameObject m_grownPrefab = new UnityEngine.GameObject();
    public float m_growTime = 3000;
    public bool Grown;
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void GrowUpdate()
    {
        if (GetComponent<ZNetView>().IsOwner() && GetComponent<BaseAI>().GetTimeSinceSpawned().TotalSeconds >= m_growTime) Grown = true;
    }
}
public class ItemDrop : UnityEngine.MonoBehaviour
{
    public ItemData m_itemData = new ItemData();
    public string GetHoverName() => gameObject.name;
    public class ItemData { public SharedData m_shared = new SharedData(); public int m_stack = 1; }
    public class SharedData { }
}
public class EggGrow : UnityEngine.MonoBehaviour
{
    public UnityEngine.GameObject m_grownPrefab = new UnityEngine.GameObject();
    public float m_growTime = 1800;
    public string Hover = "Warm";
    [MethodImpl(MethodImplOptions.NoInlining)]
    public string GetHoverText() => Hover;
}
