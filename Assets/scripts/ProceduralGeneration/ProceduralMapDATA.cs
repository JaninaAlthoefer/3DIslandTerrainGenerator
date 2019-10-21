using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralMap
{

    enum EnvironmentTiles
    {
        UNASSIGNED,
        DRY,
        EDGE,
        COAST,
        WATER,
        MEADOW,
        FOREST,
        MOUNTAIN,
        TOP
    }

    enum ActionTiles
    {
        UNASSIGNED,
        EMPTY,
        ENEMYAUTO,
        ENEMYOIL,
        PLOT,
        START,
        EXIT,
        ITEM
    }

    enum EnvironmentEdge
    {
        INLAND,
        SEA,
        COAST,
        UNASSIGNED
    }

}