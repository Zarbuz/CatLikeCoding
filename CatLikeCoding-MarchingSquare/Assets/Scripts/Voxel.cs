using UnityEngine;
using System;

[Serializable]
public class Voxel
{
    public bool State;

    public Vector2 Position, XEdgePosition, YEdgePosition;

    public Voxel(int x, int y, float size)
    {
        Position.x = (x + 0.5f) * size;
        Position.y = (y + 0.5f) * size;

        XEdgePosition = Position;
        XEdgePosition.x += size * 0.5f;
        YEdgePosition = Position;
        YEdgePosition.y += size * 0.5f;
    }

    public Voxel() { }

    public void BecomeXDummyOf(Voxel voxel, float offset)
    {
        State = voxel.State;
        Position = voxel.Position;
        XEdgePosition = voxel.XEdgePosition;
        YEdgePosition = voxel.YEdgePosition;
        Position.x += offset;
        XEdgePosition.x += offset;
        YEdgePosition.x += offset;
    }

    public void BecomeYDummyOf(Voxel voxel, float offset)
    {
        State = voxel.State;
        Position = voxel.Position;
        XEdgePosition = voxel.XEdgePosition;
        YEdgePosition = voxel.YEdgePosition;
        Position.y += offset;
        XEdgePosition.y += offset;
        YEdgePosition.y += offset;
    }

    public void BecomeXYDummyOf(Voxel voxel, float offset)
    {
        State = voxel.State;
        Position = voxel.Position;
        XEdgePosition = voxel.XEdgePosition;
        YEdgePosition = voxel.YEdgePosition;
        Position.x += offset;
        Position.y += offset;
        XEdgePosition.x += offset;
        XEdgePosition.y += offset;
        YEdgePosition.x += offset;
        YEdgePosition.y += offset;
    }
}