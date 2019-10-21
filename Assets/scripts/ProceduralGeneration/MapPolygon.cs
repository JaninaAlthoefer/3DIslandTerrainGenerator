using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;

namespace ProceduralMap
{
    public class MapPolygon
    {
        private Vector2f coord;
        private int height =  0;

        private EnvironmentTiles biome;
        private ActionTiles action;

        private List<MapEdge> edges;
        //private Dictionary<MapEdge, MapPolygon> neighbours;

        //generate new polygon from scratch
        public MapPolygon(Vector2f coord)
        {
            this.coord = coord;
            biome = EnvironmentTiles.UNASSIGNED;
            action = ActionTiles.UNASSIGNED;
            edges = new List<MapEdge>();
            //neighbours = new Dictionary<MapEdge, MapPolygon>();
        }

        public MapPolygon(Site s)
        {
            coord = s.Coord;
            biome = EnvironmentTiles.UNASSIGNED;
            action = ActionTiles.UNASSIGNED;
            edges = new List<MapEdge>();
            //neighbours = new Dictionary<MapEdge, MapPolygon>();
        }

        public Vector2f Coord
        {
            get
            {
                return coord;
            }
        }

        public List<MapEdge> Edges
        {
            get
            {
                return edges;
            }

            set
            {
                edges = value;
            }
        }

        //public Dictionary<MapEdge, MapPolygon> Neighbours
        //{
        //    get
        //    {
        //        return neighbours;
        //    }

        //    set
        //    {
        //        neighbours = value;
        //    }
        //}

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

        internal ActionTiles Action
        {
            get
            {
                return action;
            }

            set
            {
                action = value;
            }
        }

        public void AddEdge(MapEdge e)
        {
            edges.Add(e);
        }
    }

}