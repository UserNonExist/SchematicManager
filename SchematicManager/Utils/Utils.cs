using UnityEngine;

namespace SchematicManager.Utils;

public class Utils
{
    public class PosRot
    {
        public Vector3 Pos;
        public Vector3 Rot;

        public PosRot(Vector3 pos, Vector3 rot)
        {
            Pos = pos;
            Rot = rot;
        }
    }
}