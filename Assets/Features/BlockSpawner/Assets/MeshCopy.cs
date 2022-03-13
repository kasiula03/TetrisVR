using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshCopy : MonoBehaviour
{
    public List<MeshFilter> MeshesToCopy;

    [SerializeField] private MeshFilter _meshFilter;

    public void Start()
    {
        Mesh mesh = new Mesh
        {
            name = "Combine effect"
        };
        mesh.CombineMeshes(MeshesToCopy.Select(m => new CombineInstance()
        {
            mesh = m.sharedMesh,
            transform = Matrix4x4.Translate(transform.localPosition + m.transform.localPosition),
            subMeshIndex = 0
        }).ToArray());
        _meshFilter.mesh = mesh;
    }
}