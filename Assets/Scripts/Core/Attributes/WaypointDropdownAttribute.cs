// Done
using UnityEngine;

namespace Core.Attributes
{
    public class WaypointDropdownAttribute : PropertyAttribute
    {
        public string SceneFieldName { get; }
        public WaypointDropdownAttribute(string sceneFieldName) => SceneFieldName = sceneFieldName;
    }
}