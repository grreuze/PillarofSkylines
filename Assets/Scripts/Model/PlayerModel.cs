using Game.LevelElements;
using Game.Utilities;
using Game.World;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

namespace Game.Model
{
	[Serializable]
	class PlayerData {
		public Dictionary<AbilityType, AbilityState> AbilityStateDictionary;
		public Dictionary<PillarMarkId, PillarMarkState> PillarMarkStateDictionary;
		public Dictionary<PillarId, PillarState> PillarStateDictionary;

		public Dictionary<string, PersistentData> PersistentDataDictionary = new Dictionary<string, PersistentData>();
		public string[] FireflyList;
		public int GatheredFireflyCount;

		public float posX, posY, posZ;
		public float rotX, rotY, rotZ, rotW;
	}

    /// <summary>
    /// This class stores data about the player: favours, abilities, stats, ...
    /// </summary>
    public class PlayerModel
    {
        //###########################################################

        // -- ATTRIBUTES

        public AbilityData AbilityData { get; private set; }
        public LevelData LevelData { get; private set; }

        public bool PlayerHasNeedle { get; set; }
		public bool ContinuedGame = false;

        private Dictionary<AbilityType, AbilityState> AbilityStateDictionary;
        private Dictionary<PillarMarkId, PillarMarkState> PillarMarkStateDictionary;
        private Dictionary<PillarId, PillarState> PillarStateDictionary;

        private Dictionary<string, PersistentData> PersistentDataDictionary = new Dictionary<string, PersistentData>();
        private List<Firefly> FireflyList = new List<Firefly>();
        private int GatheredFireflyCount;

		private GameControl.GameController GameController;

		public Vector3 SpawnPosition;
		public Quaternion SpawnRotation;

		//###########################################################

		// -- INITIALIZATION

		public PlayerModel(GameControl.GameController gc)
        {
			GameController = gc;

			AbilityData = Resources.Load<AbilityData>("ScriptableObjects/AbilityData");
            LevelData = Resources.Load<LevelData>("ScriptableObjects/LevelData");

			if (File.Exists(Application.persistentDataPath + dataFile)) {
				Load();

			} else {
				Init();
			}
		}

		void Init() {
			AbilityStateDictionary = new Dictionary<AbilityType, AbilityState>();
			foreach (var ability in Enum.GetValues(typeof(AbilityType)).Cast<AbilityType>()) {
				AbilityStateDictionary.Add(ability, AbilityState.inactive);
			}

			PillarMarkStateDictionary = new Dictionary<PillarMarkId, PillarMarkState>();
			foreach (var pillar_mark in Enum.GetValues(typeof(PillarMarkId)).Cast<PillarMarkId>()) {
				PillarMarkStateDictionary.Add(pillar_mark, PillarMarkState.inactive);
			}

			PillarStateDictionary = new Dictionary<PillarId, PillarState>();
			foreach (var pillar_id in Enum.GetValues(typeof(PillarId)).Cast<PillarId>()) {
				PillarStateDictionary.Add(pillar_id, PillarState.Locked);
			}

			if (Application.isEditor) {
				SetAbilityState(AbilityType.Echo, AbilityState.active);
			}

			PersistentDataDictionary = new Dictionary<string, PersistentData>();
			FireflyList = new List<Firefly>();
			GatheredFireflyCount = 0;
			ContinuedGame = false;
			GameController.PlayIntroCutscene = true;
		}

		//###########################################################

		// -- SAVE SYSTEM

		string dataFile = "/playerModel.dat";

		public void DeleteSave() {
			if (File.Exists(Application.persistentDataPath + dataFile))
				File.Delete(Application.persistentDataPath + dataFile);
			
			Init();
		}

		public void Save() {
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Create(Application.persistentDataPath + dataFile);

			PlayerData data = new PlayerData();

			// Save Player

			if (GameController.IsOpenWorldLoaded) {
				data.posX = GameController.PlayerController.Transform.position.x;
				data.posY = GameController.PlayerController.Transform.position.y;
				data.posZ = GameController.PlayerController.Transform.position.z;

				data.rotX = GameController.PlayerController.Transform.rotation.x;
				data.rotY = GameController.PlayerController.Transform.rotation.y;
				data.rotZ = GameController.PlayerController.Transform.rotation.z;
				data.rotW = GameController.PlayerController.Transform.rotation.w;
			} else {//else we use the pillar entrance position

				Vector3 pos = GameController.SpawnPointManager.GetPillarExitPoint(GameController.ActivePillarId, PillarVariant.Intact);
				Quaternion rot = GameController.SpawnPointManager.GetPillarExitOrientation(GameController.ActivePillarId, PillarVariant.Intact);

				data.posX = pos.x;
				data.posY = pos.y;
				data.posZ = pos.z;

				data.rotX = rot.x;
				data.rotY = rot.y;
				data.rotZ = rot.z;
				data.rotW = rot.w;
			}

			// Save Variables
			data.AbilityStateDictionary = AbilityStateDictionary;
			data.PillarMarkStateDictionary = PillarMarkStateDictionary;
			data.PillarStateDictionary = PillarStateDictionary;

			data.PersistentDataDictionary = PersistentDataDictionary;

			// Save Fireflies
			data.FireflyList = new string[FireflyList.Count];
			for (int i = 0; i < FireflyList.Count; i++) {
				data.FireflyList[i] = FireflyList[i].ownerID;
			}

			data.GatheredFireflyCount = GatheredFireflyCount;

			//
			
			bf.Serialize(file, data);
			file.Close();
		}

		public void Load() {
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + dataFile, FileMode.Open);
			PlayerData data = (PlayerData)bf.Deserialize(file);
			file.Close();

			//Load Variables here
			AbilityStateDictionary = data.AbilityStateDictionary;
			PillarMarkStateDictionary = data.PillarMarkStateDictionary;
			PillarStateDictionary = data.PillarStateDictionary;

			PersistentDataDictionary = data.PersistentDataDictionary;

			// Load Fireflies
			foreach (var fireflyID in data.FireflyList) {

				Firefly firefly = UnityEngine.Object.Instantiate(GameController.FireflyPrefab).GetComponent<Firefly>();

				firefly.SetParent(GameController.PlayerController.Transform, true, Firefly.FireflyState.Following);
				firefly.Transform.localPosition = Vector3.zero;
				firefly.SetID(fireflyID);

				firefly.SetFollowOffset();

				FireflyList.Add(firefly);
			}

			GatheredFireflyCount = data.GatheredFireflyCount;

			ContinuedGame = true;

			SpawnPosition = new Vector3(data.posX, data.posY, data.posZ);
			SpawnRotation = new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW);

			GameController.PlayIntroCutscene = false;
			
			// Place the player at the last activated waypoint
		}


		//###########################################################

		// -- INQUIRIES

		/// <summary>
		/// Returns the state of the ability.
		/// </summary>
		/// <param name="ability_type"></param>
		/// <returns></returns>
		public AbilityState GetAbilityState(AbilityType ability_type)
        {
            return AbilityStateDictionary[ability_type];
        }

        /// <summary>
        /// This method checks if an ability is currently activated.
        /// Returns true if the ability is activated, false otherwise.
        /// </summary>
        /// <param name="ability_type"></param>
        /// <returns></returns>
        public bool CheckAbilityActive(AbilityType ability_type)
        {
            return AbilityStateDictionary[ability_type] == AbilityState.active;
        }

        /// <summary>
        /// Returns the state of the pillar mark.
        /// </summary>
        /// <param name="pillar_mark_id"></param>
        /// <returns></returns>
        public PillarMarkState GetPillarMarkState(PillarMarkId pillar_mark_id)
        {
            return PillarMarkStateDictionary[pillar_mark_id];
        }

        /// <summary>
        /// Returns the amount of active pillar marks.
        /// </summary>
        /// <returns></returns>
        public int GetActivePillarMarkCount()
        {
            int result = 0;

            foreach (var pillar_mark in Enum.GetValues(typeof(PillarMarkId)).Cast<PillarMarkId>())
            {
                if (PillarMarkStateDictionary[pillar_mark] == PillarMarkState.active)
                {
                    result++;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the current state of the pillar.
        /// </summary>
        /// <param name="pillar_id"></param>
        /// <returns></returns>
        public PillarState GetPillarState(PillarId pillar_id)
        {
            return PillarStateDictionary[pillar_id];
        }

        /// <summary>
        /// Returns the amount of pillarkeys that need to be sacrificed in order to unlock a pillar.
        /// </summary>
        /// <param name="pillarId">The Id of the pillar to be unlocked.</param>
        /// <returns></returns>
        public int GetPillarEntryPrice(PillarId pillarId)
        {
            return LevelData.GetPillarSceneActivationCost(pillarId);
        }

        /// <summary>
        /// Gets the persistent data object with the id if it exists, null otherwise.
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public PersistentData GetPersistentDataObject(string uniqueId)
        {
            if (PersistentDataDictionary.ContainsKey(uniqueId))
            {
                return PersistentDataDictionary[uniqueId];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the persistent data object with the id if it exists and can be cast to T, null otherwise.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public T GetPersistentDataObject<T>(string uniqueId) where T : PersistentData
        {
            if (PersistentDataDictionary.ContainsKey(uniqueId))
            {
                var dataObject = GetPersistentDataObject(uniqueId);

                return dataObject as T;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetFireflyCount(bool total = false)
        {
            if (total)
            {
                return GatheredFireflyCount;
            }
            else
            {
                return FireflyList.Count;
            }
        }

        /// <summary>
        /// Returns a list of all Fireflies currently attached to the player.
        /// For access only!
        /// </summary>
        /// <returns></returns>
        public List<Firefly> GetFireflyList()
        {
            return new List<Firefly>(FireflyList);
        }

        //###########################################################

        // -- OPERATIONS

        /// <summary>
        /// Sets the state of an ability.
        /// </summary>
        public void SetAbilityState(AbilityType ability_type, AbilityState ability_state)
        {
            if (AbilityStateDictionary[ability_type] != ability_state)
            {
                AbilityStateDictionary[ability_type] = ability_state;

                EventManager.SendAbilityStateChangedEvent(this, new EventManager.AbilityStateChangedEventArgs(ability_type, ability_state));
            }
        }

        /// <summary>
        /// Sets the state of a pillar mark.
        /// </summary>
        /// <param name="pillar_mark_id"></param>
        /// <param name="pillar_mark_state"></param>
        public void SetPillarMarkState(PillarMarkId pillar_mark_id, PillarMarkState pillar_mark_state)
        {
            if (PillarMarkStateDictionary[pillar_mark_id] != pillar_mark_state)
            {
                PillarMarkStateDictionary[pillar_mark_id] = pillar_mark_state;

                EventManager.SendPillarMarkStateChangedEvent(this, new EventManager.PillarMarkStateChangedEventArgs(pillar_mark_id, pillar_mark_state, GetActivePillarMarkCount()));
			}
        }

        /// <summary>
        /// Sets the state of the pillar.
        /// </summary>
        /// <param name="pillar_id"></param>
        /// <param name="pillar_state"></param>
        public void SetPillarState(PillarId pillar_id, PillarState pillar_state)
        {
            if (PillarStateDictionary[pillar_id] != pillar_state)
            {
                PillarStateDictionary[pillar_id] = pillar_state;

                EventManager.SendPillarStateChangedEvent(this, new EventManager.PillarStateChangedEventArgs(pillar_id, pillar_state));
			}
        }

        /// <summary>
        /// Registers a new persistent data object. Returns true if the registration was successfull, false otherwise.
        /// </summary>
        /// <param name="dataObject"></param>
        /// <returns></returns>
        public bool AddPersistentDataObject(PersistentData dataObject)
        {
            if (PersistentDataDictionary.ContainsKey(dataObject.UniqueId))
            {
                if (PersistentDataDictionary[dataObject.UniqueId] == dataObject)
                {
                    return true;
                }
                else
                {
                    Debug.LogErrorFormat("Model: AddPersistentDataObject: An object with the id \"{0}\" already exists! currentObjectType={1}; newObjectType={2}",
                        dataObject.UniqueId,
                        PersistentDataDictionary[dataObject.UniqueId].GetType(),
                        dataObject.GetType()
                    );

                    return false;
                }
            }
            else
            {
                PersistentDataDictionary.Add(dataObject.UniqueId, dataObject);
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firefly"></param>
        public void PushFirefly(Firefly firefly)
        {
            if (!FireflyList.Contains(firefly))
            {
                GatheredFireflyCount++;
                FireflyList.Add(firefly);

				Save();
			}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Firefly PopFirefly()
        {
            if(FireflyList.Count == 0)
            {
                return null;
            }

            var result = FireflyList[0];
            FireflyList.RemoveAt(0);

			Save();

			return result;
        }
    }
} //end of namespace