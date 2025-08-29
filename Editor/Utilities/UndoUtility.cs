using UnityEditor;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public class UndoUtility
    {
        public static void RecordObjects(string message, params Object[] objects)
        {
            Undo.RecordObjects(objects, message);
        }
    }
}