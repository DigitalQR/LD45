﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class RoomInstance : MonoBehaviour
{
	private Vector3Int m_Extents;
	private RoomConnections m_Connections;
	private Vector3 m_TopDoorLocation;
	private Vector3 m_BottomDoorLocation;
	private Vector3 m_LeftDoorLocation;
	private Vector3 m_RightDoorLocation;

	private GameObject m_EnemyContentContainer;
	private GameObject m_FloorContentContainer;
	private GameObject m_WallContentContainer;

	private FloorSettings m_FloorSettings;
	private RoomSettings m_RoomSettings;

	private void Start()
	{
		m_EnemyContentContainer.SetActive(true);
		m_FloorContentContainer.SetActive(true);
		m_WallContentContainer.SetActive(true);
	}

	private void SetupRoom(RoomGeneratorHelper generator, Vector3Int extents)
	{
		m_Extents = extents;

		GetComponent<MeshFilter>().mesh = generator.GeneratedMesh;
		GetComponent<MeshCollider>().sharedMesh = generator.GeneratedMesh;

		Vector2Int doorLoc;
		if (generator.GetDoor(RoomConnections.Top, out doorLoc))
			m_TopDoorLocation = new Vector3(doorLoc.x, m_RoomSettings.m_FloorHeight, doorLoc.y);
		if (generator.GetDoor(RoomConnections.Bottom, out doorLoc))
			m_BottomDoorLocation = new Vector3(doorLoc.x, m_RoomSettings.m_FloorHeight, doorLoc.y);
		if (generator.GetDoor(RoomConnections.Left, out doorLoc))
			m_LeftDoorLocation = new Vector3(doorLoc.x, m_RoomSettings.m_FloorHeight, doorLoc.y);
		if (generator.GetDoor(RoomConnections.Right, out doorLoc))
			m_RightDoorLocation = new Vector3(doorLoc.x, m_RoomSettings.m_FloorHeight, doorLoc.y);

		// Setup contains to make management easier for spawned stuff
		m_EnemyContentContainer = new GameObject();
		m_EnemyContentContainer.transform.parent = transform;
		m_EnemyContentContainer.SetActive(false);

		m_FloorContentContainer = new GameObject();
		m_FloorContentContainer.transform.parent = transform;
		m_FloorContentContainer.SetActive(false);

		m_WallContentContainer = new GameObject();
		m_WallContentContainer.transform.parent = transform;
		m_WallContentContainer.SetActive(false);

		ProcessPlacementSettings(m_EnemyContentContainer, generator, GetEnemyPlacementSettings(), true);
		ProcessPlacementSettings(m_FloorContentContainer, generator, GetLootPlacementSettings(), true);
		ProcessPlacementSettings(m_FloorContentContainer, generator, GetFloorDressingSettings(), true);
		ProcessPlacementSettings(m_WallContentContainer, generator, GetWallDressingSettings(), false);
	}

	private void ProcessPlacementSettings(GameObject targetContainer, RoomGeneratorHelper generator, PlacementSettings settings, bool useAccessibleSlots)
	{
		int placeCount = Random.Range(settings.m_MinPlacements, settings.m_MaxPlacements);

		for (int n = 0; n < placeCount; ++n)
		{
			bool hasSlots = useAccessibleSlots ? generator.AccessibleSpots.Any() : generator.InaccessibleSpots.Any();
			if (!hasSlots)
				return;

			GroupPlacementSettings groupSettings = settings.SelectRandomGroup();
			int objectCount = Random.Range(groupSettings.m_MinElements, groupSettings.m_MaxElements) + groupSettings.m_AlwaysPlacedObjects.Length;

			var spots = useAccessibleSlots ? generator.GetFreeAccessibleSpot(objectCount) : generator.GetFreeInaccessibleSpot(objectCount);

			for (int i = 0; i < spots.Count(); ++i)
			{
				GameObject targetObj;

				if (i < groupSettings.m_AlwaysPlacedObjects.Length)
					targetObj = groupSettings.m_AlwaysPlacedObjects[i];
				else
					targetObj = groupSettings.SelectRandomObject().m_PlacedObject;

				GameObject newObj = Instantiate(targetObj, transform.position + spots.ElementAt(i) - new Vector3(m_Extents.x, 0, m_Extents.z) * 0.5f, Quaternion.identity, targetContainer.transform);
			}
		}
	}

	private PlacementSettings GetEnemyPlacementSettings()
	{
		return m_RoomSettings.m_OverrideFloorEnemyPlacement ? m_RoomSettings.m_EnemyPlacements : m_FloorSettings.m_EnemyPlacements;
	}

	private PlacementSettings GetLootPlacementSettings()
	{
		return m_RoomSettings.m_OverrideFloorLootPlacement ? m_RoomSettings.m_LootPlacements : m_FloorSettings.m_LootPlacements;
	}

	private PlacementSettings GetWallDressingSettings()
	{
		return m_RoomSettings.m_OverrideFloorWallDressingPlacement ? m_RoomSettings.m_WallDressingPlacements : m_FloorSettings.m_WallDressingPlacements;
	}

	private PlacementSettings GetFloorDressingSettings()
	{
		return m_RoomSettings.m_OverrideFloorFloorDressingPlacement ? m_RoomSettings.m_FloorDressingPlacements : m_FloorSettings.m_FloorDressingPlacements;
	}

	public static RoomInstance PlaceRoom(RoomInstance prefab, FloorSettings floorSettings, RoomSettings roomSettings, Vector3Int location, Vector3Int extents, RoomConnections connection)
	{
		RoomInstance instance = Instantiate(prefab, location, Quaternion.identity);
		instance.m_RoomSettings = roomSettings;
		instance.m_FloorSettings = floorSettings;

		RoomGeneratorHelper generator = new RoomGeneratorHelper(roomSettings, extents, connection);
		instance.SetupRoom(generator, extents);
		return instance;
	}

	public bool HasTopDoor
	{
		get { return (m_Connections & RoomConnections.Top) != 0; }
	}

	public bool HasBottomDoor
	{
		get { return (m_Connections & RoomConnections.Bottom) != 0; }
	}

	public bool HasLeftDoor
	{
		get { return (m_Connections & RoomConnections.Left) != 0; }
	}

	public bool HasRightDoor
	{
		get { return (m_Connections & RoomConnections.Right) != 0; }
	}
}