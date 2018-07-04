using UnityEngine;

namespace Utilities {
    public class Math {
        public static float GetAngleBetweenVector2(Vector2 vectorA, Vector2 vectorB) {
            float angle =
                Mathf.Rad2Deg * Mathf.Acos(Vector2.Dot(vectorA, vectorB) / (vectorA.magnitude * vectorB.magnitude));

            return angle;
        }

        public static float GetRandomFromVector2(Vector2 vector2) {
            return Random.Range(vector2.x, vector2.y);
        }
    }
}