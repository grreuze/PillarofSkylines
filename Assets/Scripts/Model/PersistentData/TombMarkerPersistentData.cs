using Game.EchoSystem;
using Game.LevelElements;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Model
{
	[System.Serializable]
	public class TombMarkerPersistentData : PersistentData, IWaypoint
    {
        //###########################################################

        // -- CONSTANTS

        [Header("Waypoint")]
        [SerializeField] private bool useCameraAngle;
        [SerializeField] private float cameraAngle;

        //###########################################################

        // -- ATTRIBUTES

        public bool IsWaypoint { get; set; }
        public bool IsTombCollected { get; set; }

		float lastPosX, lastPosY, lastPosZ;

		private string IDfromOwner;

		//###########################################################

		// -- INITIALIZATION

		public TombMarkerPersistentData(string unique_id, Vector3 position) : base(unique_id)
        {
			lastPosX = position.x;
			lastPosY = position.y;
			lastPosZ = position.z;
        }

        //###########################################################

        // -- INQUIRIES
		
        public bool UseCameraAngle { get { return useCameraAngle; } }
        public float CameraAngle { get { return cameraAngle; } }

        public Vector3 Position
        {
            get
            {
                if (IDfromOwner != null)
                {
                    return ActiveInstance().Transform.position;
                }
                return new Vector3(lastPosX, lastPosY, lastPosZ);
            }
        }

		public TombMarker ActiveInstance() {
			return TombMarker.allMarkers[IDfromOwner];
		}

        //###########################################################

        // -- OPERATIONS

        void IWaypoint.OnWaypointRemoved()
        {
            if (IsWaypoint)
            {
                IsWaypoint = false;
				ActiveInstance()?.ActivateWaypoint(false);
            }
        }

        public void SetActiveInstance(Utilities.UniqueId instanceID = null)
        {
            if (instanceID == null && IDfromOwner != null)
            {
                Vector3 pos = ActiveInstance().Transform.position;

				lastPosX = pos.x;
				lastPosY = pos.y;
				lastPosZ = pos.z;
			}
			if (instanceID)
				IDfromOwner = instanceID.Id;
        }
    }
} // end of namespace