using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder.Experimental.CSG
{
    /// <summary>
    /// Representation of a mesh in CSG terms.  Contains methods for translating to and from UnityEngine.Mesh.
    /// </summary>
    sealed class CSG_Model
    {
        public List<CSG_Vertex> vertices;
        public List<int> indexes;

        public CSG_Model()
        {
            vertices = new List<CSG_Vertex>();
            indexes = new List<int>();
        }

        /**
         * Initialize a CSG_Model with the mesh of a gameObject.
         */
        public CSG_Model(GameObject go)
        {
            var mesh = go.GetComponent<MeshFilter>().sharedMesh;
            var transform = go.GetComponent<Transform>();

            vertices = CSG_VertexUtility.GetVertices(mesh).Select(x => transform.TransformVertex(x)).ToList();
            indexes = new List<int>(mesh.triangles);
        }

        public CSG_Model(List<CSG_Polygon> list)
        {
            this.vertices = new List<CSG_Vertex>();
            this.indexes = new List<int>();

            int p = 0;
            for (int i = 0; i < list.Count; i++)
            {
                CSG_Polygon poly = list[i];

                for (int j = 2; j < poly.vertices.Count; j++)
                {
                    this.vertices.Add(poly.vertices[0]);
                    this.indexes.Add(p++);

                    this.vertices.Add(poly.vertices[j - 1]);
                    this.indexes.Add(p++);

                    this.vertices.Add(poly.vertices[j]);
                    this.indexes.Add(p++);
                }
            }
        }

        public List<CSG_Polygon> ToPolygons()
        {
            List<CSG_Polygon> list = new List<CSG_Polygon>();

            for (int i = 0; i < indexes.Count; i += 3)
            {
                List<CSG_Vertex> triangle = new List<CSG_Vertex>()
                {
                    vertices[indexes[i + 0]],
                    vertices[indexes[i + 1]],
                    vertices[indexes[i + 2]]
                };

                list.Add(new CSG_Polygon(triangle));
            }

            return list;
        }

        /**
         * Converts a CSG_Model to a Unity mesh.
         */
        public Mesh ToMesh()
        {
            var mesh = new Mesh();
            CSG_VertexUtility.SetMesh(mesh, vertices);
            mesh.triangles = indexes.ToArray();
            return mesh;
        }
    }
}
