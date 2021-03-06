﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;

public class DiagramBroker : MonoBehaviour
{
    ConnectionManager coordinator = ConnectionManager.coordinator;
    // offset used to position house "districts"

    //Queues of received JSON strings.
    public static Queue<String> classDiagramQueue = new Queue<String>();
    public static Queue<String> deploymentDiagramQueue = new Queue<String>();
    public static Queue<String> sequenceDiagramQueue = new Queue<String>();


    public GameObject ssdSpawnerSpawner;
    private SSDInit ssdInit;
    private string topic;

    private void Start()
    {
        // Assign handler for handling the receiving messages
        coordinator.GetMqttClient().MqttMsgPublishReceived += Client_MqttMsgPublishReceived;

        topic = "root/" + coordinator.GetInstructor() + "/" + coordinator.GetRoom();

        // Subscribes to choosen intructor's room 
        coordinator.Subscribe(topic + "/class_diagram");

        // Subscribes to choosen intructor's room 
        coordinator.Subscribe(topic + "/deployment_diagram");
        
        // Subscribes to choosen intructor's room 
        coordinator.Subscribe(topic + "/sequence_diagram");

        // Gets SSD initiator
        ssdInit = ssdSpawnerSpawner.GetComponent<SSDInit>();
    }

    private void Update()
    {   //Dequeues and calls render functions for the different diagrams
        if (classDiagramQueue.Count > 0)
        {
            Debug.Log("Dequeued: " + classDiagramQueue.Peek());
            JsonParser parser = new JsonParser(classDiagramQueue.Dequeue());
            RenderClassDiagram(parser.ParseClass(), int.Parse(parser.GetMeta()));
        }
        else if (deploymentDiagramQueue.Count > 0)
        {
            Debug.Log("Dequeued: " + deploymentDiagramQueue.Peek());
            JsonParser parser = new JsonParser(deploymentDiagramQueue.Dequeue());
            RenderDeployment(parser.ParseDeployment(), int.Parse(parser.GetMeta()));
        }
        else if (sequenceDiagramQueue.Count > 0)
        {
            JsonParser parser = new JsonParser(sequenceDiagramQueue.Dequeue());
            var offset = int.Parse(parser.GetMeta());
            StartCoroutine(RenderDeploymentConnections(parser.ParseSequence(), offset));
            PlaceSSD(parser.ParseSequence(), int.Parse(parser.GetMeta()), parser.GetSSDRoom());
        }
    }


    // Handler that gets received messages from subscribed topics
    void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        Debug.Log("Received\r\n" + "Topic: " + e.Topic + "\r\n" + "Message: " +
            System.Text.Encoding.UTF8.GetString(e.Message));

        CheckReceived(e);
    }

    // Method checks if received message should be added to the queue that will be animated
    private void CheckReceived(MqttMsgPublishEventArgs e)
    {
        if (e.Topic == coordinator.GetParentTopic() + "/class_diagram")          // Checks topic
        {
            String payload = System.Text.Encoding.UTF8.GetString(e.Message);

            // Checks if payload is a valid JSON and of valid type
            if (IsValidJson(payload) && IsValidDiagramType(payload))
            {
                Debug.Log("Class diagram JSON received, verified and queued");
                classDiagramQueue.Enqueue(payload);             // Adds payload (JSON) to the queue
            }
        }
        else if (e.Topic == coordinator.GetParentTopic() + "/deployment_diagram")
        {
            String payload = System.Text.Encoding.UTF8.GetString(e.Message);
            Debug.Log("Deployment diagram JSON received, verified and queued");
            if (IsValidJson(payload) && IsValidDiagramType(payload))
            {
                deploymentDiagramQueue.Enqueue(payload);
            }
        }
        else if(e.Topic == coordinator.GetParentTopic() + "/sequence_diagram")
        {
            String payload = System.Text.Encoding.UTF8.GetString(e.Message);
            string[] array = payload.Split(' ');

            if (IsValidJson(payload) && IsValidDiagramType(payload))
            {
                sequenceDiagramQueue.Enqueue(payload);
            }
        }
    }

    public void PlaceSSD(JSONSequence JSONSeq, float offset, string room)
    {
        SSDInit init = ssdSpawnerSpawner.GetComponent<SSDInit>();
        init.SpawnSSDSpawner(offset, topic + "/sequence_diagram", room);
    }

    public void RenderClassDiagram(JSONClass JSONClass, float houseOffset)
    {
        string id = Guid.NewGuid().ToString("N");
        RenderClasses(JSONClass, id, houseOffset);
        RenderRelationships(JSONClass, id);
    }

    public void RenderDeployment(JSONDeployment JSONDeployment, float offset)
    {
        RenderDevices(JSONDeployment, offset);
    }

    private void RenderSystemBoxes(JSONSequence JSONSequence)
    {
        gameObject.GetComponent<RenderSystemBoxes>().CreateSystemBoxes(JSONSequence);
    }


    public void RenderClasses(JSONClass JSONClass, string id, float offset)
    {
        gameObject.GetComponent<RenderClasses>().AddHouse(JSONClass, id, offset);
    }

    public void RenderRelationships(JSONClass JSONClass, string id)
    {
        gameObject.GetComponent<RenderClassRelationship>().AddRelationship(JSONClass, id);
    }

    private void RenderDevices(JSONDeployment JSONDeployment, float offset)
    {
        gameObject.GetComponent<RenderDevices>().CreateDevices(JSONDeployment, offset);
    }

    // Waits 1.5 seconds before rendering deployment connections since FindDeploymentConnections
    // is depedant on that the deployment diagrams has been rendered before it executes
    private IEnumerator RenderDeploymentConnections(JSONSequence JSONSequence, float offset)
    {
        yield return new WaitForSeconds(1.5f);   
        gameObject.GetComponent<FindDeploymentConnections>().NewMessage(JSONSequence, offset);
    }


    // Source: https://goo.gl/n89LoF
    private bool IsValidJson(string strInput)
    {
        strInput = strInput.Trim();
        if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
            (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
        {
            try
            {
                var obj = JToken.Parse(strInput);
                obj.Equals(obj); //Could not suppress warning so this prevents the error message
                return true;
            }
            catch (JsonReaderException jex) //Exception in parsing json
            {
                Debug.Log(jex.Message);
                return false;
            }
            catch (Exception ex) //some other exception
            {
                Debug.Log(ex.ToString());
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    // Checks if the json is either a sequence, class or a deployment diagram
    private bool IsValidDiagramType(string json)
    {
        string[] allowedDiagramTypes = { "sequence_diagram", "class_diagram", "deployment_diagram" };
        string diagramType = new JsonParser(json).GetDiagramType(); // Gets the diagram type
        // Checks if allowedDiagramTypes contains diagram type of the string json
        if ((((IList<string>)allowedDiagramTypes).Contains(diagramType)))
            return true;
        return false;
    }
}