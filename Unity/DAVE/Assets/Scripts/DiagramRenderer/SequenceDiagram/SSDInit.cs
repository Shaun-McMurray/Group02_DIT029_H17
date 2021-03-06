﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SSDInit : MonoBehaviour {

	public GameObject player;
	public GameObject ssdSpawnerPrefab;
	public bool spawnSpawner;
	public string room;
	public int size;
	public int offset;

	public void SpawnSSDSpawner (float offset, string SSDroom, string uniqueKey) {

		Vector3 pos = new Vector3(-110, 10, offset + 30);
		Vector3 rot = new Vector3(0, 0, 0);

		GameObject SSDGO = (GameObject)Instantiate(
                    ssdSpawnerPrefab,
                    pos,
                	Quaternion.Euler(new Vector3(0, 0, 0))
                );

		SSDSpawner spawner = SSDGO.GetComponent<SSDSpawner>();
		spawner.room = SSDroom + uniqueKey;
		spawner.size = size;
		Debug.Log(SSDroom);
	}
}
