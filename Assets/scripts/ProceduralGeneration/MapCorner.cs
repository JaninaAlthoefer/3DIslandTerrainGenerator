using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralMap
{
    public class MapCorner
    {
        private Vector2f coord;
        private int height = 0;

        private EnvironmentTiles biome;
        private List<MapEdge> edges;
        private List<MapCorner> neighbours;

        public MapCorner(Vector2f coord)
        {
            this.coord = coord;
            biome = EnvironmentTiles.UNASSIGNED;
            edges = new List<MapEdge>();
            neighbours = new List<MapCorner>();
        }

        public Vector2f Coord
        {
            get
            {
                return coord;
            }

            //set
            //{
            //    coord = value;
            //}
        }

        public List<MapEdge> Edges
        {
            get
            {
                return edges;
            }
        }

        public int Height
        {
            get
            {
                return height;
            }

            set
            {
                height = value;
            }
        }

        public List<MapCorner> Neighbours
        {
            get
            {
                return neighbours;
            }

        }

        internal EnvironmentTiles Biome
        {
            get
            {
                return biome;
            }

            set
            {
                biome = value;
            }
        }

        public void addEdge(MapEdge e)
        {
            edges.Add(e);
        }

        public void addNeighbour(MapCorner c)
        {
            neighbours.Add(c);
        }

        public bool isCoastline()
        {
            if (biome == EnvironmentTiles.EDGE)
                return false;


            int counter = 0;

            foreach (MapEdge e in edges)
            {
                if (e.Biome == EnvironmentEdge.COAST)
                {
                    counter++;
                }
            }

            return (counter>=2);
        }
    }

}