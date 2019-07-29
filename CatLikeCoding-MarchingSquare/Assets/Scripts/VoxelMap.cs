using UnityEngine;

public class VoxelMap : MonoBehaviour
{
    private static readonly string[] FillTypeNames = { "Filled", "Empty" };
    private static readonly string[] RadiusNames = { "0", "1", "2", "3", "4", "5" };
    private static readonly string[] StencilNames = { "Square", "Circle" };

    public float Size = 2f;

    public int VoxelResolution = 8;
    public int ChunkResolution = 2;

    public VoxelGrid VoxelGridPrefab;

    private VoxelGrid[] _chunks;

    private float _chunkSize, _voxelSize, _halfSize;

    private int _fillTypeIndex, _radiusIndex, _stencilIndex;

    private readonly VoxelStencil[] _stencils = {
        new VoxelStencil(),
        new VoxelStencilCircle()
    };

    private void Awake()
    {
        _halfSize = Size * 0.5f;
        _chunkSize = Size / ChunkResolution;
        _voxelSize = _chunkSize / VoxelResolution;

        _chunks = new VoxelGrid[ChunkResolution * ChunkResolution];
        for (int i = 0, y = 0; y < ChunkResolution; y++)
        {
            for (int x = 0; x < ChunkResolution; x++, i++)
            {
                CreateChunk(i, x, y);
            }
        }
        BoxCollider box = gameObject.AddComponent<BoxCollider>();
        box.size = new Vector3(Size, Size);
    }

    private void CreateChunk(int i, int x, int y)
    {
        VoxelGrid chunk = Instantiate(VoxelGridPrefab) as VoxelGrid;
        chunk.Initialize(VoxelResolution, _chunkSize);
        chunk.transform.parent = transform;
        chunk.transform.localPosition = new Vector3(x * _chunkSize - _halfSize, y * _chunkSize - _halfSize);
        _chunks[i] = chunk;
        if (x > 0)
        {
            _chunks[i - 1].XNeighbor = chunk;
        }
        if (y > 0)
        {
            _chunks[i - ChunkResolution].yNeighbor = chunk;
            if (x > 0)
            {
                _chunks[i - ChunkResolution - 1].xyNeighbor = chunk;
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                if (hitInfo.collider.gameObject == gameObject)
                {
                    EditVoxels(transform.InverseTransformPoint(hitInfo.point));
                }
            }
        }
    }

    private void EditVoxels(Vector3 point)
    {
        int centerX = (int)((point.x + _halfSize) / _voxelSize);
        int centerY = (int)((point.y + _halfSize) / _voxelSize);

        int xStart = (centerX - _radiusIndex - 1) / VoxelResolution;
        if (xStart < 0)
        {
            xStart = 0;
        }
        int xEnd = (centerX + _radiusIndex) / VoxelResolution;
        if (xEnd >= ChunkResolution)
        {
            xEnd = ChunkResolution - 1;
        }
        int yStart = (centerY - _radiusIndex - 1) / VoxelResolution;
        if (yStart < 0)
        {
            yStart = 0;
        }
        int yEnd = (centerY + _radiusIndex) / VoxelResolution;
        if (yEnd >= ChunkResolution)
        {
            yEnd = ChunkResolution - 1;
        }

        VoxelStencil activeStencil = _stencils[_stencilIndex];
        activeStencil.Initialize(_fillTypeIndex == 0, _radiusIndex);

        int voxelYOffset = yEnd * VoxelResolution;
        for (int y = yEnd; y >= yStart; y--)
        {
            int i = y * ChunkResolution + xEnd;
            int voxelXOffset = xEnd * VoxelResolution;
            for (int x = xEnd; x >= xStart; x--, i--)
            {
                activeStencil.SetCenter(centerX - voxelXOffset, centerY - voxelYOffset);
                _chunks[i].Apply(activeStencil);
                voxelXOffset -= VoxelResolution;
            }
            voxelYOffset -= VoxelResolution;
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(4f, 4f, 150f, 500f));
        GUILayout.Label("Fill Type");
        _fillTypeIndex = GUILayout.SelectionGrid(_fillTypeIndex, FillTypeNames, 2);
        GUILayout.Label("Radius");
        _radiusIndex = GUILayout.SelectionGrid(_radiusIndex, RadiusNames, 6);
        GUILayout.Label("Stencil");
        _stencilIndex = GUILayout.SelectionGrid(_stencilIndex, StencilNames, 2);
        GUILayout.EndArea();
    }
}