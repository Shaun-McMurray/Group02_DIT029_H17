﻿using System.Collections.Generic;
using UnityEngine;

public class RenderSystemBox : MonoBehaviour {

	public GameObject lTarget;
	public GameObject lineSegmentPrefab;
	float cooldown = 0.5f;
	float cooldownRemaining = 0;
	GameObject target;

    private Light systemLight;
    public bool lightSwitch;
    public List<GameObject> lifeLine;
    float counter = 0;
    public bool activeLifeLine;
    // Use this for initialization
    void Start () {

        activeLifeLine = true;
        lightSwitch = false;
        systemLight = GetComponent<Light>();
        systemLight.enabled = false;
		Vector3 v = new Vector3(this.transform.position.x, transform.parent.transform.position.y, 
            this.transform.position.z);
		target = (GameObject)Instantiate(lTarget, v, this.transform.rotation);
		
	}
	
	// Update is called once per frame
	void Update () {

        if (lightSwitch == false && counter > 200) { pauseLifeline(); }
        else
        {
            if (activeLifeLine == false) { startLifeline(); }
            counter++;
            cooldownRemaining -= Time.deltaTime;
            if (cooldownRemaining <= 0)
            {
                cooldownRemaining = cooldown;
                newLifeLine(target);
            }
        }
        if (lightSwitch == false)
        {
            systemLight.enabled = false;
        }
        else
        {
            systemLight.enabled = true;
        }
    }

	void newLifeLine(GameObject t){

		GameObject lifeLineGO = (GameObject)Instantiate(lineSegmentPrefab, this.transform.position, 
            this.transform.rotation);
		RenderLifeline l = lifeLineGO.GetComponent<RenderLifeline>();
		l.target = t.transform;
        l.parentSystem = this.gameObject;
        lifeLine.Add(lifeLineGO);
	}
    void pauseLifeline(){
        foreach (GameObject segment in lifeLine){
            segment.GetComponent<RenderLifeline>().pause = true;
            activeLifeLine = false;
        }
    }
    void startLifeline(){
        foreach (GameObject segment in lifeLine){
            segment.GetComponent<RenderLifeline>().pause = false;
            activeLifeLine = true;
        }
    }
}
