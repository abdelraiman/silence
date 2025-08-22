using UnityEngine;
using System.Linq;

public static class GameObjectExtensions
{// this ensures that when a game object is distroyed it will return null
    public static T OrNull<T>(this T obj) where T : Object => obj ? obj : null;
}