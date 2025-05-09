using AIGraph;
using Il2CppInterop.Runtime.Injection;
using LevelGeneration;
using UnityEngine;
using Il2cppAIGPortalList = Il2CppSystem.Collections.Generic.List<AIGraph.AIG_CoursePortal>;

namespace EOSExt.CircularZones
{
    internal sealed class ForceConnect : MonoBehaviour
    {
        private void Update()
        {
            if (!_IsDynamicPortalSetup && LinkedSecDoor != null && GameStateManager.CurrentStateName == eGameStateName.InLevel)
            {
                LG_Gate gate = LinkedSecDoor.Gate;
                _Portal = gate.CoursePortal;
                _FromNode = gate.m_linksFrom.m_courseNode;
                _ToNode = gate.m_linksTo.m_courseNode;
                _IsDynamicPortalSetup = true;
            }
            else
            {
                if (_IsDynamicPortalSetup)
                {
                    eDoorStatus status = LinkedSecDoor.m_sync.GetCurrentSyncState().status;
                    eDoorStatus eDoorStatus = status;
                    if (eDoorStatus != eDoorStatus.Open)
                    {
                        SetPortalEnabled(false);
                    }
                    else
                    {
                        SetPortalEnabled(true);
                    }
                }
            }
        }

        private void SetPortalEnabled(bool enabled)
        {
            if (enabled && !_PortalEnabled)
            {
                _FromNode.m_portals.Add(_Portal);
                _ToNode.m_portals.Add(_Portal);
                _PortalEnabled = true;
            }
            else if(!enabled && _PortalEnabled)
            {
                RemoveItem(_FromNode.m_portals, _Portal);
                RemoveItem(_ToNode.m_portals, _Portal);
                _PortalEnabled = false;
            }
        }

        private void RemoveItem(Il2cppAIGPortalList portals, AIG_CoursePortal portal)
        {
            for (int i = 0; i < portals.Count; i++)
            {
                if (portals[i].Pointer == portal.Pointer)
                {
                    portals.RemoveAt(i);
                    break;
                }
            }
        }

        public int ID;

        public LG_SecurityDoor LinkedSecDoor;

        public bool ShouldFlipDoor;

        private bool _IsDynamicPortalSetup;

        private AIG_CourseNode _FromNode;

        private AIG_CourseNode _ToNode;

        private AIG_CoursePortal _Portal;

        private bool _PortalEnabled = true;

        static ForceConnect()
        {
            ClassInjector.RegisterTypeInIl2Cpp<ForceConnect>();
        }
    }
}
