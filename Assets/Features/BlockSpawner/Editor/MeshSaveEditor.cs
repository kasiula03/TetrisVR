using System.Linq;
using UnityEditor;
using UnityEngine;

public static class MeshSaverEditor {

    [MenuItem("CONTEXT/MeshCopy/Save Mesh...")]
    public static void SaveMeshInPlace (MenuCommand menuCommand) {
        Mesh mesh = new Mesh
        {
            name = "Combine effect"
        };
        MeshCopy toCopy = menuCommand.context as MeshCopy;
        mesh.CombineMeshes(toCopy.MeshesToCopy.Select(m => new CombineInstance()
        {
            mesh = m.sharedMesh,
            transform = Matrix4x4.Translate(toCopy.transform.localPosition + m.transform.localPosition),
            subMeshIndex = 0
        }).ToArray());
        
        
        SaveMesh(mesh, mesh.name, false, true);
    }

    [MenuItem("CONTEXT/MeshCopy/Save Mesh As New Instance...")]
    public static void SaveMeshNewInstanceItem (MenuCommand menuCommand) {
        Mesh mesh = new Mesh
        {
            name = "Combine effect"
        };
        MeshCopy toCopy = menuCommand.context as MeshCopy;
        mesh.CombineMeshes(toCopy.MeshesToCopy.Select(m => new CombineInstance()
        {
            mesh = m.sharedMesh,
            transform = Matrix4x4.Translate(toCopy.transform.localPosition + m.transform.localPosition),
            subMeshIndex = 0
        }).ToArray());
        
        
        SaveMesh(mesh, mesh.name, false, true);
    }

    public static void SaveMesh (Mesh mesh, string name, bool makeNewInstance, bool optimizeMesh) {
        string path = EditorUtility.SaveFilePanel("Save Separate Mesh Asset", "Assets/", name, "asset");
        if (string.IsNullOrEmpty(path)) return;
        
        path = FileUtil.GetProjectRelativePath(path);

        Mesh meshToSave = (makeNewInstance) ? Object.Instantiate(mesh) as Mesh : mesh;
		
        if (optimizeMesh)
            MeshUtility.Optimize(meshToSave);
        
        AssetDatabase.CreateAsset(meshToSave, path);
        AssetDatabase.SaveAssets();
    }
	
}