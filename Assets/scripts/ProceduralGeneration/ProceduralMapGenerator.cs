using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;
using System;

namespace ProceduralMap
{
    public class ProceduralMapGenerator : MonoBehaviour
    {
        // The number of polygons/sites we want
        [Range (100, 500)]
        public int polygonNumber = 200;
        //the size of the heightmap the terrain uses 
        //used to detail the size of biomes
        public int resolutionSize = 512;

        //variables to influence the world generation
        [Range(0, 10000)]
        public int maxHeight;
        [Range(0, 10000)]
        public int sealevel = 100;
        [Range(0.0f, 1.0f)]
        public float waterChance;// = 0.15f;
        [Range(1, 10)]
        public int waterIterations;// = 2;
        [Range(0.0f, 1.0f)]
        public float enemyChance = 0.1f;
        [Range(0.0f, 1.0f)]
        public float enemyDivergence = 0.5f;
        [Range(0.0f, 1.0f)]
        public float mountainPeakChance = 0.5f;
        

        // This is where we will store the resulting data
        //private Dictionary<Vector2f, Site> sites;
        //private List<Edge> edges;

        private Dictionary<Vector2Int, MapEdge> m_Edges = new Dictionary<Vector2Int, MapEdge>();
        private Dictionary<Vector2Int, MapPolygon> m_Polys = new Dictionary<Vector2Int, MapPolygon>();
        private Dictionary<Vector2Int, MapCorner> m_Corners = new Dictionary<Vector2Int, MapCorner>();

        void Start()
        {
            // Create your sites (lets call that the center of your polygons)
            List<Vector2f> points = CreateRandomPoint();

            // Create the bounds of the voronoi diagram
            // Use Rectf instead of Rect; it's a struct just like Rect and does pretty much the same,
            // but like that it allows you to run the delaunay library outside of unity (which mean also in another tread)
            Rectf bounds = new Rectf(0, 0, resolutionSize, resolutionSize);

            // There is a two ways you can create the voronoi diagram: with or without the lloyd relaxation
            // Here I used it with 2 iterations of the lloyd relaxation
            Voronoi voronoi = new Voronoi(points, bounds, 5);

            // Now retreive the edges from it, and the new sites position if you used lloyd relaxtion
            //sites = voronoi.SitesIndexedByLocation;
            //edges = voronoi.Edges;

            getMapDATA(voronoi.SitesIndexedByLocation, voronoi.Edges);
            SetBiomesAndHeight();
            DisplayVoronoiDiagram();

            
        }

        private List<Vector2f> CreateRandomPoint()
        {
            // Use Vector2f, instead of Vector2
            // Vector2f is pretty much the same than Vector2, but like you could run Voronoi in another thread
            List<Vector2f> points = new List<Vector2f>();
            for (int i = 0; i < polygonNumber; i++)
            {
                points.Add(new Vector2f(UnityEngine.Random.Range(0, resolutionSize), UnityEngine.Random.Range(0, resolutionSize)));
            }

            return points;
        }

        // Here is a very simple way to display the result using a simple bresenham line algorithm
        // Just attach this script to a quad
        //
        //modified to use the new wrapper classes for the world generation
        private void DisplayVoronoiDiagram()
        {
            Texture2D tx = new Texture2D(resolutionSize, resolutionSize);

            //draw the polygon center
            foreach (KeyValuePair<Vector2Int, MapPolygon> kv in m_Polys)
            {
                if (kv.Value.Biome == EnvironmentTiles.WATER)
                {
                    DrawDot(tx, kv.Key, 1, Color.cyan);
                }
                else
                {
                    DrawDot(tx, kv.Key, 1, Color.red);
                }
            }
            //draw the lines between corners
            foreach (KeyValuePair<Vector2Int, MapEdge> kve in m_Edges)
            {
                //draw a line between the 2 corners the edge has
                if (kve.Value.Biome == EnvironmentEdge.SEA)
                {
                    DrawLine(kve.Value.Corners[0].Coord, kve.Value.Corners[1].Coord, tx, Color.cyan);
                }
                else if (kve.Value.Biome == EnvironmentEdge.COAST)
                {
                    DrawLine(kve.Value.Corners[0].Coord, kve.Value.Corners[1].Coord, tx, Color.magenta);
                }
                else
                {
                    DrawLine(kve.Value.Corners[0].Coord, kve.Value.Corners[1].Coord, tx, Color.black);   
                }
            }
            //draw corners of polygon
            foreach (KeyValuePair<Vector2Int, MapCorner> kvc in m_Corners)
            {
                if (kvc.Value.Biome == EnvironmentTiles.EDGE)
                    DrawDot(tx, kvc.Key, 3, Color.blue);
                else if (kvc.Value.Biome == EnvironmentTiles.WATER)
                    DrawDot(tx, kvc.Key, 3, Color.cyan);
                else if (kvc.Value.Biome == EnvironmentTiles.COAST)
                    DrawDot(tx, kvc.Key, 3, Color.magenta);
                else
                    DrawDot(tx, kvc.Key, 1, Color.green);
            }

            tx.Apply();

            GetComponent<Renderer>().material.mainTexture = tx;
        }

        private static void DrawDot(Texture2D tx, Vector2Int pos, int size, Color c)
        {
            tx.SetPixel(pos.x, pos.y, c);

            for (int i = 1; i < (size+1); i++)
            {
                tx.SetPixel(pos.x - i, pos.y, c);
                tx.SetPixel(pos.x + i, pos.y, c);
                tx.SetPixel(pos.x, pos.y - i, c);
                tx.SetPixel(pos.x, pos.y + i, c); 
            }
        }

        //Bresenham line algorithm
        private void DrawLine(Vector2f p0, Vector2f p1, Texture2D tx, Color c, int offset = 0)
        {
            int x0 = (int)p0.x;
            int y0 = (int)p0.y;
            int x1 = (int)p1.x;
            int y1 = (int)p1.y;

            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                tx.SetPixel(x0 + offset, y0 + offset, c);

                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }


        //wrap the voronoi data into classes that hold map information
        private void getMapDATA(Dictionary<Vector2f, Site> sites, List<Edge> edges)
        {
            foreach (KeyValuePair<Vector2f, Site> kv in sites)
            {
                MapCorner nC1, nC2;
                MapEdge edge;
                MapPolygon nPoly = new MapPolygon(kv.Value);

                foreach ( Edge e in kv.Value.Edges)
                {
                    //outside bounds - don't do anything
                    if (e.ClippedEnds == null)
                        continue;

                    //make new corner or get corner from list
                    GetCornersForEdges(out nC1, out nC2, e);


                    //make new edge or get from list
                    Vector2f edgeCoord = (nC1.Coord + nC2.Coord) / 2f;

                    if (m_Edges.ContainsKey((Vector2Int) edgeCoord))
                    {
                        edge = m_Edges[(Vector2Int)edgeCoord];
                        edge.setNeighbourPolygon(nPoly);
                    }
                    else
                    {
                        edge = new MapEdge(nPoly);
                        edge.Corners = new MapCorner[] {nC1, nC2 };
                        m_Edges.Add((Vector2Int)edge.Coord, edge);
                    }


                    //add edge to corners
                    nC1.addEdge(edge);
                    nC2.addEdge(edge);

                    //add edge to polys
                    nPoly.AddEdge(edge);

                }

                m_Polys.Add((Vector2Int)kv.Value.Coord, nPoly);

            }

            
        }

        //return the 2 corners that make up an edge either from the list or make new
        private void GetCornersForEdges(out MapCorner nC1, out MapCorner nC2, Edge e)
        {
            if (m_Corners.ContainsKey((Vector2Int)e.ClippedEnds[LR.LEFT]))
            {
                nC1 = m_Corners[(Vector2Int)e.ClippedEnds[LR.LEFT]];
            }
            else
            {
                nC1 = new MapCorner(e.ClippedEnds[LR.LEFT]);
                m_Corners.Add((Vector2Int)nC1.Coord, nC1);
            }

            if (m_Corners.ContainsKey((Vector2Int)e.ClippedEnds[LR.RIGHT]))
            {
                nC2 = m_Corners[(Vector2Int)e.ClippedEnds[LR.RIGHT]];
            }
            else
            {
                nC2 = new MapCorner(e.ClippedEnds[LR.RIGHT]);
                m_Corners.Add((Vector2Int)nC2.Coord, nC2);
            }

            nC1.addNeighbour(nC2);
            nC2.addNeighbour(nC1);
        }

        //sets the biomes to polygons, edges and corners depending on their input variables
        //the height will be used in a heightmap for the terrain
        private void SetBiomesAndHeight()
        {
            //polys will either be water or unassigned after this
            #region waterbiomes
            #region corners

            foreach (KeyValuePair<Vector2Int, MapCorner> kv in m_Corners)
            {
                MapCorner c = kv.Value;

                if ((int)c.Coord.x == 0 || (int)c.Coord.x == resolutionSize || (int)c.Coord.y == 0 || (int)c.Coord.y == resolutionSize)
                {
                    c.Biome = EnvironmentTiles.EDGE;
                }

            }

            //water will happen if your neighbour or neighbours neighbour is an edge
            foreach (KeyValuePair<Vector2Int, MapCorner> kv in m_Corners)
            {
                MapCorner c = kv.Value;

                if (c.Biome == EnvironmentTiles.UNASSIGNED)
                {
                    foreach (MapCorner mc in c.Neighbours)
                    {
                        if (mc.Biome == EnvironmentTiles.EDGE)
                        {
                            c.Biome = EnvironmentTiles.WATER;
                            break;
                        }

                        foreach (MapCorner mc2 in mc.Neighbours)
                        {
                            if (mc2.Biome == EnvironmentTiles.EDGE)
                            {
                                c.Biome = EnvironmentTiles.WATER;
                                break;
                            }
                        }
                    }


                }
            }

            //water has a chance to happen if your neighbour or neighbours neighbour is water
            foreach (KeyValuePair<Vector2Int, MapCorner> kv in m_Corners)
            {
                MapCorner c = kv.Value;

                if (c.Biome == EnvironmentTiles.UNASSIGNED)
                {
                    foreach (MapCorner mc in c.Neighbours)
                    {
                        if (mc.Biome == EnvironmentTiles.WATER)
                        {


                            if (UnityEngine.Random.Range(0.0f, 1.0f) < waterChance)
                            {
                                c.Biome = EnvironmentTiles.WATER;
                                break;
                            }
                            else
                            {
                                c.Biome = EnvironmentTiles.COAST;
                                break;
                            }
                        }

                        foreach (MapCorner mc2 in mc.Neighbours)
                        {
                            if (mc2.Biome == EnvironmentTiles.WATER)
                            {
                                if (UnityEngine.Random.Range(0.0f, 1.0f) < (waterChance * waterChance))
                                {
                                    c.Biome = EnvironmentTiles.WATER;
                                    break;
                                }
                                else
                                {
                                    c.Biome = EnvironmentTiles.COAST;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region edges
            foreach (KeyValuePair<Vector2Int, MapEdge> kv in m_Edges)
            {
                MapEdge e = kv.Value;

                if (e.isWaterByCorners())
                {
                    e.Biome = EnvironmentEdge.SEA;
                }
                else if (e.isCoastByCorners())
                {
                    e.Biome = EnvironmentEdge.COAST;    
                }
                else
                {
                    e.Biome = EnvironmentEdge.INLAND;
                }
                
            }

            #endregion

            //polys will either be water or unassigned after this
            #region polys

            foreach (KeyValuePair<Vector2Int, MapPolygon> kv in m_Polys)
            {
                MapPolygon p = kv.Value;

                foreach (MapEdge e in p.Edges)
                {
                    if (e.Biome == EnvironmentEdge.SEA)
                    {
                        p.Biome = EnvironmentTiles.WATER;
                        break;
                    }
                }

                
            }

            #endregion

            //maybe leave this out depending on how it looks in the end
            #region smoothing
            /*/reassign edges to smooth the outcome of the generation
            foreach (KeyValuePair<Vector2Int, MapEdge> kv in m_Edges)
            {
                MapEdge e = kv.Value;

                if (e.isWaterByPolygons())
                {
                    e.Biome = EnvironmentEdge.SEA;
                }
                else if (e.isCoastByPolygons())
                {
                    e.Biome = EnvironmentEdge.COAST;
                }
                else
                {
                    e.Biome = EnvironmentEdge.INLAND;
                }
            }

            //reassign corner if they don't belong to a coastline
            foreach(KeyValuePair<Vector2Int, MapCorner> kv in m_Corners)
            {
                MapCorner c = kv.Value;

                if (c.Biome == EnvironmentTiles.EDGE || c.Biome == EnvironmentTiles.UNASSIGNED)
                    continue;

                if (!c.isCoastline())
                {
                    c.Biome = EnvironmentTiles.WATER;
                }

            } //*/
            #endregion


            #endregion

            #region heights

            int highest = 0;

            //corner heights
            foreach(KeyValuePair<Vector2Int, MapCorner> kv in m_Corners)
            {
                MapCorner c = kv.Value;

                //if theres water it'll be as low as possible
                if (c.Biome == EnvironmentTiles.WATER || c.Biome == EnvironmentTiles.EDGE)
                {
                    //Debug.Log("water");
                    c.Height = 0;
                }                
                //if it's coast it'll be at sea level 
                else if (c.Biome == EnvironmentTiles.COAST)
                {
                    //Debug.Log("coast");
                    c.Height = sealevel;
                }
                //compute the height from the distance of every corner then either take the highest or second highest 
                //and add some random elevation 
                //make sure it's below max height
                else
                {
                    //Debug.Log("land");
                    List<int> heights = new List<int>();
                    //get heights from corner distance
                    heights.Add((int)(c.Coord.x * c.Coord.x + c.Coord.y * c.Coord.y));
                    heights.Add((int)((resolutionSize - c.Coord.x) * (resolutionSize - c.Coord.x) + c.Coord.y * c.Coord.y));
                    heights.Add((int)(c.Coord.x * c.Coord.x + (resolutionSize - c.Coord.y) * (resolutionSize - c.Coord.y)));
                    heights.Add((int)((resolutionSize - c.Coord.x) * (resolutionSize - c.Coord.x) + (resolutionSize - c.Coord.y) * (resolutionSize - c.Coord.y)));

                    heights.Sort();

                    //assign heights depending on chance
                    if (UnityEngine.Random.Range(0.0f, 1.0f) < mountainPeakChance)
                    {
                        c.Height = (int) Mathf.Sqrt(heights[3]);
                    }
                    else
                    {
                        c.Height = (int)Mathf.Sqrt(heights[2]);
                    }

                    //increase height the close one gets to the center
                    if (c.Height < (maxHeight*0.5f))
                    {
                        float decrease = 0.00005f * resolutionSize;

                        //fators that increase when closer to middle (0-1)
                        float factorX = (Mathf.Abs((0.5f * resolutionSize) - c.Coord.x) * decrease);
                        float factorY = (Mathf.Abs((0.5f * resolutionSize) - c.Coord.y) * decrease);

                        //resize resfactor by 0.1 to keep the additional height reasonable
                        float resFactor = ((factorX + factorY) * 0.1f) - 0.1f;

                        //add random height to original height
                        int addition = (int) (UnityEngine.Random.Range(0.0f, resFactor) * maxHeight);
                        c.Height += addition;
                        //Debug.Log("lands");
                    }//*/
                     //Debug.Log(c.Height);

                }

                if (c.Height > highest)
                    highest = c.Height;

            }

            //resdistribute heights;

            #endregion

        }


    }

}