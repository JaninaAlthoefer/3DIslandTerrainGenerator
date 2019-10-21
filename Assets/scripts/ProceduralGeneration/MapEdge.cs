using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralMap
{
    public class MapEdge
    {
        private Vector2f coord;

        private MapCorner[] corners;
        private MapPolygon[] polygons;

        private EnvironmentEdge biome;

        public MapEdge(MapPolygon polygon)
        {
            polygons = new MapPolygon[2];
            this.polygons[0] = polygon;


            biome = EnvironmentEdge.UNASSIGNED;
        }

        public MapCorner[] Corners
        {
            get
            {
                return corners;
            }

            set
            {
                corners = value;

                coord = (Corners[0].Coord + Corners[1].Coord) / 2.0f;

            }
        }

        public MapPolygon[] Polygons
        {
            get
            {
                return polygons;
            }

            set
            {
                polygons = value;
            }
        }

        internal EnvironmentEdge Biome
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

        public Vector2f Coord
        {
            get
            {
                return coord;
            }

        }

        public void setNeighbourPolygon(MapPolygon poly)
        {
            if (polygons[0] != null)
            {
                polygons[1] = poly;
            }
            else
            {
                polygons[0] = poly;
            }
        }

        public MapPolygon getNeighbourPolygon(MapPolygon fix)
        {
            if (polygons[0] != fix)
            {
                return polygons[0];
            }
            else
            {
                return polygons[1];
            }
        }

        //returns if one or both of the corners are water biomes or not
        public bool isWaterByCorners()
        {
            if (corners[0].Biome == EnvironmentTiles.WATER || corners[0].Biome == EnvironmentTiles.EDGE)
                return true;

            if (corners[1].Biome == EnvironmentTiles.WATER || corners[1].Biome == EnvironmentTiles.EDGE)
                return true;


            return false;
        }

        //returns if both corners are coast biomes
        public bool isCoastByCorners()
        {
            if (corners[0].Biome == EnvironmentTiles.COAST || corners[1].Biome == EnvironmentTiles.COAST)
                return true;
            else
                return false;
        }

        //returns if the edge lies between two water biomes
        public bool isWaterByPolygons()
        {
            if (polygons[0].Biome == EnvironmentTiles.WATER && polygons[1].Biome == EnvironmentTiles.WATER)
                return true;
            else
                return false;
        }

        //returns if the edge lies between a water and a coast biome
        public bool isCoastByPolygons()
        {
            if ((polygons[0].Biome == EnvironmentTiles.WATER && polygons[1].Biome != EnvironmentTiles.WATER)
                || (polygons[0].Biome != EnvironmentTiles.WATER && polygons[1].Biome == EnvironmentTiles.WATER))
                return true;
            else
                return false;
        }
    }

}