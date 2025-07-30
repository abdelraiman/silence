using UnityEngine;

public static class Vector3Extensions
{//this extention allows me to make a vector 3 with customizable floats in the code set x y and z
    public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null)
    {
        return new Vector3(x ?? vector.x, y ?? vector.y, z ?? vector.z);
    }
}