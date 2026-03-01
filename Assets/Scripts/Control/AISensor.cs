using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;

namespace Control
{
    [ExecuteInEditMode]
    public class AISensor : MonoBehaviour
    {
        [SerializeField] private float distance = 10.0f;
        [SerializeField] private float angle = 30.0f;
        [SerializeField] private Color meshColor = new(1f, 0f, 0f, 0.3f);
        [SerializeField] private int scanFrequency = 30;
        [SerializeField] private LayerMask layers;
        [SerializeField] private LayerMask occlusionLayers;
        [SerializeField] private List<GameObject> objects = new();

        private readonly Collider2D[] _colliders = new Collider2D[50];
        private ContactFilter2D _contactFilter;
        private Mesh _mesh;
        private int _count;
        private float _scanInterval;
        private float _scanTimer;
        private Direction _facingDirection = Direction.Down;

        public float Distance => distance;
        public float Angle => angle;
        public List<GameObject> Objects => objects;

        public bool IsPlayerInRange() => objects.Any(obj => obj.CompareTag("Player"));

        public void SetFacingDirection(Direction direction)
        {
            if (_facingDirection == direction) return;
            _facingDirection = direction;
            _mesh = CreateConeMesh();
        }

        private void Start()
        {
            _scanInterval = 1.0f / scanFrequency;
            _mesh = CreateConeMesh();
            
            _contactFilter = new ContactFilter2D();
            _contactFilter.SetLayerMask(layers);
            _contactFilter.useTriggers = true;
        }

        private void Update()
        {
            _scanTimer -= Time.deltaTime;

            if (_scanTimer < 0)
            {
                _scanTimer += _scanInterval;
                Scan();
            }
        }

        private void Scan()
        {
            _count = Physics2D.OverlapCircle(transform.position, distance, _contactFilter, _colliders);
            
            objects.Clear();

            for (int i = 0; i < _count; ++i)
            {
                GameObject obj = _colliders[i].gameObject;

                if (IsInSight(obj))
                    objects.Add(obj);
            }
        }

        private bool IsInSight(GameObject obj)
        {
            Vector2 origin = transform.position;
            Vector2 dest = obj.transform.position;
            Vector2 direction = dest - origin;

            Vector2 facingDir = GetFacingVector();
            float deltaAngle = Vector2.Angle(facingDir, direction);

            if (deltaAngle > angle)
                return false;

            if (Physics2D.Linecast(origin, dest, occlusionLayers))
                return false;

            return true;
        }

        private Vector2 GetFacingVector()
        {
            return _facingDirection switch
            {
                Direction.Up    => Vector2.up,
                Direction.Down  => Vector2.down,
                Direction.Left  => Vector2.left,
                Direction.Right => Vector2.right,
                _ => Vector2.down
            };
        }

        private float GetFacingAngle()
        {
            return _facingDirection switch
            {
                Direction.Up    => 90f,
                Direction.Down  => -90f,
                Direction.Left  => 180f,
                Direction.Right => 0f,
                _ => -90f
            };
        }

        private Mesh CreateConeMesh()
        {
            Mesh coneMesh = new();

            int segments = 20;
            int numTriangles = segments;
            int numVertices = numTriangles * 3;

            Vector3[] vertices = new Vector3[numVertices];
            int[] triangles = new int[numVertices];

            float facingAngle = GetFacingAngle();
            float startAngle = facingAngle - angle;
            float deltaAngle = (angle * 2f) / segments;

            int vert = 0;

            for (int i = 0; i < segments; i++)
            {
                float a1 = (startAngle + deltaAngle * i) * Mathf.Deg2Rad;
                float a2 = (startAngle + deltaAngle * (i + 1)) * Mathf.Deg2Rad;

                // Reversed winding order so the face points toward the 2D camera (Z-negative)
                vertices[vert++] = Vector3.zero;
                vertices[vert++] = new Vector3(Mathf.Cos(a2), Mathf.Sin(a2), 0f) * distance;
                vertices[vert++] = new Vector3(Mathf.Cos(a1), Mathf.Sin(a1), 0f) * distance;
            }

            for (int i = 0; i < numVertices; i++)
                triangles[i] = i;

            coneMesh.vertices = vertices;
            coneMesh.triangles = triangles;
            coneMesh.RecalculateNormals();

            return coneMesh;
        }

        private void OnValidate()
        {
            _mesh = CreateConeMesh();
            _scanInterval = 1.0f / scanFrequency;
        }

        private void OnDrawGizmos()
        {
            if (_mesh && enabled)
            {
                Gizmos.color = meshColor;
                Gizmos.DrawMesh(_mesh, transform.position, Quaternion.identity);
            }

            Gizmos.color = Color.green;
            foreach (GameObject obj in objects)
                Gizmos.DrawWireSphere(obj.transform.position, 0.3f);
        }
    }
}