using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MimicServer))]
class MimicServerEditor : Editor
{
    private MimicServer mimicServer;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        mimicServer = (MimicServer)target;
        if (GUILayout.Button("Send Command"))
        {
            mimicServer.SendMessageToClient(mimicServer.CommandToSend); 
            Debug.Log("Mimic'd sending to client " + mimicServer.CommandToSend);
        }
    }
}

