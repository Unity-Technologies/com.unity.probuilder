using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    public static class SelectPathFaces
    {
        public static List<Face> GetPath(Face start, Face end, ProBuilderMesh mesh)
        {
            List<Face> currentPath = new List<Face>();
            Face current = start;
            currentPath.Add(start);
            int currentWeight = 0;
            do
            {
                foreach (var edge in current.edges)
                {
                    var faces = ElementSelection.GetNeighborFaces(mesh, edge);
                    foreach (var face in faces)
                    {

                    }
                }
            } while (current != end);
         
           
            return null;
        }

        private static List<Face> Dijkstra(List<Tuple<Face, int>> graph, )
        {

        }

    }

    


    private static List<Face> GetPathInternal(Face current, Face end, ProBuilderMesh mesh, int weight, List<Face> faces)
    {
        if (current == end)
        {
            return faces;
        }
    }
}
