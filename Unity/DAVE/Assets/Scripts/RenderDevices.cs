﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderDevices : MonoBehaviour
{

    public GameObject devicePrefab;
    public GameObject processPrefab;
    public TextMesh namePrefab;
    public static ArrayList Devices = new ArrayList();
    ArrayList DeviceNames = new ArrayList();
    public Material connnectionMaterial;
    public GameObject communication;
    GameObject df;
    GameObject dt;
    float delta;
    public Vector3 center;

    public void CreateDevices(JSONDeployment json)
    {
        // Create devices containing their processes
        int i;
        float yPos = 4.890001F;
        int biggest = 1;
        // Go through the parsed JSON
        foreach (var pair in json.Mapping)
        {
            i = DeviceNames.IndexOf(pair.Device);
            if (i == -1)
            {
                Devices.Add(new Device(pair.Device, pair.Process));
                DeviceNames.Add(pair.Device);
            }
            else
            {

                Device tmp = (Device)Devices[i];
                tmp.AddProcess(pair.Process);
                if(tmp.GetProcesses().Count > biggest)
                {
                    biggest++;
                    yPos += 1; 
                }
                Devices[i] = tmp;
            }
        }

        i = 0;
        center = new Vector3(0, yPos, 9.880002F);

        foreach (Device device in Devices)
        {
            //Place the devices in a circle
            Vector3 pos = PlaceInCircle(center, (float)(Devices.Count), (float)(360 / Devices.Count * i++));
            GameObject newDevice = (GameObject)Instantiate(
                devicePrefab,
                pos,
                this.transform.rotation);

            Vector3 nPos = pos;

            // Rezises the height of the device
            int length = device.GetProcesses().Count;
            if (length == 2)
            {
                newDevice.transform.localScale += new Vector3(0, 0, newDevice.transform.localScale.z);
            }
            else if(length > 6)
            {
                newDevice.transform.localScale += new Vector3(0, (float)(0.8 * 3), newDevice.transform.localScale.z);
            }
            else if(length > 2)
                newDevice.transform.localScale += new Vector3(0, (float)(0.8 * (int)((length) / 2)), newDevice.transform.localScale.z);
            nPos += new Vector3(0.5F, newDevice.transform.localScale.y / 2 - 0.5F, 0);
            newDevice.name = device.GetName();

            // Places the name of the device
            Quaternion rot = Quaternion.Euler(0, 270, 0);
            TextMesh name = (TextMesh)Instantiate(
                namePrefab,
                nPos,
                rot);
            name.text = "<<Device>>\n" + device.GetName();

            // Resizes the width of the device
            float width = GetWidth(name);
            if (width > (newDevice.transform.localScale.z))
                newDevice.transform.localScale += new Vector3(0, 0, width - newDevice.transform.localScale.z);

            // The position of the process
            if (length == 1)
                pos += new Vector3(0.1F, 0.8F * (length) / 2 - 0.6F, 0);
            else if (length == 2)
                pos += new Vector3(0.1F, 0.8F * (length -1) / 2 - 0.6F, -newDevice.transform.localScale.z / 4);
            else
                pos += new Vector3(0.1F, newDevice.transform.localScale.y / 2 - 1, - newDevice.transform.localScale.z/4);
            foreach (string process in device.GetProcesses())
            {

                //Creating the processes
                GameObject newProcess = (GameObject)Instantiate(
                    processPrefab,
                    pos,
                    this.transform.rotation
                    );
                newProcess.name = "proc:" + process;

                //TURN OFF THE LIGHT
                newProcess.GetComponentInChildren<Light>().intensity = 0;

                newProcess.GetComponentInChildren<TextMesh>().text =  ":" + process;

                if (device.GetProcesses().IndexOf(process) % 2 == 0)
                    pos.z += newDevice.transform.localScale.z / 5 * 2.5F;
                else
                {
                    // Update posistion for the next process
                    pos.y -= 0.8F;
                    pos.z -= newDevice.transform.localScale.z / 5 * 2.5F;
                }

            }
        }

    }
    //Places an object at a posistion in a circle 
    Vector3 PlaceInCircle(Vector3 center, float radius, float ang)
    {
        Vector3 pos;
        pos.x = center.x;
        pos.y = center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        pos.z = center.z + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
        return pos;

    }


    // Gets the width of the text
    float GetWidth(TextMesh mesh)
    {
        float width = 0;
        foreach (char symbol in mesh.text)
        {
            CharacterInfo info;
            if (mesh.font.GetCharacterInfo(symbol, out info, mesh.fontSize, mesh.fontStyle))
            {
                width += info.advance;
            }
        }
        return width * mesh.characterSize * 0.06f;
    }

}
